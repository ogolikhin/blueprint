using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using System.Collections.Generic;

namespace ArtifactStore.Repositories.Revisions
{
    public interface IRevisionRepository
    {
        Task<int?> CreateNewRevision(int userId, string comment);

        Task<IEnumerable<int>> AddHistory(int revisionId, ISet<int> artifactIds);
    }

    public class SqlRevisionRepository : SqlBaseArtifactRepository, IRevisionRepository
    {
        public SqlRevisionRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        public SqlRevisionRepository(ISqlConnectionWrapper connectionWrapper)
            : this(connectionWrapper, new SqlArtifactPermissionsRepository(connectionWrapper))
        {
        }

        public SqlRevisionRepository(ISqlConnectionWrapper connectionWrapper,
            IArtifactPermissionsRepository artifactPermissionsRepository)
            : base(connectionWrapper, artifactPermissionsRepository)
        {
        }

        public async Task<int?> CreateNewRevision(int userId, string comment)
        {
            return await CreateNewRevisionInternal(userId, comment);
        }

        public async Task<IEnumerable<int>> AddHistory(int revisionId, ISet<int> artifactIds)
        {
            return await AddHistoryInternal(revisionId, artifactIds);
        }

        private async Task<int?> CreateNewRevisionInternal(int userId, string comment)
        {
            var param = new DynamicParameters();
            param.Add("@userId", userId);
            param.Add("@comment", comment);
            return (await ConnectionWrapper.QueryAsync<int?>("CreateRevision", param,
                commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }

        private async Task<IEnumerable<int>> AddHistoryInternal(int revisionId, ISet<int> artifactIds)
        {
            if (artifactIds.Count == 0)
            {
                return await Task.FromResult(new[] { 0 });
            }

            var param = new DynamicParameters();
            param.Add("@revisionId", revisionId);
            var artifactIdsTable = SqlConnectionWrapper.ToDataTable(artifactIds);
            param.Add("@artifactIds", artifactIdsTable);

            return (await ConnectionWrapper.QueryAsync<int>("AddHistory", param,
                commandType: CommandType.StoredProcedure));
        }
    }
}