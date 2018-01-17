using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Repositories
{
    public class SqlCollectionsRepository : ICollectionsRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;

        internal const string GetArtifactIdsInCollectionQuery =
            "SELECT * FROM [dbo].[GetArtifactIdsInCollection](@userId, @collectionId, @addDrafts)";

        public SqlCollectionsRepository() : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        public SqlCollectionsRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<IReadOnlyList<int>> GetContentArtifactIdsAsync(int collectionId, int userId, bool addDrafts = true)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            parameters.Add("@collectionId", collectionId);
            parameters.Add("@addDrafts", addDrafts);

            var result = await _connectionWrapper.QueryAsync<int>(
                GetArtifactIdsInCollectionQuery,
                parameters,
                commandType: CommandType.Text);

            return result.ToList();
        }
    }
}
