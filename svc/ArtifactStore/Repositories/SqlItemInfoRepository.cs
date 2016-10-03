using ArtifactStore.Repositories;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ArtifactStore.Helpers
{
    internal class SqlItemInfoRepository
    {
        internal readonly ISqlConnectionWrapper _connectionWrapper;

        internal SqlItemInfoRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }
        internal SqlItemInfoRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }
        internal async Task<IEnumerable<ItemLabel>> GetItemsLabels(int userId, IEnumerable<int> itemIds, bool addDrafts = true, int revisionId = int.MaxValue)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@itemIds", SqlConnectionWrapper.ToDataTable(itemIds, "Int32Collection", "Int32Value"));
            parameters.Add("@userId", userId);
            parameters.Add("@addDrafts", addDrafts);
            parameters.Add("@revisionId", revisionId);
            return (await _connectionWrapper.QueryAsync<ItemLabel>("GetItemsLabels", parameters, commandType: CommandType.StoredProcedure));
        }

        internal async Task<IEnumerable<ItemDetails>> GetItemsDetails(int userId, IEnumerable<int> itemIds, bool addDrafts = true, int revisionId = int.MaxValue)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            parameters.Add("@itemIds", SqlConnectionWrapper.ToDataTable(itemIds, "Int32Collection", "Int32Value"));
            parameters.Add("@addDrafts", addDrafts);
            parameters.Add("@revisionId", revisionId);
            return await _connectionWrapper.QueryAsync<ItemDetails>("GetItemsDetails", parameters, commandType: CommandType.StoredProcedure);
        }

        internal async Task<int> GetRevisionIdByVersionIndex(int artifactId, int versionIndex)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@artifactId", artifactId);
            parameters.Add("@versionIndex", versionIndex);
            return (await _connectionWrapper.QueryAsync<int>("GetRevisionIdByVersionIndex", parameters, commandType: CommandType.StoredProcedure)).SingleOrDefault();
        }

    }
}