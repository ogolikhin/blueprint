using ArtifactStore.Helpers;
using ArtifactStore.Models;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ArtifactStore.Repositories
{
    public class SqlArtifactVersionsRepository : IArtifactVersionsRepository
    {
        internal readonly ISqlConnectionWrapper ConnectionWrapper;
        public SqlArtifactVersionsRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal SqlArtifactVersionsRepository(ISqlConnectionWrapper connectionWrapper)
        {
            ConnectionWrapper = connectionWrapper;
        }

        private async Task<bool> IncludeDraftVersion(int? userId, int sessionUserId, int artifactId)
        {
            if (!userId.HasValue || userId.Value == sessionUserId)
            {
                var artifactWithDraftPrm = new DynamicParameters();
                var artifactIdsTable = SqlConnectionWrapper.ToDataTable(new List<int> { artifactId }, "Int32Collection", "Int32Value");
                artifactWithDraftPrm.Add("@userId", sessionUserId);
                artifactWithDraftPrm.Add("@artifactIds", artifactIdsTable);
                return (await ConnectionWrapper.QueryAsync<int>("GetArtifactsWithDraft", artifactWithDraftPrm, commandType: CommandType.StoredProcedure)).Count() == 1;
            }
            return false;
        }

        private async Task<bool> DoesArtifactHavePublishedOrDraftVersions(int artifactId)
        {
            var isPublishOrDraftPrm = new DynamicParameters();
            isPublishOrDraftPrm.Add("@artifactId", artifactId);
            return (await ConnectionWrapper.QueryAsync<bool>("DoesArtifactHavePublishedOrDraftVersion", isPublishOrDraftPrm, commandType: CommandType.StoredProcedure)).SingleOrDefault();
        }

        public async Task<bool> IsItemDeleted(int itemId)
        {
            var isDeletedPrm = new DynamicParameters();
            isDeletedPrm.Add("@artifactId", itemId);
            return (await ConnectionWrapper.QueryAsync<bool>("IsArtifactDeleted", isDeletedPrm, commandType: CommandType.StoredProcedure)).SingleOrDefault();
        }

        public async Task<DeletedItemInfo> GetDeletedItemInfo(int itemId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@itemId", itemId);
            return (await ConnectionWrapper.QueryAsync<DeletedItemInfo>("GetDeletedItemInfo", parameters, commandType: CommandType.StoredProcedure)).SingleOrDefault();
        }

        private async Task<ArtifactHistoryVersion> DeletedVersionInfo(int artifactId)
        {
            var isDeletedPrm = new DynamicParameters();
            isDeletedPrm.Add("@artifactId", artifactId);
            var result = (await ConnectionWrapper.QueryAsync<ArtifactHistoryVersion>("GetDeletedVersionInfo", isDeletedPrm, commandType: CommandType.StoredProcedure)).SingleOrDefault();
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
            return await ConnectionWrapper.QueryAsync<ArtifactHistoryVersion>("GetArtifactVersions", artifactVersionsPrm, commandType: CommandType.StoredProcedure);
        }

        private async Task<IEnumerable<UserInfo>> GetUserInfos(IEnumerable<int> userIds)
        {
            var userInfosPrm = new DynamicParameters();
            var userIdsTable = SqlConnectionWrapper.ToDataTable(userIds, "Int32Collection", "Int32Value");
            userInfosPrm.Add("@userIds", userIdsTable);
            return await ConnectionWrapper.QueryAsync<UserInfo>("GetUserInfos", userInfosPrm, commandType: CommandType.StoredProcedure);
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

        public async Task<ArtifactHistoryResultSet> GetArtifactVersions(int artifactId, int limit, int offset, int? userId, bool asc, int sessionUserId)
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
                    distinctUserIds = distinctUserIds.Union(new int[] { deletedVersionInfo.UserId });
                    InsertDraftOrDeletedVersion(limit, offset, asc, artifactVersions, deletedVersionInfo);
                }
            }
            else 
            {
                var includeDraftVersion = (await IncludeDraftVersion(userId, sessionUserId, artifactId));
                if (includeDraftVersion)
            {
                distinctUserIds = distinctUserIds.Union(new int[] { sessionUserId });
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
                    new ArtifactHistoryVersionWithUserInfo {
                                                             VersionId = artifactVersion.VersionId,
                                                             UserId = artifactVersion.UserId,
                                                             Timestamp = DateTime.SpecifyKind(artifactVersion.Timestamp.GetValueOrDefault(), DateTimeKind.Utc),
                                                             DisplayName = userInfo.DisplayName,
                                                             HasUserIcon = userInfo.ImageId != null,
                                                             ArtifactState = artifactVersion.ArtifactState 
                                                             });
            }
            var result = new ArtifactHistoryResultSet { ArtifactId = artifactId, ArtifactHistoryVersions = artifactHistoryVersionWithUserInfos };
            return result;
        }

        #region GetVersionControlArtifactInfoAsync

        public Task<VersionControlArtifactInfo> GetVersionControlArtifactInfoAsync(int itemId, int userId)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}