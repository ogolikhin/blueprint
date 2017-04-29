using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AdminStore.Models;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;

namespace AdminStore.Repositories
{
    public class InstanceRolesRepository : IInstanceRolesRepository
    {
        internal readonly ISqlConnectionWrapper _connectionWrapper;


        public InstanceRolesRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal InstanceRolesRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<IEnumerable<AdminRole>> GetInstanceRolesAsync()
        {
            var result = await _connectionWrapper.QueryAsync<AdminRole>("GetInstanceAdminRoles", commandType: CommandType.StoredProcedure);
            return result;
        }
    }
}