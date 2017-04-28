﻿using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;

namespace AdminStore.Repositories
{
    public class SqlPrivilegesRepository : IPrivilegesRepository
    {
        internal readonly ISqlConnectionWrapper _connectionWrapper;

        public SqlPrivilegesRepository() : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal SqlPrivilegesRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<bool> IsUserHasPermissions(IEnumerable<int> permissionsList, int userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            var permissionsResult = (await _connectionWrapper.QueryAsync<int>("GetInstancePermissionsForUser", parameters, commandType: CommandType.StoredProcedure)).FirstOrDefault();

            var expectedPermissions = permissionsList.Select(permission => permission & permissionsResult).ToList();
            return permissionsList.ToList().OrderBy(p => p).SequenceEqual(expectedPermissions.OrderBy(p => p));
        }

        public async Task<InstanceAdminPrivileges> GetInstanceAdminPrivilegesAsync(int userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            return await _connectionWrapper.ExecuteScalarAsync<InstanceAdminPrivileges>("GetInstancePermissionsForUser", parameters, commandType: CommandType.StoredProcedure);
        }
    }
}