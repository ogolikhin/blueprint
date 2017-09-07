using ArtifactStore.Models;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ServiceLibrary.Models.VersionControl;

namespace ArtifactStore.Repositories
{
    public class SqlArtifactVersionsRepository : IArtifactVersionsRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;
        private readonly IArtifactPermissionsRepository _artifactPermissionsRepository;
        private readonly ISqlItemInfoRepository _itemInfoRepository;

        public SqlArtifactVersionsRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal SqlArtifactVersionsRepository(ISqlConnectionWrapper connectionWrapper)
            : this(connectionWrapper, new SqlArtifactPermissionsRepository(connectionWrapper), new SqlItemInfoRepository(connectionWrapper))
        {
        }

        internal SqlArtifactVersionsRepository(ISqlConnectionWrapper connectionWrapper,
                IArtifactPermissionsRepository artifactPermissionsRepository, ISqlItemInfoRepository itemInfoRepository)
        {
            _connectionWrapper = connectionWrapper;
            _artifactPermissionsRepository = artifactPermissionsRepository;
            _itemInfoRepository = itemInfoRepository;
        }

        private async Task<bool> IncludeDraftVersion(int? userId, int sessionUserId, int artifactId, bool includeDrafts)
        {
            if (includeDrafts && (!userId.HasValue || userId.Value == sessionUserId))
            {
                var artifactWithDraftPrm = new DynamicParameters();
                var artifactIdsTable = SqlConnectionWrapper.ToDataTable(new List<int> { artifactId }, "Int32Collection", "Int32Value");
                artifactWithDraftPrm.Add("@userId", sessionUserId);
                artifactWithDraftPrm.Add("@artifactIds", artifactIdsTable);
                return (await _connectionWrapper.QueryAsync<int>("GetArtifactsWithDraft", artifactWithDraftPrm, commandType: CommandType.StoredProcedure)).Count() == 1;
            }

            return false;
        }

        private async Task<bool> DoesArtifactHavePublishedOrDraftVersions(int artifactId)
        {
            var isPublishOrDraftPrm = new DynamicParameters();
            isPublishOrDraftPrm.Add("@artifactId", artifactId);
            return (await _connectionWrapper.QueryAsync<bool>("DoesArtifactHavePublishedOrDraftVersion", isPublishOrDraftPrm, commandType: CommandType.StoredProcedure)).SingleOrDefault();
        }

        public async Task<bool> IsItemDeleted(int itemId)
        {
            var isDeletedPrm = new DynamicParameters();
            isDeletedPrm.Add("@artifactId", itemId);
            return (await _connectionWrapper.QueryAsync<bool>("IsArtifactDeleted", isDeletedPrm, commandType: CommandType.StoredProcedure)).SingleOrDefault();
        }

        public async Task<DeletedItemInfo> GetDeletedItemInfo(int itemId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@itemId", itemId);
            return (await _connectionWrapper.QueryAsync<DeletedItemInfo>("GetDeletedItemInfo", parameters, commandType: CommandType.StoredProcedure)).SingleOrDefault();
        }

        public async Task<bool> LockArtifactAsync(int artifactId, int userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            parameters.Add("@artifactId", artifactId);
            var result = (await _connectionWrapper.QueryAsync<int?>("LockArtifact", parameters, commandType: CommandType.StoredProcedure)).SingleOrDefault();
            if (result.HasValue && result.Value > 0)
            {
                return true;
            }
            return false;
        }

        private async Task<ArtifactHistoryVersion> DeletedVersionInfo(int artifactId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@artifactId", artifactId);
            var result = (await _connectionWrapper.QueryAsync<ArtifactHistoryVersion>("GetDeletedVersionInfo", parameters, commandType: CommandType.StoredProcedure)).SingleOrDefault();
            result.VersionId = int.MaxValue;
            result.ArtifactState = ArtifactState.Deleted;
            return result;
        }

        private async Task<IEnumerable<ArtifactHistoryVersion>> GetPublishedArtifactHistory(int artifactId, int limit, int offset, int? userId, bool asc)
        {
            var artifactVersionsPrm = new DynamicParameters();
            artifactVersionsPrm.Add("@artifactId", artifactId);
            artifactVersionsPrm.Add("@lim", limit);
            artifactVersionsPrm.Add("@offset", offset);
            if (userId.HasValue) { artifactVersionsPrm.Add("@userId", userId.Value); }
            else { artifactVersionsPrm.Add("@userId", null); }
            artifactVersionsPrm.Add("@ascd", asc);
            return await _connectionWrapper.QueryAsync<ArtifactHistoryVersion>("GetArtifactVersions", artifactVersionsPrm, commandType: CommandType.StoredProcedure);
        }

        private async Task<IEnumerable<UserInfo>> GetUserInfos(IEnumerable<int> userIds)
        {
            var userInfosPrm = new DynamicParameters();
            var userIdsTable = SqlConnectionWrapper.ToDataTable(userIds, "Int32Collection", "Int32Value");
            userInfosPrm.Add("@userIds", userIdsTable);
            return await _connectionWrapper.QueryAsync<UserInfo>("GetUserInfos", userInfosPrm, commandType: CommandType.StoredProcedure);
        }

        private void InsertDraftOrDeletedVersion(int limit, int offset, bool asc, List<ArtifactHistoryVersion> artifactVersions, ArtifactHistoryVersion deletedOrDraftEntry)
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

            if (!(await DoesArtifactHavePublishedOrDraftVersions(artifactId)))
            {
                return new ArtifactHistoryResultSet { ArtifactId = artifactId, ArtifactHistoryVersions = new List<ArtifactHistoryVersionWithUserInfo>() };
            }

            var artifactVersions = (await GetPublishedArtifactHistory(artifactId, limit, offset, userId, asc)).ToList();
            var distinctUserIds = artifactVersions.Select(a => a.UserId).Distinct();
            var isDeleted = (await IsItemDeleted(artifactId));

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
                var includeDraftVersion = (await IncludeDraftVersion(userId, sessionUserId, artifactId, includeDrafts));

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
                    }
                );
            }

            return new ArtifactHistoryResultSet
            {
                ArtifactId = artifactId,
                ArtifactHistoryVersions = artifactHistoryVersionWithUserInfos
            };
        }

        private async Task<bool> IsArtifactInBaseline(int artifactId, int baselineId, int userId)
        {
            var baselinArtifacts = await _itemInfoRepository.GetBaselineArtifacts(baselineId, userId);
            if (baselinArtifacts != null)
            {
                return baselinArtifacts.Contains(artifactId);
            }
            return false;
        }

        #region GetVersionControlArtifactInfoAsync

        public async Task<VersionControlArtifactInfo> GetVersionControlArtifactInfoAsync(int itemId, int? baselineId, int userId)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@userId", userId);
            dynamicParameters.Add("@itemId", itemId);
            ArtifactBasicDetails artifactBasicDetails = (await _connectionWrapper.QueryAsync<ArtifactBasicDetails>(
                "GetArtifactBasicDetails", dynamicParameters, commandType: CommandType.StoredProcedure)).FirstOrDefault();
            if (artifactBasicDetails == null)
            {
                string errorMessage = I18NHelper.FormatInvariant("Item (Id:{0}) is not found.", itemId);
                throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
            }
            Dictionary<int, RolePermissions> itemIdsPermissions =
                // Always getting permissions for the Head version of an artifact.
                // But, just in case, RevisionId and AddDrafts are available in ArtifactBasicDetails.
                (await _artifactPermissionsRepository.GetArtifactPermissions(Enumerable.Repeat(artifactBasicDetails.ArtifactId, 1), userId));
            if (!itemIdsPermissions.ContainsKey(artifactBasicDetails.ArtifactId) || !itemIdsPermissions[artifactBasicDetails.ArtifactId].HasFlag(RolePermissions.Read))
            {
                string errorMessage = I18NHelper.FormatInvariant("User does not have permissions for Artifact (Id:{0}).", artifactBasicDetails.ArtifactId);
                throw new AuthorizationException(errorMessage, ErrorCodes.UnauthorizedAccess);
            }
            VersionControlArtifactInfo artifactInfo = new VersionControlArtifactInfo
            {
                Id = artifactBasicDetails.ArtifactId,
                SubArtifactId = (artifactBasicDetails.ArtifactId != artifactBasicDetails.ItemId) ? (int?)artifactBasicDetails.ItemId : null,
                Name = artifactBasicDetails.Name,
                ProjectId = artifactBasicDetails.ProjectId,
                ParentId = artifactBasicDetails.ParentId,
                OrderIndex = artifactBasicDetails.OrderIndex,
                ItemTypeId = artifactBasicDetails.ItemTypeId,
                Prefix = artifactBasicDetails.Prefix,
                PredefinedType = (ItemTypePredefined)artifactBasicDetails.PrimitiveItemTypePredefined,
                Version = (artifactBasicDetails.VersionIndex != null) && (artifactBasicDetails.VersionIndex.Value <= 0) ? -1 : artifactBasicDetails.VersionIndex,
                VersionCount = artifactBasicDetails.VersionsCount,
                IsDeleted = (artifactBasicDetails.DraftDeleted || artifactBasicDetails.LatestDeleted),
                HasChanges = ((artifactBasicDetails.LockedByUserId != null) && (artifactBasicDetails.LockedByUserId.Value == userId))
                    || artifactBasicDetails.HasDraftRelationships,
                LockedByUser = (artifactBasicDetails.LockedByUserId != null)
                    ? new UserGroup { Id = artifactBasicDetails.LockedByUserId.Value, DisplayName = artifactBasicDetails.LockedByUserName } : null,
                LockedDateTime = (artifactBasicDetails.LockedByUserTime != null)
                    ? (DateTime?)DateTime.SpecifyKind(artifactBasicDetails.LockedByUserTime.Value, DateTimeKind.Utc)
                    : null
            };
            if (artifactBasicDetails.DraftDeleted)
            {
                artifactInfo.DeletedByUser = (artifactBasicDetails.UserId != null)
                        ? new UserGroup { Id = artifactBasicDetails.UserId.Value, DisplayName = artifactBasicDetails.UserName } : null;
                artifactInfo.DeletedDateTime = (artifactBasicDetails.LastSaveTimestamp != null)
                    ? (DateTime?)DateTime.SpecifyKind(artifactBasicDetails.LastSaveTimestamp.Value, DateTimeKind.Utc)
                    : null;
            }
            else if (artifactBasicDetails.LatestDeleted)
            {
                artifactInfo.DeletedByUser = (artifactBasicDetails.LatestDeletedByUserId != null)
                        ? new UserGroup { Id = artifactBasicDetails.LatestDeletedByUserId.Value, DisplayName = artifactBasicDetails.LatestDeletedByUserName } : null;
                artifactInfo.DeletedDateTime = (artifactBasicDetails.LatestDeletedByUserTime != null)
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

        

        #endregion GetVersionControlArtifactInfoAsync

    }
}