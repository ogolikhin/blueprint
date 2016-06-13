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
    public class UserInfo
    {
        public int UserId { get; set; }
        public string DisplayName { get; set; }
        public int? Image_ImageId { get; set; }
    }
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

            var prm = new DynamicParameters();
            prm.Add("@artifactId", artifactId);
            prm.Add("@lim", limit);
            prm.Add("@offset", offset);
            if (userId.HasValue) { prm.Add("@userId", userId.Value); }
            else { prm.Add("@userId", null); }
            prm.Add("@ascd", asc);
            var artifactVersions = (await ConnectionWrapper.QueryAsync<ArtifactHistoryVersion>("GetArtifactVersions", prm,
                    commandType: CommandType.StoredProcedure)).ToList();

            var prm2 = new DynamicParameters();
            var artifactIdsTable = DapperHelper.GetIntCollectionTableValueParameter(new List<int> { artifactId });
            prm2.Add("@userId", sessionUserId);
            prm2.Add("@artifactIds", artifactIdsTable);
            var doesCurrentUserHaveDraft = (await ConnectionWrapper.QueryAsync<int>("GetArtifactsWithDraft", prm2, commandType: CommandType.StoredProcedure)).Count() == 1;
            if (doesCurrentUserHaveDraft)
            {
                var getUserInfosPrm = new DynamicParameters();
                var userIdsTable = DapperHelper.GetIntCollectionTableValueParameter(new List<int> { sessionUserId });
                getUserInfosPrm.Add("@userIds", userIdsTable);
                var userInfo = (await ConnectionWrapper.QueryAsync<UserInfo>("GetUserInfos", getUserInfosPrm, commandType: CommandType.StoredProcedure)).SingleOrDefault();

                var draftItem = new ArtifactHistoryVersion {
                    VersionId = int.MaxValue,
                    UserId = sessionUserId,
                    DisplayName = userInfo?.DisplayName,
                    HasUserIcon = userInfo?.Image_ImageId != null,
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
            var result = new ArtifactHistoryResultSet {
                ArtifactId = artifactId,
                ArtifactHistoryVersions = artifactVersions
            };
            return result;
        }
    }
}