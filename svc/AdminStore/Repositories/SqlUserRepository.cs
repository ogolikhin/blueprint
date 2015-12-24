﻿using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Models;
using Dapper;
using ServiceLibrary.Repositories;

namespace AdminStore.Repositories
{
    public class SqlUserRepository : ISqlUserRepository
    {
        internal readonly ISqlConnectionWrapper _connectionWrapper;

        public SqlUserRepository()
            : this(new SqlConnectionWrapper(WebApiConfig.RaptorMain))
        {
        }

        internal SqlUserRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<AuthenticationUser> GetUserByLoginAsync(string login)
        {
            var prm = new DynamicParameters();
            prm.Add("@Login", login);
            return (await _connectionWrapper.QueryAsync<AuthenticationUser>("GetUserByLogin", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }

        public async Task<int> GetEffectiveUserLicenseAsync(int userId)
        {
            var prm = new DynamicParameters();
            prm.Add("@UserId", userId);
            return (await _connectionWrapper.QueryAsync<int>("GetEffectiveUserLicense", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }

        public async Task<LoginUser> GetLoginUserByIdAsync(int userId)
        {
            var prm = new DynamicParameters();
            prm.Add("@UserId", userId);
            return (await _connectionWrapper.QueryAsync<LoginUser>("GetLoginUserById", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }

        public async Task<IEnumerable<LicenseTransactionUser>> GetLicenseTransactionUserInfoAsync(IEnumerable<int> userIds)
        {
            var prm = new DynamicParameters();
            var userIdTable = new DataTable();
            userIdTable.SetTypeName("Int32Collection");
            userIdTable.Columns.Add("Int32Value", typeof(int));
            foreach (var id in userIds)
            {
                userIdTable.Rows.Add(id);
            }
            prm.Add("@UserIds", userIdTable);
            return await _connectionWrapper.QueryAsync<LicenseTransactionUser>("GetLicenseTransactionUser", prm, commandType: CommandType.StoredProcedure);
        }

        public async Task UpdateUserOnInvalidLoginAsync(AuthenticationUser user)
        {
            var prm = new DynamicParameters();
            prm.Add("@Login", user.Login);
            prm.Add("@Enabled", user.IsEnabled);
            prm.Add("@InvalidLogonAttemptsNumber", user.InvalidLogonAttemptsNumber);
            prm.Add("@LastInvalidLogonTimeStamp", user.LastInvalidLogonTimeStamp);
            await _connectionWrapper.ExecuteAsync("UpdateUserOnInvalidLogin", prm, commandType: CommandType.StoredProcedure);
        }
    }
}
