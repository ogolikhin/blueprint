using ArtifactStore.Models;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using System;
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
        public async Task<ArtifactHistoryResultSet> GetArtifactVersions(int artifactId, int limit, int offset, int? userId, bool asc)
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
            prm.Add("@limit", limit);
            prm.Add("@offset", offset);
            if (userId.HasValue)
            {
                prm.Add("@userId", userId.Value);
            }
            else
            {
                prm.Add("@userId", null);
            }
            prm.Add("@asc", asc);
            var artifactVersions = (await ConnectionWrapper.QueryAsync<ArtifactHistoryVersion>("GetArtifactVersions", prm,
                    commandType: CommandType.StoredProcedure)).ToList();
            var numEntriesForQuery = await GetNumEntriesForQuery(artifactId, offset, userId, asc);
            var result = new ArtifactHistoryResultSet {
                ArtifactId = artifactId,
                ArtifactHistoryVersions = artifactVersions,
                HasMore = numEntriesForQuery > limit
            };
            return result;
        }

        private async Task<int> GetNumEntriesForQuery(int artifactId, int offset, int? userId, bool asc)
        {
            var prm = new DynamicParameters();
            prm.Add("@artifactId", artifactId);
            prm.Add("@offset", offset);
            if (userId.HasValue)
            {
                prm.Add("@userId", userId.Value);
            }
            else
            {
                prm.Add("@userId", null);
            }
            prm.Add("@asc", asc);
            return (await ConnectionWrapper.QueryAsync<int>("GetNumVersionEntries", prm,
                    commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }
    }
}