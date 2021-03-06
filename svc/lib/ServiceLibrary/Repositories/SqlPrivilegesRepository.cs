﻿using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using System.Data;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
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

        public async Task<ProjectAdminPrivileges> GetProjectAdminPermissionsAsync(int userId, int projectId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@ProjectId", projectId);
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var permissions = await _connectionWrapper.ExecuteScalarAsync<ProjectAdminPrivileges>
            (
                "GetProjectAdminPermissions",
                parameters,
                commandType: CommandType.StoredProcedure);

            var errorCode = parameters.Get<int?>("ErrorCode");

            if (errorCode.HasValue && errorCode.Value == (int)SqlErrorCodes.ProjectWithCurrentIdNotExist)
            {
                throw new ResourceNotFoundException(ErrorMessages.ProjectNotExist, ErrorCodes.ResourceNotFound);
            }

            return permissions;
        }
    }
}
