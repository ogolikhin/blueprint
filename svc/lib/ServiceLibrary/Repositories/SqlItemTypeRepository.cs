using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.ItemType;

namespace ServiceLibrary.Repositories
{
    public interface ISqlItemTypeRepository
    {
        Task<SqlItemType> GetCustomItemTypeForProvidedStandardItemTypeIdInProject(int projectId,
            int standardItemTypeId);
    }

    public class SqlItemTypeRepository : ISqlItemTypeRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;

        public SqlItemTypeRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        public SqlItemTypeRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<SqlItemType> GetCustomItemTypeForProvidedStandardItemTypeIdInProject(int projectId,
            int standardItemTypeId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@projectId", projectId);
            parameters.Add("@standardItemTypeId", standardItemTypeId);

            return (await _connectionWrapper.QueryAsync<SqlItemType>("GetCustomItemTypeForProvidedStandardItemType", parameters, commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }
    }
}
