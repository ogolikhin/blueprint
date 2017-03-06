﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Helpers;
using AdminStore.Models;
using Dapper;
using ServiceLibrary.Repositories;
using ServiceLibrary.Helpers;

namespace AdminStore.Repositories
{
    public class SqlUserRepository : ISqlUserRepository
    {
        internal readonly ISqlConnectionWrapper _connectionWrapper;
        internal readonly ISqlConnectionWrapper _adminStorageConnectionWrapper;

        public SqlUserRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain), new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal SqlUserRepository(ISqlConnectionWrapper connectionWrapper, ISqlConnectionWrapper adminStorageConnectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
            _adminStorageConnectionWrapper = adminStorageConnectionWrapper;
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

        public async Task<UserIcon> GetUserIconByUserIdAsync(int userId)
        {
            var prm = new DynamicParameters();
            prm.Add("@UserId", userId);
            return (await _connectionWrapper.QueryAsync<UserIcon>("GetUserIconByUserId", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();
        }

        public async Task<IEnumerable<LicenseTransactionUser>> GetLicenseTransactionUserInfoAsync(IEnumerable<int> userIds)
        {
            var prm = new DynamicParameters();
            prm.Add("@UserIds", SqlConnectionWrapper.ToDataTable(userIds, "Int32Collection", "Int32Value"));
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

        public async Task UpdateUserOnPasswordResetAsync(AuthenticationUser user)
        {
            var prm = new DynamicParameters();
            prm.Add("@userId", user.Id);
            await _connectionWrapper.ExecuteAsync("AddCurrentUserPasswordToHistory", prm, commandType: CommandType.StoredProcedure);

            prm = new DynamicParameters();
            prm.Add("@Login", user.Login);
            prm.Add("@Password", user.Password);
            prm.Add("@UserSALT", user.UserSalt);
            await _connectionWrapper.ExecuteAsync("UpdateUserOnPasswordResetAsync", prm, commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> ValidateUserPasswordForHistoryAsync(int userId, string newPassword)
        {
            var prm = new DynamicParameters();
            prm.Add("@userId", userId);
            var passwordHistory = await _connectionWrapper.QueryAsync<HashedPassword>("GetLastUserPasswords", prm, commandType: CommandType.StoredProcedure);

            foreach (var password in passwordHistory)
            {
                var newHashedPassword = HashingUtilities.GenerateSaltedHash(newPassword, password.UserSALT);
                if (string.Equals(newHashedPassword, password.Password))
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<bool> CanUserResetPassword(string login)
        {
            string query = @"
SELECT COUNT([UserId])
FROM [Raptor].[dbo].[Users]
where [Login] = @Login
and [Source] = 0 --User is a DB user
and [Email] is not null
and ([AllowFallback] is NULL or [AllowFallback] = 1) --Fallback is enabled
and [Enabled] = 1
";

            var prm = new DynamicParameters();
            prm.Add("@login", login);
            var result = (await _connectionWrapper.QueryAsync<int>(query, prm, commandType: CommandType.Text));

            return result.FirstOrDefault() > 0;
        }

        public async Task UpdatePasswordRecoveryTokens(string login)
        {
            string query = @"
INSERT INTO [Blueprint_AdminStorage].[dbo].[PasswordRecoveryTokens]
([Login],[CreationTime],[RecoveryToken])
VALUES (@login, CURRENT_TIMESTAMP, NEWID())
";

            var prm = new DynamicParameters();
            prm.Add("@login", login);
            await _adminStorageConnectionWrapper.QueryAsync<int>(query, prm, commandType: CommandType.Text);
        }

        public async Task<bool> HasUserExceededPasswordRequestLimit(string login)
        {
            string query = @"
SELECT COUNT([Login])
FROM [Blueprint_AdminStorage].[dbo].[PasswordRecoveryTokens]
WHERE [Login] = @login
AND [CreationTime] > DATEADD(d,-1,CURRENT_TIMESTAMP)
";

            const int passwordRequestLimit = 3;

            var prm = new DynamicParameters();
            prm.Add("@login", login);
            var result = (await _connectionWrapper.QueryAsync<int>(query, prm, commandType: CommandType.Text));

            return result.FirstOrDefault() > passwordRequestLimit;
        }

        internal class HashedPassword
        {
            internal string Password { get; set; }
            internal Guid UserSALT { get; set; }
        }
    }
}
