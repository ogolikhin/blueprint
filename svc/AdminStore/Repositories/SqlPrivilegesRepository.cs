﻿using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using System.Data;
using System.Threading.Tasks;

namespace AdminStore.Repositories
{
    public class SqlPrivilegesRepository : IPrivilegesRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;

        public SqlPrivilegesRepository() : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal SqlPrivilegesRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<InstanceAdminPrivileges> GetInstanceAdminPrivilegesAsync(int userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);

            return await _connectionWrapper.ExecuteScalarAsync<InstanceAdminPrivileges>("GetInstancePermissionsForUser", parameters, commandType: CommandType.StoredProcedure);
        }
    }
}
