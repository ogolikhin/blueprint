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
    public class SqlArtifactVersionsRepository : ISqlArtifactVersionsRepository
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
                var artifactIdsTable = DapperHelper.GetIntCollectionTableValueParameter(new List<int> { artifactId });
                artifactWithDraftPrm.Add("@userId", sessionUserId);
                artifactWithDraftPrm.Add("@artifactIds", artifactIdsTable);
                return (await ConnectionWrapper.QueryAsync<int>("GetArtifactsWithDraft", artifactWithDraftPrm, commandType: CommandType.StoredProcedure)).Count() == 1;
            }
            return false;
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

        private async Task<IEnumerable<UserInfo>> GetUserInfos(IEnumerable<int> UserIds)
        {
            var userInfosPrm = new DynamicParameters();
            var userIdsTable = DapperHelper.GetIntCollectionTableValueParameter(UserIds);
            userInfosPrm.Add("@userIds", userIdsTable);
            return await ConnectionWrapper.QueryAsync<UserInfo>("GetUserInfos", userInfosPrm, commandType: CommandType.StoredProcedure);
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

            var artifactVersions = (await GetPublishedArtifactHistory(artifactId, limit, offset, userId, asc)).ToList();
            var distinctUserIds = artifactVersions.Select(a => a.UserId).Distinct();

            if (await IncludeDraftVersion(userId, sessionUserId, artifactId))
            {
                distinctUserIds = distinctUserIds.Union(new int[] { sessionUserId });
                var draftItem = new ArtifactHistoryVersion {
                    VersionId = int.MaxValue,
                    UserId = sessionUserId,
                    Timestamp = null
                };
                if (asc && artifactVersions.Count < limit)
                {
                    artifactVersions.Insert(artifactVersions.Count, draftItem);
                }
                else if (!asc && offset == 0)
                {
                    artifactVersions.Insert(0, draftItem);
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
                                                             Timestamp = artifactVersion.Timestamp,
                                                             DisplayName = userInfo.DisplayName,
                                                             HasUserIcon = userInfo.Image_ImageId != null });
            }
            var result = new ArtifactHistoryResultSet {
                ArtifactId = artifactId,
                ArtifactHistoryVersions = artifactHistoryVersionWithUserInfos
            };
            return result;
        }
    }
}