using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ServiceLibrary.Helpers;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;

namespace ServiceLibrary.Repositories
{
    public class SqlArtifactRepository : ISqlArtifactRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;
        private readonly ISqlItemInfoRepository _itemInfoRepository;
        private readonly IArtifactPermissionsRepository _artifactPermissionsRepository;

        public SqlArtifactRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        public SqlArtifactRepository(ISqlConnectionWrapper connectionWrapper)
            : this(connectionWrapper, new SqlItemInfoRepository(connectionWrapper),
                  new SqlArtifactPermissionsRepository(connectionWrapper))
        {
        }

        public SqlArtifactRepository(ISqlConnectionWrapper connectionWrapper,
            SqlItemInfoRepository itemInfoRepository,
            IArtifactPermissionsRepository artifactPermissionsRepository)
        {
            _connectionWrapper = connectionWrapper;
            _itemInfoRepository = itemInfoRepository;
            _artifactPermissionsRepository = artifactPermissionsRepository;
        }

        #region GetProjectOrArtifactChildrenAsync

        public virtual async Task<List<Artifact>> GetProjectOrArtifactChildrenAsync(int projectId, int? artifactId, int userId)
        {
            if (projectId < 1)
                throw new ArgumentOutOfRangeException(nameof(projectId));
            if (artifactId < 1)
                throw new ArgumentOutOfRangeException(nameof(artifactId));
            if (userId < 1)
                throw new ArgumentOutOfRangeException(nameof(userId));

            // We do not treat the project as the artifact
            if (artifactId == projectId)
                ThrowNotFoundException(projectId, artifactId);

            var prm = new DynamicParameters();
            prm.Add("@projectId", projectId);
            prm.Add("@artifactId", artifactId ?? projectId);
            prm.Add("@userId", userId);

            var artifactVersions = (await _connectionWrapper.QueryAsync<ArtifactVersion>("GetArtifactChildren", prm,
                    commandType: CommandType.StoredProcedure)).ToList();

            // The artifact or the project is not found
            if (!artifactVersions.Any())
                ThrowNotFoundException(projectId, artifactId);

            var dicUserArtifactVersions = artifactVersions.GroupBy(v => v.ItemId).ToDictionary(g => g.Key, g => GetUserArtifactVersion(g.ToList()));

            // For the project and the roots of collections and baselines/reviews only, get orphan artifacts
            ArtifactVersion av;
            ProjectSection? projectSection;
            dicUserArtifactVersions.TryGetValue(artifactId ?? projectId, out av);

            // Bug 4357: Required for adjusting property hasChildren of Collections root  
            var hasCollectionsSectionOrphans = false;
            var hasBaselinesAndReviewsSectionOrphans = false;

            if (av != null && TryGetProjectSectionFromRoot(av, out projectSection))
            {
                prm = new DynamicParameters();
                prm.Add("@projectId", projectId);
                prm.Add("@userId", userId);

                var orphanVersions = (await _connectionWrapper.QueryAsync<ArtifactVersion>("GetProjectOrphans", prm,
                    commandType: CommandType.StoredProcedure)).ToList();

                if (orphanVersions.Any())
                {
                    var dicUserOrphanVersions = orphanVersions.GroupBy(v => v.ItemId).ToDictionary(g => g.Key, g => GetUserArtifactVersion(g.ToList()));

                    foreach (var userOrphanVersion in dicUserOrphanVersions.Values.Where(v => v?.ParentId == projectId))
                    {
                        // Replace with the corrected ParentId
                        if (dicUserArtifactVersions.ContainsKey(userOrphanVersion.ItemId))
                            dicUserArtifactVersions.Remove(userOrphanVersion.ItemId);

                        // Bug 4357
                        if (userOrphanVersion.ItemTypePredefined != null)
                        {
                            hasCollectionsSectionOrphans |=
                                userOrphanVersion.ItemTypePredefined.Value.IsCollectionsGroupType();
                            hasBaselinesAndReviewsSectionOrphans |=
                                userOrphanVersion.ItemTypePredefined.Value.IsBaselinesAndReviewsGroupType();
                        }

                        // Add the orphan with children belonging to the respective project section
                        if (BelongsToProjectSection(userOrphanVersion, projectSection.GetValueOrDefault()))
                        {
                            userOrphanVersion.ParentId = av.ItemId;
                            dicUserArtifactVersions.Add(userOrphanVersion.ItemId, userOrphanVersion);
                            foreach (
                                var userOrphanChildVersion in
                                    dicUserOrphanVersions.Values.Where(v => v?.ParentId == userOrphanVersion.ItemId))
                            {
                                if (dicUserArtifactVersions.ContainsKey(userOrphanChildVersion.ItemId))
                                    continue;

                                dicUserArtifactVersions.Add(userOrphanChildVersion.ItemId, userOrphanChildVersion);
                            }
                        }
                    }
                }
            }

            ArtifactVersion parentUserArtifactVersion;
            dicUserArtifactVersions.TryGetValue(artifactId ?? projectId, out parentUserArtifactVersion);

            // The artifact or the project is not found
            if (parentUserArtifactVersion == null)
            {
                ThrowNotFoundException(projectId, artifactId);
            }

            // The artifact or the project has the direct permissions without Read
            if (parentUserArtifactVersion.DirectPermissions.HasValue
                    && !parentUserArtifactVersion.DirectPermissions.GetValueOrDefault().HasFlag(RolePermissions.Read))
            {
                ThrowForbiddenException(projectId, artifactId);
            }

            if (!parentUserArtifactVersion.DirectPermissions.HasValue)
            {
                var ancestorUserVersion = FindAncestorOrProjectUserVersionWithDirectPermissions(dicUserArtifactVersions, parentUserArtifactVersion, projectId);
                if (ancestorUserVersion != null)
                {
                    parentUserArtifactVersion.EffectivePermissions = ancestorUserVersion.DirectPermissions;
                }
            }
            else
            {
                parentUserArtifactVersion.EffectivePermissions = parentUserArtifactVersion.DirectPermissions;
            }

            // The artifact or the project effective permissions does not have Read
            if (!parentUserArtifactVersion.EffectivePermissions.GetValueOrDefault().HasFlag(RolePermissions.Read))
            {
                ThrowForbiddenException(projectId, artifactId);
            }

            var userArtifactVersionChildren = ProcessChildren(dicUserArtifactVersions, parentUserArtifactVersion);

            var maxIndexOrder = userArtifactVersionChildren.Any()
                ? userArtifactVersionChildren.Max(a => a.OrderIndex)
                : 0;

            // Bug 4357: If the project (the Collections root is a child of the project) and there are orphans in Collection section,
            if (artifactId == null)
            {
                // set property hasChildren of the Collections root to true
                if (hasCollectionsSectionOrphans)
                {
                    var collectionsRoot = FindRoot(ItemTypePredefined.CollectionFolder, userArtifactVersionChildren, projectId);
                    if (collectionsRoot != null)
                        collectionsRoot.HasChildren = true;
                }
                // set property hasChildren of the Baselines and Reviews root to true
                if (hasBaselinesAndReviewsSectionOrphans)
                {
                    var baselinesAndReviewsRoot = FindRoot(ItemTypePredefined.BaselineFolder, userArtifactVersionChildren, projectId);
                    if (baselinesAndReviewsRoot != null)
                        baselinesAndReviewsRoot.HasChildren = true;
                }
            }

            return userArtifactVersionChildren.Select(v => new Artifact
            {
                Id = v.ItemId,
                Name = v.Name,
                ProjectId = v.VersionProjectId,
                ParentId = v.ParentId,
                ItemTypeId = GetItemTypeId(v),
                Prefix = v.Prefix,
                ItemTypeIconId = v.ItemTypeIconId,
                PredefinedType = v.ItemTypePredefined.GetValueOrDefault(),
                Version = v.VersionsCount,
                OrderIndex = v.OrderIndex,
                HasChildren = v.HasChildren,
                Permissions = v.EffectivePermissions,
                LockedByUser = v.LockedByUserId.HasValue ? new UserGroup { Id = v.LockedByUserId } : null,
                LockedDateTime = v.LockedByUserTime
            })
            .OrderBy(a => {
                // To put Collections and Baselines and Reviews folder at the end of the project children 
                if (a.OrderIndex >= 0)
                    return a.OrderIndex;
                if (a.OrderIndex < 0 && a.PredefinedType == ItemTypePredefined.CollectionFolder)
                    return maxIndexOrder + 1; // Collections folder comes after artifacts
                if (a.OrderIndex < 0 && a.PredefinedType == ItemTypePredefined.BaselineFolder)
                    return maxIndexOrder + 2; // Baseline and Reviews folder comes after Collections folder

                Debug.Assert(false, "Illegal Order Index: " + a.OrderIndex);
                return double.MaxValue;
            }).ToList();
        }

        private ArtifactVersion FindRoot(ItemTypePredefined rootType, IEnumerable<ArtifactVersion> artifacts, int projectId)
        {
            return artifacts.FirstOrDefault(
                        av => av?.ItemTypePredefined != null && av.ParentId == projectId
                        && av.ItemTypePredefined.Value == rootType);
        }

        private bool BelongsToProjectSection(ArtifactVersion artifactVersion, ProjectSection projectSection)
        {
            var itp = artifactVersion.ItemTypePredefined.GetValueOrDefault();
            switch (projectSection)
            {
                case ProjectSection.Artifacts:
                    return itp.IsRegularArtifactType();
                case ProjectSection.Collections:
                    return itp.IsCollectionsGroupType();
                case ProjectSection.BaselinesAndReviews:
                    return itp.IsBaselinesAndReviewsGroupType();
                default:
                    return false;
            }
        }

        private bool TryGetProjectSectionFromRoot(ArtifactVersion artifactVersion, out ProjectSection? projectSection)
        {
            projectSection = null;
            if (artifactVersion == null)
                return false;

            if (artifactVersion.ItemTypePredefined == ItemTypePredefined.Project)
            {
                projectSection = ProjectSection.Artifacts;
                return true;
            }
            
            // Collections and Baselines/Reviews roots are under the project
            if (artifactVersion.ParentId != artifactVersion.VersionProjectId)
                return false;
            if (artifactVersion.ItemTypePredefined == ItemTypePredefined.CollectionFolder)
            {
                projectSection = ProjectSection.Collections;
                return true;
            }
            if (artifactVersion.ItemTypePredefined == ItemTypePredefined.BaselineFolder)
            {
                projectSection = ProjectSection.BaselinesAndReviews;
                return true;
            }

            return false;
        }

        // Returns stub ItemTypeId for Collections and Baselines and Reviews folders under the project.
        private static int? GetItemTypeId(ArtifactVersion av)
        {
            if (av.ParentId != av.VersionProjectId)
                return av.ItemTypeId;

            switch (av.ItemTypePredefined)
            {
                case ItemTypePredefined.CollectionFolder:
                    return ServiceConstants.StubCollectionsItemTypeId;
                case ItemTypePredefined.BaselineFolder:
                    return ServiceConstants.StubBaselinesAndReviewsItemTypeId;
                default:
                    return av.ItemTypeId;
            }
        }

        private List<ArtifactVersion> ProcessChildren(Dictionary<int, ArtifactVersion> dicUserArtifactVersions, ArtifactVersion parentUserArtifactVersion)
        {
            var children = FindVisibleChildren(dicUserArtifactVersions, parentUserArtifactVersion);
            children.ForEach(v =>
            {
                v.HasChildren = FindVisibleChildren(dicUserArtifactVersions, v).Count > 0;
                v.EffectivePermissions = v.DirectPermissions ?? parentUserArtifactVersion.EffectivePermissions;
            });
            return children;
        }

        private List<ArtifactVersion> FindVisibleChildren(Dictionary<int, ArtifactVersion> dicUserArtifactVersions, ArtifactVersion parentUserArtifactVersion)
        {
            return dicUserArtifactVersions.Values.
                Where(v => v?.ParentId == parentUserArtifactVersion.ItemId
                            && (v.DirectPermissions == null || v.DirectPermissions.Value.HasFlag(RolePermissions.Read))).ToList();
        }

        private ArtifactVersion FindAncestorOrProjectUserVersionWithDirectPermissions(Dictionary<int, ArtifactVersion> dicUserArtifactVersions, ArtifactVersion parentUserVersion, int projectId)
        {
            if (parentUserVersion?.ParentId == null)
                return null;

            ArtifactVersion directAncestorUserVersion;
            dicUserArtifactVersions.TryGetValue(parentUserVersion.ParentId.GetValueOrDefault(), out directAncestorUserVersion);

            if (directAncestorUserVersion == null)
            {
                ArtifactVersion projectUserArtifactVersion;
                dicUserArtifactVersions.TryGetValue(projectId, out projectUserArtifactVersion);
                return projectUserArtifactVersion?.DirectPermissions != null ? projectUserArtifactVersion : null;
            }

            return directAncestorUserVersion.DirectPermissions != null
                ? directAncestorUserVersion
                : FindAncestorUserVersionWithDirectPermissions(dicUserArtifactVersions, directAncestorUserVersion);
        }

        private ArtifactVersion FindAncestorUserVersionWithDirectPermissions(Dictionary<int, ArtifactVersion> dicUserArtifactVersions, ArtifactVersion userArtifactVersion)
        {
            if (userArtifactVersion?.ParentId == null)
                return null;

            ArtifactVersion ancestorUserVersion;
            dicUserArtifactVersions.TryGetValue(userArtifactVersion.ParentId.GetValueOrDefault(), out ancestorUserVersion);

            return ancestorUserVersion.DirectPermissions != null
                ? ancestorUserVersion
                : FindAncestorUserVersionWithDirectPermissions(dicUserArtifactVersions, ancestorUserVersion);
        }

        private ArtifactVersion GetUserArtifactVersion(List<ArtifactVersion> headAndDraft)
        {
            if (headAndDraft == null)
                throw new ArgumentNullException(nameof(headAndDraft));

            if (headAndDraft.Count == 0)
                return null;

            Debug.Assert(headAndDraft.Count < 3, "More than 2 version for Head and Draft collection: " + headAndDraft.Count);
            Debug.Assert(headAndDraft.Count != 2
                || (headAndDraft[0].ItemId == headAndDraft[1].ItemId && headAndDraft[0].HasDraft == headAndDraft[1].HasDraft),
                "ItemId or HasDraft properties of Head and Draft are different.");

            var headOrDraft = headAndDraft[0].HasDraft
                ? headAndDraft.FirstOrDefault(v => v.StartRevision == ServiceConstants.VersionDraft && v.EndRevision != ServiceConstants.VersionDraftDeleted)
                : headAndDraft.FirstOrDefault(v => v.EndRevision == ServiceConstants.VersionHead);

            // Adjust the versions count
            if (headOrDraft?.VersionsCount != null)
                headOrDraft.VersionsCount /= headAndDraft.Count;

            return headOrDraft;
        }

        private static void ThrowNotFoundException(int projectId, int? artifactId)
        {
            var errorMessage = artifactId == null
                ? I18NHelper.FormatInvariant("The project (Id:{0}) can no longer be accessed. It may have been deleted, or is no longer accessible by you.", projectId)
                : I18NHelper.FormatInvariant("Artifact (Id:{0}) in Project (Id:{1}) is not found.", artifactId, projectId);
            throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
        }

        private static void ThrowForbiddenException(int projectId, int? artifactId)
        {
            var errorMessage = artifactId == null
                ? I18NHelper.FormatInvariant("User does not have permissions for Project (Id:{0}).", projectId)
                : I18NHelper.FormatInvariant("User does not have permissions for Artifact (Id:{0}).", artifactId);
            throw new AuthorizationException(errorMessage, ErrorCodes.UnauthorizedAccess);
        }

        #endregion GetProjectOrArtifactChildrenAsync

        #region GetSubArtifactTreeAsync

        private async Task<IEnumerable<SubArtifact>> GetSubArtifacts(int artifactId, int userId, int revisionId, bool includeDrafts)
        {
            var getSubArtifactsDraftPrm = new DynamicParameters();
            getSubArtifactsDraftPrm.Add("@artifactId", artifactId);
            getSubArtifactsDraftPrm.Add("@userId", userId);
            getSubArtifactsDraftPrm.Add("@revisionId", revisionId);
            getSubArtifactsDraftPrm.Add("@includeDrafts", includeDrafts);
            return (await _connectionWrapper.QueryAsync<SubArtifact>("GetSubArtifacts", getSubArtifactsDraftPrm, commandType: CommandType.StoredProcedure));
        }

        public async Task<IEnumerable<SubArtifact>> GetSubArtifactTreeAsync(int artifactId, int userId, int revisionId = int.MaxValue, bool includeDrafts = true)
        {
            var subArtifactsDictionary = (await GetSubArtifacts(artifactId, userId, revisionId, includeDrafts)).ToDictionary(a => a.Id);
            var itemIds = subArtifactsDictionary.Select(a => a.Key).ToList();
            var itemDetailsDictionary = (await _itemInfoRepository.GetItemsDetails(userId, itemIds, true, int.MaxValue)).ToDictionary(a => a.HolderId);
            foreach (var subArtifactEntry in subArtifactsDictionary)
            {
                var subArtifact = subArtifactEntry.Value;
                var parentSubArtifactId = subArtifact.ParentId;
                SubArtifact parentSubArtifact;
                if (parentSubArtifactId != artifactId && subArtifactsDictionary.TryGetValue(parentSubArtifactId, out parentSubArtifact))
                {
                    if (parentSubArtifact.Children == null)
                    {
                        parentSubArtifact.Children = new List<SubArtifact>();
                    }
                    ((List<SubArtifact>)parentSubArtifact.Children).Add(subArtifact);
                    parentSubArtifact.HasChildren = true;
                }
                ItemDetails itemDetails;
                if (itemDetailsDictionary!= null && itemDetailsDictionary.TryGetValue(subArtifact.Id, out itemDetails))
                {
                    subArtifactEntry.Value.Prefix = itemDetails.Prefix;
                }
            }
            var isUseCase = subArtifactsDictionary.Any() && (subArtifactsDictionary.ElementAt(0).Value.PredefinedType == ItemTypePredefined.PreCondition
                                        || subArtifactsDictionary.ElementAt(0).Value.PredefinedType == ItemTypePredefined.PostCondition
                                        || subArtifactsDictionary.ElementAt(0).Value.PredefinedType == ItemTypePredefined.Flow
                                        || subArtifactsDictionary.ElementAt(0).Value.PredefinedType == ItemTypePredefined.Step);

            if (isUseCase) {
                var itemLabelsDictionary = (await _itemInfoRepository.GetItemsLabels(userId, itemIds)).ToDictionary(a => a.ItemId);
                foreach (var subArtifactEntry in subArtifactsDictionary) {
                    //filter out flow subartifacts and append children of flow to children of flow's parent.
                    if (subArtifactEntry.Value.PredefinedType == ItemTypePredefined.Flow)
                    {
                        SubArtifact parent;
                        var children = subArtifactEntry.Value.Children;
                        if (subArtifactsDictionary.TryGetValue(subArtifactEntry.Value.ParentId, out parent))
                        {
                            ((List<SubArtifact>)parent.Children).Remove(subArtifactEntry.Value);
                            ((List<SubArtifact>)parent.Children).AddRange(children);
                        }
                    }
                    //populate label as display names.
                    ItemLabel itemLabel;
                    if (itemLabelsDictionary != null && itemLabelsDictionary.TryGetValue(subArtifactEntry.Value.Id, out itemLabel))
                    {
                        subArtifactEntry.Value.DisplayName = itemLabel.Label;
                    }
                }
            }
            var result = subArtifactsDictionary.Where(a => a.Value.ParentId == artifactId).Select(b => b.Value);
            return result;
        }

        #endregion GetSubArtifactTreeAsync

        #region GetExpandedTreeToArtifactAsync

        public virtual async Task<List<Artifact>> GetExpandedTreeToArtifactAsync(int projectId, int expandedToArtifactId, bool includeChildren, int userId)
        {
            if (projectId < 1)
                throw new ArgumentOutOfRangeException(nameof(projectId));
            if (expandedToArtifactId < 1)
                throw new ArgumentOutOfRangeException(nameof(expandedToArtifactId));
            if (userId < 1)
                throw new ArgumentOutOfRangeException(nameof(userId));

            // We do not treat the project as the artifact
            if (expandedToArtifactId == projectId)
                return await GetProjectOrArtifactChildrenAsync(projectId, null, userId);

            var prm = new DynamicParameters();
            prm.Add("@projectId", projectId);
            prm.Add("@artifactId", expandedToArtifactId);
            prm.Add("@userId", userId);

            // One of the return items is supposed to be the project
            var ancestorsAndSelfIds = (await
                _connectionWrapper.QueryAsync<ArtifactVersion>("GetArtifactAncestorsAndSelf", prm,
                    commandType: CommandType.StoredProcedure)).Select(av => av.ItemId).ToList();

            var setAncestorsAndSelfIds = new HashSet<int>(ancestorsAndSelfIds);

            if (!setAncestorsAndSelfIds.Any())
                ThrowNotFoundException(projectId, expandedToArtifactId);

            if (!includeChildren)
                setAncestorsAndSelfIds.Remove(expandedToArtifactId);

            var rootArtifacts = await GetProjectOrArtifactChildrenAsync(projectId, null, userId);

            bool isFetched = await AddChildrenToAncestors(rootArtifacts, setAncestorsAndSelfIds, projectId, expandedToArtifactId, userId);
            if (!isFetched && !setAncestorsAndSelfIds.Contains(projectId))
            {
                // Could be an orphan in the Collections or Baselines and Reviews hierarchy
                var rootCollections = rootArtifacts.Where(a => a.PredefinedType == ItemTypePredefined.CollectionFolder).Take(1);
                var rootBaselinesAndReviews = rootArtifacts.Where(a => a.PredefinedType == ItemTypePredefined.BaselineFolder).Take(1);
                foreach (var root in rootCollections.Union(rootBaselinesAndReviews))
                {
                    var children = await GetProjectOrArtifactChildrenAsync(projectId, root.Id, userId);
                    if (children.Any(child => child.Id == expandedToArtifactId))
                    {
                        root.Children = children.Cast<IArtifact>().ToList();
                        isFetched = true;
                        break;
                    }
                }
            }

            if (!isFetched)
            {
                ThrowForbiddenException(projectId, expandedToArtifactId);
            }

            return rootArtifacts;
        }

        private async Task<bool> AddChildrenToAncestors(IList<Artifact> siblings, HashSet<int> ancestorsAndSelfIds, int projectId, int expandedToArtifactId, int userId)
        {
            var isArtifactToExpandToFetched = false;
            while (true)
            {
                isArtifactToExpandToFetched |= siblings.Any(a => a.Id == expandedToArtifactId);

                var ancestor = siblings.FirstOrDefault(a => ancestorsAndSelfIds.Contains(a.Id));
                if (ancestor == null)
                {
                    return isArtifactToExpandToFetched;
                }
                siblings = await GetProjectOrArtifactChildrenAsync(projectId, ancestor.Id, userId);
                ancestor.Children = siblings.Cast<IArtifact>().ToList();
            }
        }

        #endregion GetExpandedTreeToArtifactAsync

        #region GetArtifactNavigationPathAsync

        public async Task<List<Artifact>> GetArtifactNavigationPathAsync(int artifactId, int userId)
        {
            if (artifactId < 1)
                throw new ArgumentOutOfRangeException(nameof(artifactId));
            if (userId < 1)
                throw new ArgumentOutOfRangeException(nameof(userId));

            var prm = new DynamicParameters();
            prm.Add("@userId", userId);
            prm.Add("@itemId", artifactId);
            var artifactBasicDetails = (await _connectionWrapper.QueryAsync<ArtifactBasicDetails>(
                "GetArtifactBasicDetails", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
            if (artifactBasicDetails == null || artifactBasicDetails.LatestDeleted)
            {
                var errorMessage = I18NHelper.FormatInvariant("Item (Id:{0}) is not found.", artifactId);
                throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
            }
            var itemIdsPermissions = (await _artifactPermissionsRepository.GetArtifactPermissions(new [] { artifactId }, userId));
            if (!itemIdsPermissions.ContainsKey(artifactId) || !itemIdsPermissions[artifactId].HasFlag(RolePermissions.Read))
            {
                var errorMessage = I18NHelper.FormatInvariant("User does not have permissions for Artifact (Id:{0}).", artifactId);
                throw new AuthorizationException(errorMessage, ErrorCodes.UnauthorizedAccess);
            }

            prm = new DynamicParameters();
            prm.Add("@artifactId", artifactId);
            prm.Add("@userId", userId);

            var ancestorsAndSelf =  (await _connectionWrapper.QueryAsync<ArtifactVersion>("GetArtifactNavigationPath", prm, commandType: CommandType.StoredProcedure))
                .ToList();
            return OrderAncestors(ancestorsAndSelf, artifactId).Select(a => new Artifact
            {
                Id = a.ItemId,
                Name = a.Name,
                ProjectId = a.VersionProjectId,
                ItemTypeId = a.ItemTypeId
            }).ToList();
        }

        // This method does not return the self.
        private static IEnumerable<ArtifactVersion> OrderAncestors(List<ArtifactVersion> ancestorsAndSelf, int artifactId)
        {
            var result = new List<ArtifactVersion>();
            if (ancestorsAndSelf == null || !ancestorsAndSelf.Any())
                return result;

            var dicAncestorsAndSelf = ancestorsAndSelf.ToDictionary(a => a.ItemId);
            int? childId = artifactId;
            ArtifactVersion child;

            while (childId.HasValue && dicAncestorsAndSelf.TryGetValue(childId.Value, out child))
            {
                result.Add(child);
                childId = child.ParentId;
            }
            result.RemoveAt(0);
            result.Reverse();
            return result;
        }

        #endregion GetArtifactNavigationPathAsync

        #region GetArtifactsNavigationPathsAsync

        public async Task<IDictionary<int, IEnumerable<string>>> GetArtifactsNavigationPathsAsync(
            int userId,
            IEnumerable<int> artifactIds,
            bool includeArtifactItself = true,
            int? revisionId = null,
            bool addDraft = true)
        {
            if (artifactIds == null)
            {
                throw new ArgumentOutOfRangeException(nameof(artifactIds));
            }

            var param = new DynamicParameters();
            param.Add("@userId", userId);
            param.Add("@artifactIds", SqlConnectionWrapper.ToDataTable(artifactIds, "Int32Collection", "Int32Value"));
            param.Add("@revisionId", revisionId ?? int.MaxValue);
            param.Add("@addDrafts", addDraft);

            var itemPaths = (await _connectionWrapper.QueryAsync<ArtifactsNavigationPath>("GetArtifactsNavigationPaths", param, commandType: CommandType.StoredProcedure)).ToList();

            var artifactNavigationPaths = new Dictionary<int, IDictionary<int, string>>();

            foreach (var artifactsNavigationPath in itemPaths)
            {
                if(!includeArtifactItself && artifactsNavigationPath.Level == 0)
                    continue;

                IDictionary<int, string> pathArray;
                if (!artifactNavigationPaths.TryGetValue(artifactsNavigationPath.ArtifactId, out pathArray))
                {
                    pathArray = new Dictionary<int, string>
                    {
                        {artifactsNavigationPath.Level, artifactsNavigationPath.Name}
                    };
                    artifactNavigationPaths.Add(artifactsNavigationPath.ArtifactId, pathArray);
                }
                else
                {
                    pathArray.Add(artifactsNavigationPath.Level, artifactsNavigationPath.Name);
                }
            }

            var result = new Dictionary<int, IEnumerable<string>>(artifactNavigationPaths.Count);

            foreach (var entry in artifactNavigationPaths)
            {
                result.Add(entry.Key, entry.Value.OrderByDescending(i => i.Key).Select(j => j.Value));
            }

            return result;
        }

        #endregion GetArtifactsNavigationPathsAsync

        #region GetProjectNameByIdsAsync

        public async Task<IEnumerable<ProjectNameIdPair>> GetProjectNameByIdsAsync(IEnumerable<int> projectIds)
        {
            var param = new DynamicParameters();
            param.Add("@projectIds", SqlConnectionWrapper.ToDataTable(projectIds, "Int32Collection", "Int32Value"));
            
            return (await _connectionWrapper.QueryAsync<ProjectNameIdPair>("GetProjectNameByIds", param, commandType: CommandType.StoredProcedure));            
        }

        #endregion GetProjectNameByIdsAsync
    }
}
