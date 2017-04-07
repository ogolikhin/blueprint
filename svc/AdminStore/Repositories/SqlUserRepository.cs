using System;
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
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain), new SqlConnectionWrapper(WebApiConfig.AdminStorage))
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

        public async Task<bool> CanUserResetPasswordAsync(string login)
        {
            var prm = new DynamicParameters();
            prm.Add("@login", login);
            var result = (await _connectionWrapper.QueryAsync<int>("GetCanUserResetPassword", prm, commandType: CommandType.StoredProcedure));

            return result.FirstOrDefault() > 0;
        }

        public async Task UpdatePasswordRecoveryTokensAsync(string login, Guid recoveryToken)
        {
            var prm = new DynamicParameters();
            prm.Add("@login", login);
            prm.Add("@recoverytoken", recoveryToken);
            await _adminStorageConnectionWrapper.QueryAsync<int>("SetUserPasswordRecoveryToken", prm, commandType: CommandType.StoredProcedure);
        }
        public async Task<IEnumerable<PasswordRecoveryToken>> GetPasswordRecoveryTokensAsync(Guid token)
        {
            var prm = new DynamicParameters();
            prm.Add("@token", token);
            return await _adminStorageConnectionWrapper.QueryAsync<PasswordRecoveryToken>("GetUserPasswordRecoveryTokens", prm, commandType: CommandType.StoredProcedure);
        }

        public  Task<int> AddUser(User loginUser)
        {
            var prm = new DynamicParameters();
            prm.Add("@Login", loginUser.Login);
            prm.Add("@Source", (int)loginUser.Source);
            prm.Add("@InstanceAdminRoleId", loginUser.InstanceAdminRoleId);
            prm.Add("@AllowFallback", loginUser.AllowFallback);
            prm.Add("@Enabled", loginUser.Enabled);
            prm.Add("@ExpirePassword", loginUser.ExpirePassword);
            prm.Add("@DisplayName", loginUser.DisplayName);
            prm.Add("@FirstName", loginUser.FirstName);
            prm.Add("@LastName", loginUser.LastName);
            prm.Add("@ImageId", loginUser.ImageId);
            prm.Add("@Password", loginUser.Password);
            prm.Add("@UserSALT", loginUser.UserSALT);
            prm.Add("@Email", loginUser.Email);
            prm.Add("@Title", loginUser.Title);
            prm.Add("@Department", loginUser.Department);
            prm.Add("@GroupMembership", SqlConnectionWrapper.ToDataTable(loginUser.GroupMembership, "Int32Collection", "Int32Value"));
            prm.Add("@Guest", loginUser.Guest);
            return   _connectionWrapper.ExecuteScalarAsync<int>("AddUser", prm, commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> HasUserExceededPasswordRequestLimitAsync(string login)
        {
            const int passwordRequestLimit = 3;

            var prm = new DynamicParameters();
            prm.Add("@login", login);
            var result = (await _adminStorageConnectionWrapper.QueryAsync<int>("GetUserPasswordRecoveryRequestCount", prm, commandType: CommandType.StoredProcedure));

            return result.FirstOrDefault() >= passwordRequestLimit;
        }

        internal class HashedPassword
        {
            internal string Password { get; set; }
            internal Guid UserSALT { get; set; }
        }
    }
}
