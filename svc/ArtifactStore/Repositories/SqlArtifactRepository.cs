using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Models;
using ServiceLibrary.Repositories;
using ServiceLibrary.Helpers;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Models;

namespace ArtifactStore.Repositories
{
    public class SqlArtifactRepository : ISqlArtifactRepository
    {
        internal readonly ISqlConnectionWrapper ConnectionWrapper;

        public SqlArtifactRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal SqlArtifactRepository(ISqlConnectionWrapper connectionWrapper)
        {
            ConnectionWrapper = connectionWrapper;
        }

        #region GetProjectOrArtifactChildrenAsync

        public async Task<List<Artifact>> GetProjectOrArtifactChildrenAsync(int projectId, int? artifactId, int userId)
        {
            if (projectId < 1)
                throw new ArgumentOutOfRangeException(nameof(projectId));
            if (artifactId < 1)
                throw new ArgumentOutOfRangeException(nameof(artifactId));
            if (userId < 1)
                throw new ArgumentOutOfRangeException(nameof(userId));

            // We do not treat the project as the artifact
            if (artifactId == projectId)
            {
                ThrowNotFoundException(projectId, artifactId);
            }

            var prm = new DynamicParameters();
            prm.Add("@projectId", projectId);
            prm.Add("@artifactId", artifactId ?? projectId);
            prm.Add("@userId", userId);

            var artifactVersions = (await ConnectionWrapper.QueryAsync<ArtifactVersion>("GetArtifactChildren", prm,
                    commandType: CommandType.StoredProcedure)).ToList();

            // The artifact or the project is not found
            if (!artifactVersions.Any())
            {
                ThrowNotFoundException(projectId, artifactId);
            }

            var dicUserArtifactVersions = artifactVersions.GroupBy(v => v.ItemId).ToDictionary(g => g.Key, g => GetUserArtifactVersion(g.ToList()));

            // For projects only, get orphan artifacts
            if (artifactId == null)
            {
                prm = new DynamicParameters();
                prm.Add("@projectId", projectId);
                prm.Add("@userId", userId);

                var orphanVersions = (await ConnectionWrapper.QueryAsync<ArtifactVersion>("GetProjectOrphans", prm,
                    commandType: CommandType.StoredProcedure)).ToList();

                if(orphanVersions.Any())
                {
                    var dicUserOrphanVersions = orphanVersions.GroupBy(v => v.ItemId).ToDictionary(g => g.Key, g => GetUserArtifactVersion(g.ToList()));

                    foreach (var userOrphanVersion in dicUserOrphanVersions.Values.Where(v => v.ParentId == projectId))
                    {
                        // Replace with the corrected ParentId
                        if (dicUserArtifactVersions.ContainsKey(userOrphanVersion.ItemId))
                            dicUserArtifactVersions.Remove(userOrphanVersion.ItemId);

                        // Add the orphan with children
                        dicUserArtifactVersions.Add(userOrphanVersion.ItemId, userOrphanVersion);
                        foreach (var userOrphanChildVersion in dicUserOrphanVersions.Values.Where(v => v.ParentId == userOrphanVersion.ItemId))
                        {
                            if (dicUserArtifactVersions.ContainsKey(userOrphanChildVersion.ItemId))
                                continue;

                            dicUserArtifactVersions.Add(userOrphanChildVersion.ItemId, userOrphanChildVersion);
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

            return userArtifactVersionChildren.Select(v => new Artifact
            {
                Id = v.ItemId,
                Name = v.Name,
                ProjectId = v.VersionProjectId,
                ParentId = v.ParentId,
                ItemTypeId = GetItemTypeId(v),
                Prefix = v.Prefix,
                PredefinedType = v.ItemTypePredefined.GetValueOrDefault(),
                Version = v.VersionsCount,
                OrderIndex = v.OrderIndex,
                HasChildren = v.HasChildren,
                Permissions = v.EffectivePermissions,
                LockedByUser = v.LockedByUserId.HasValue ? new UserGroup { Id = v.LockedByUserId } : null,
                LockedDateTime = v.LockedByUserTime
            })
            //NOTE:: Temporary filter Review and BaseLines ou from the list
            // See US#809: http://svmtfs2015:8080/tfs/svmtfs2015/Blueprint/_workitems?_a=edit&id=809
            .Where(a => a.PredefinedType != ItemTypePredefined.BaselineFolder)
            .OrderBy(a => {
                // To put Collections and Baselines and Reviews folder at the end of the project children 
                if (a.OrderIndex >= 0)
                    return a.OrderIndex;
                if(a.OrderIndex < 0 && a.PredefinedType == ItemTypePredefined.CollectionFolder)
                    return maxIndexOrder + 1; // Collections folder comes after artifacts
                if (a.OrderIndex < 0 && a.PredefinedType == ItemTypePredefined.BaselineFolder)
                    return maxIndexOrder + 2; // Baseline and Reviews folder comes after Collections folder

                Debug.Assert(false, "Illegal Order Index: " + a.OrderIndex);
                return double.MaxValue;
            }).ToList();
        }

        // Returns stub ItemTypeId for Collections and Baselines and Reviews folders under the project.
        private static int? GetItemTypeId(ArtifactVersion av)
        {
            if (av.ParentId != av.VersionProjectId)
                return av.ItemTypeId;

            switch(av.ItemTypePredefined)
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

            if(directAncestorUserVersion == null)
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
            if(headAndDraft == null)
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
                ? I18NHelper.FormatInvariant("Project (Id:{0}) is not found.", projectId)
                : I18NHelper.FormatInvariant("Artifact (Id:{0}) in Project (Id:{1}) is not found.", artifactId, projectId);
            throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
        }

        private static void ThrowForbiddenException(int projectId, int? artifactId)
        {
            var errorMessage = artifactId == null
                ? I18NHelper.FormatInvariant("User does not permissions for Project (Id:{0}).", projectId)
                : I18NHelper.FormatInvariant("User does not permissions for Artifact (Id:{0}).", artifactId);
            throw new AuthorizationException(errorMessage, ErrorCodes.UnauthorizedAccess);
        }

        #endregion

        #region GetProjectOrArtifactChildrenAsync

        public Task<List<Artifact>> GetExpandedTreeToArtifactAsync(int projectId, int expandedToArtifactId, int userId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    internal class ArtifactVersion
    {
        internal int ItemId { get; set; }
        internal int VersionProjectId { get; set; }
        internal int? ParentId { get; set; }
        internal string Name { get; set; }
        internal double? OrderIndex { get; set; }
        internal int StartRevision { get; set; }
        internal int EndRevision { get; set; }
        internal ItemTypePredefined? ItemTypePredefined { get; set; }
        internal int? ItemTypeId { get; set; }
        internal string Prefix { get; set; }
        internal int? LockedByUserId { get; set; }
        internal DateTime? LockedByUserTime { get; set; }
        internal RolePermissions? DirectPermissions { get; set; }
        // Not returned in SQL server but calculated in the server application
        internal RolePermissions? EffectivePermissions { get; set; }
        // Returned doubled if returned Head and Draft 
        internal int? VersionsCount { get; set; }
        internal bool HasDraft { get; set; }
        // Not returned in SQL server but calculated in the server application
        internal bool? HasChildren { get; set; }
    }
}