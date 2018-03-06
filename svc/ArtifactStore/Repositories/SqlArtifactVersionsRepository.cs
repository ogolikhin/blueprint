using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Models;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.VersionControl;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Repositories
{
    public class SqlArtifactVersionsRepository : IArtifactVersionsRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;
        private readonly IArtifactRepository _artifactRepository;
        private readonly IArtifactPermissionsRepository _artifactPermissionsRepository;
        private readonly IItemInfoRepository _itemInfoRepository;

        public SqlArtifactVersionsRepository() : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal SqlArtifactVersionsRepository(ISqlConnectionWrapper connectionWrapper)
            : this(
                connectionWrapper,
                new SqlArtifactRepository(connectionWrapper),
                new SqlArtifactPermissionsRepository(connectionWrapper),
                new SqlItemInfoRepository(connectionWrapper))
        {
        }

        internal SqlArtifactVersionsRepository(
            ISqlConnectionWrapper connectionWrapper,
            IArtifactRepository artifactRepository,
            IArtifactPermissionsRepository artifactPermissionsRepository,
            IItemInfoRepository itemInfoRepository)
        {
            _connectionWrapper = connectionWrapper;
            _artifactRepository = artifactRepository;
            _artifactPermissionsRepository = artifactPermissionsRepository;
            _itemInfoRepository = itemInfoRepository;
        }

        private async Task<bool> IncludeDraftVersion(int? userId, int sessionUserId, int artifactId, bool includeDrafts)
        {
            if (!includeDrafts || userId.HasValue && userId.Value != sessionUserId)
            {
                return false;
            }

            var parameters = new DynamicParameters();
            var artifactIdsTable = SqlConnectionWrapper.ToDataTable(new List<int> { artifactId });
            parameters.Add("@userId", sessionUserId);
            parameters.Add("@artifactIds", artifactIdsTable);

            return (await _connectionWrapper.QueryAsync<int>("GetArtifactsWithDraft", parameters, commandType: CommandType.StoredProcedure)).Count() == 1;
        }

        private async Task<bool> DoesArtifactHavePublishedOrDraftVersions(int artifactId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@artifactId", artifactId);

            return (await _connectionWrapper.QueryAsync<bool>("DoesArtifactHavePublishedOrDraftVersion", parameters, commandType: CommandType.StoredProcedure)).SingleOrDefault();
        }

        public async Task<bool> IsItemDeleted(int itemId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@artifactId", itemId);

            return (await _connectionWrapper.QueryAsync<bool>("IsArtifactDeleted", parameters, commandType: CommandType.StoredProcedure)).SingleOrDefault();
        }

        public async Task<IEnumerable<int>> GetDeletedAndNotInProjectItems(IEnumerable<int> itemIds, int projectId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@itemIds", SqlConnectionWrapper.ToDataTable(itemIds));
            parameters.Add("@projectId", projectId);

            return await _connectionWrapper.QueryAsync<int>("GetDeletedAndNotInProjectItems", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<DeletedItemInfo> GetDeletedItemInfo(int itemId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@itemId", itemId);

            return (await _connectionWrapper.QueryAsync<DeletedItemInfo>("GetDeletedItemInfo", parameters, commandType: CommandType.StoredProcedure)).SingleOrDefault();
        }

        private async Task<ArtifactHistoryVersion> DeletedVersionInfo(int artifactId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@artifactId", artifactId);

            var result = (await _connectionWrapper.QueryAsync<ArtifactHistoryVersion>("GetDeletedVersionInfo", parameters, commandType: CommandType.StoredProcedure)).Single();
            result.VersionId = int.MaxValue;
            result.ArtifactState = ArtifactState.Deleted;

            return result;
        }

        private async Task<IEnumerable<ArtifactHistoryVersion>> GetPublishedArtifactHistory(int artifactId, int limit, int offset, int? userId, bool asc)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@artifactId", artifactId);
            parameters.Add("@lim", limit);
            parameters.Add("@offset", offset);

            if (userId.HasValue)
            {
                parameters.Add("@userId", userId.Value);
            }
            else
            {
                parameters.Add("@userId");
            }

            parameters.Add("@ascd", asc);

            return await _connectionWrapper.QueryAsync<ArtifactHistoryVersion>("GetArtifactVersions", parameters, commandType: CommandType.StoredProcedure);
        }

        private async Task<IEnumerable<UserInfo>> GetUserInfos(IEnumerable<int> userIds)
        {
            var parameters = new DynamicParameters();
            var userIdsTable = SqlConnectionWrapper.ToDataTable(userIds);
            parameters.Add("@userIds", userIdsTable);

            return await _connectionWrapper.QueryAsync<UserInfo>("GetUserInfos", parameters, commandType: CommandType.StoredProcedure);
        }

        private static void InsertDraftOrDeletedVersion(int limit, int offset, bool asc, IList<ArtifactHistoryVersion> artifactVersions, ArtifactHistoryVersion deletedOrDraftEntry)
        {
            if (asc && artifactVersions.Count < limit)
            {
                artifactVersions.Insert(artifactVersions.Count, deletedOrDraftEntry);
            }
            else if (!asc && offset == 0)
            {
                artifactVersions.Insert(0, deletedOrDraftEntry);
            }
        }

        public async Task<ArtifactHistoryResultSet> GetArtifactVersions(int artifactId, int limit, int offset, int? userId, bool asc, int sessionUserId, bool includeDrafts)
        {
            if (artifactId < 1)
                throw new ArgumentOutOfRangeException(nameof(artifactId));

            if (limit < 1 || limit > 100)
                throw new ArgumentOutOfRangeException(nameof(limit));

            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (userId.HasValue && userId < 1)
                throw new ArgumentOutOfRangeException(nameof(userId));

            if (!await DoesArtifactHavePublishedOrDraftVersions(artifactId))
            {
                return new ArtifactHistoryResultSet { ArtifactId = artifactId, ArtifactHistoryVersions = new List<ArtifactHistoryVersionWithUserInfo>() };
            }

            var artifactVersions = (await GetPublishedArtifactHistory(artifactId, limit, offset, userId, asc)).ToList();
            var distinctUserIds = artifactVersions.Select(a => a.UserId).Distinct();
            var isDeleted = await IsItemDeleted(artifactId);

            if (isDeleted)
            {
                var deletedVersionInfo = await DeletedVersionInfo(artifactId);

                if (userId == null || userId.Value == deletedVersionInfo.UserId)
                {
                    deletedVersionInfo.ArtifactState = ArtifactState.Deleted;
                    distinctUserIds = distinctUserIds.Union(new[] { deletedVersionInfo.UserId });
                    InsertDraftOrDeletedVersion(limit, offset, asc, artifactVersions, deletedVersionInfo);
                }
            }
            else
            {
                var includeDraftVersion = await IncludeDraftVersion(userId, sessionUserId, artifactId, includeDrafts);

                if (includeDraftVersion)
                {
                    distinctUserIds = distinctUserIds.Union(new[] { sessionUserId });

                    var draftItem = new ArtifactHistoryVersion
                    {
                        VersionId = int.MaxValue,
                        UserId = sessionUserId,
                        Timestamp = null,
                        ArtifactState = ArtifactState.Draft
                    };

                    InsertDraftOrDeletedVersion(limit, offset, asc, artifactVersions, draftItem);
                }
            }

            var userInfoDictionary = (await GetUserInfos(distinctUserIds)).ToDictionary(a => a.UserId);
            var artifactHistoryVersionWithUserInfos = new List<ArtifactHistoryVersionWithUserInfo>();

            foreach (var artifactVersion in artifactVersions)
            {
                UserInfo userInfo;
                userInfoDictionary.TryGetValue(artifactVersion.UserId, out userInfo);

                artifactHistoryVersionWithUserInfos.Add(
                    new ArtifactHistoryVersionWithUserInfo
                    {
                        VersionId = artifactVersion.VersionId,
                        UserId = artifactVersion.UserId,
                        Timestamp = DateTime.SpecifyKind(artifactVersion.Timestamp.GetValueOrDefault(), DateTimeKind.Utc),
                        DisplayName = userInfo?.DisplayName,
                        HasUserIcon = userInfo?.ImageId != null,
                        ArtifactState = artifactVersion.ArtifactState
                    });
            }

            return new ArtifactHistoryResultSet
            {
                ArtifactId = artifactId,
                ArtifactHistoryVersions = artifactHistoryVersionWithUserInfos
            };
        }

        private async Task<bool> IsArtifactInBaseline(int artifactId, int baselineId, int userId)
        {
            var baselineArtifacts = await _itemInfoRepository.GetBaselineArtifacts(baselineId, userId);

            return baselineArtifacts != null && baselineArtifacts.Contains(artifactId);
        }

        public async Task<VersionControlArtifactInfo> GetVersionControlArtifactInfoAsync(int itemId, int? baselineId, int userId, IDbTransaction transaction = null)
        {
            var artifactBasicDetails = await _artifactRepository.GetArtifactBasicDetails(itemId, userId, transaction);
            if (artifactBasicDetails == null)
            {
                var errorMessage = I18NHelper.FormatInvariant("Item (Id:{0}) is not found.", itemId);
                throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
            }

            // Always getting permissions for the Head version of an artifact.
            // But, just in case, RevisionId and AddDrafts are available in ArtifactBasicDetails.
            var itemIdsPermissions = await _artifactPermissionsRepository.GetArtifactPermissions(Enumerable.Repeat(artifactBasicDetails.ArtifactId, 1), userId);
            if (!itemIdsPermissions.ContainsKey(artifactBasicDetails.ArtifactId) || !itemIdsPermissions[artifactBasicDetails.ArtifactId].HasFlag(RolePermissions.Read))
            {
                var errorMessage = I18NHelper.FormatInvariant("User does not have permissions for Artifact (Id:{0}).", artifactBasicDetails.ArtifactId);
                throw new AuthorizationException(errorMessage, ErrorCodes.UnauthorizedAccess);
            }

            var artifactInfo = new VersionControlArtifactInfo
            {
                Id = artifactBasicDetails.ArtifactId,
                SubArtifactId = artifactBasicDetails.ArtifactId != artifactBasicDetails.ItemId
                    ? (int?)artifactBasicDetails.ItemId
                    : null,
                Name = artifactBasicDetails.Name,
                ProjectId = artifactBasicDetails.ProjectId,
                ParentId = artifactBasicDetails.ParentId,
                OrderIndex = artifactBasicDetails.OrderIndex,
                ItemTypeId = artifactBasicDetails.ItemTypeId,
                Prefix = artifactBasicDetails.Prefix,
                PredefinedType = (ItemTypePredefined)artifactBasicDetails.PrimitiveItemTypePredefined,
                Version = artifactBasicDetails.VersionIndex != null && artifactBasicDetails.VersionIndex.Value <= 0 ? -1 : artifactBasicDetails.VersionIndex,
                VersionCount = artifactBasicDetails.VersionsCount,
                IsDeleted = artifactBasicDetails.DraftDeleted || artifactBasicDetails.LatestDeleted,
                HasChanges = artifactBasicDetails.LockedByUserId != null && artifactBasicDetails.LockedByUserId.Value == userId
                    || artifactBasicDetails.HasDraftRelationships,
                LockedByUser = artifactBasicDetails.LockedByUserId != null
                    ? new UserGroup { Id = artifactBasicDetails.LockedByUserId.Value, DisplayName = artifactBasicDetails.LockedByUserName }
                    : null,
                LockedDateTime = artifactBasicDetails.LockedByUserTime != null
                    ? (DateTime?)DateTime.SpecifyKind(artifactBasicDetails.LockedByUserTime.Value, DateTimeKind.Utc)
                    : null
            };

            if (artifactBasicDetails.DraftDeleted)
            {
                artifactInfo.DeletedByUser = artifactBasicDetails.UserId != null
                    ? new UserGroup { Id = artifactBasicDetails.UserId.Value, DisplayName = artifactBasicDetails.UserName }
                    : null;
                artifactInfo.DeletedDateTime = artifactBasicDetails.LastSaveTimestamp != null
                    ? (DateTime?)DateTime.SpecifyKind(artifactBasicDetails.LastSaveTimestamp.Value, DateTimeKind.Utc)
                    : null;
            }
            else if (artifactBasicDetails.LatestDeleted)
            {
                artifactInfo.DeletedByUser = artifactBasicDetails.LatestDeletedByUserId != null
                    ? new UserGroup { Id = artifactBasicDetails.LatestDeletedByUserId.Value, DisplayName = artifactBasicDetails.LatestDeletedByUserName }
                    : null;
                artifactInfo.DeletedDateTime = artifactBasicDetails.LatestDeletedByUserTime != null
                    ? (DateTime?)DateTime.SpecifyKind(artifactBasicDetails.LatestDeletedByUserTime.Value, DateTimeKind.Utc)
                    : null;
            }

            artifactInfo.Permissions = itemIdsPermissions[artifactBasicDetails.ArtifactId];

            if (baselineId != null)
            {
                var baselineRevisionId = await _itemInfoRepository.GetRevisionId(itemId, userId, null, baselineId.Value);
                var itemInfo = await _artifactPermissionsRepository.GetItemInfo(itemId, userId, false, baselineRevisionId);
                if (itemInfo == null)
                {
                    artifactInfo.IsNotExistsInBaseline = true;
                }

                artifactInfo.IsIncludedInBaseline = await IsArtifactInBaseline(artifactBasicDetails.ArtifactId, baselineId.Value, userId);
            }

            return artifactInfo;
        }
    }
}
