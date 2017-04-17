using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Models;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;

namespace AdminStore.Repositories
{
    public class SqlPrivilegesRepository : ISqlPrivilegesRepository
    {
        internal readonly ISqlConnectionWrapper _connectionWrapper;
        public SqlPrivilegesRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal SqlPrivilegesRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<int> GetUserPermissionsAsync(int userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            return  (await _connectionWrapper.QueryAsync<int>("GetUserPermissions", parameters, commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }
    }
}