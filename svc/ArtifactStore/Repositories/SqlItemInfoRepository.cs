using ArtifactStore.Repositories;
using Dapper;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ArtifactStore.Helpers
{
    internal class ItemInfoHelper
    {
        private ISqlConnectionWrapper _connectionWrapper;
        public ItemInfoHelper(ISqlConnectionWrapper connectionWrapper)
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
    }
}