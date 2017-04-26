using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Helpers;
using AdminStore.Models;
using Dapper;
using ServiceLibrary.Exceptions;
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

        public QueryResult GetUsers(TableSettings settings)
        {
            var total = 0;
            var users = GetUsersList(settings, out total).ToList();
            var result = new QueryResult()
            {
                Data = new Data(),
                Pagination = new Pagination()
                {
                    TotalCount = total,
                    Page = settings.Page,
                    PageSize = settings.PageSize,
                    Count = users.Count
                }
            };
            var mappedUsers = UserMapper.Map(users).ToArray();
            result.Data.Users = mappedUsers;
            return result;
        }

        private IEnumerable<User> GetUsersList(TableSettings settings, out int total)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Page", settings.Page);
            parameters.Add("@PageSize", settings.PageSize);
            parameters.Add("@SearchUser", settings.Filter);
            parameters.Add("@OrderField", string.IsNullOrEmpty(settings.Sort) ? "displayName" : settings.Sort);
            parameters.Add("@Total", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var usersList = (_connectionWrapper.Query<User>("GetUsers", parameters, commandType: CommandType.StoredProcedure)).ToList();
            total = parameters.Get<int>("Total");
            return usersList;
        }

        public async Task<User> GetUser(int userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            var result = await _connectionWrapper.QueryAsync<User>("GetUserDetails", parameters, commandType: CommandType.StoredProcedure);
            var enumerable = result as IList<User> ?? result.ToList();
            return enumerable.Any() ? enumerable.First() : new User();
        }

        public async Task<UserDto> GetUserDto(int userId)
        {
            var user = await GetUser(userId);
            return UserMapper.Map(user);
        }

        public async Task<bool> HasUserExceededPasswordRequestLimitAsync(string login)
        {
            const int passwordRequestLimit = 3;

            var prm = new DynamicParameters();
            prm.Add("@login", login);
            var result = (await _adminStorageConnectionWrapper.QueryAsync<int>("GetUserPasswordRecoveryRequestCount", prm, commandType: CommandType.StoredProcedure));

            return result.FirstOrDefault() >= passwordRequestLimit;
        }

        public async Task<int> AddUserAsync(User loginUser)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Login", loginUser.Login);
            parameters.Add("@Source", (int)loginUser.Source);
            parameters.Add("@InstanceAdminRoleId", loginUser.InstanceAdminRoleId);
            parameters.Add("@AllowFallback", loginUser.AllowFallback);
            parameters.Add("@Enabled", loginUser.Enabled);
            parameters.Add("@ExpirePassword", loginUser.ExpirePassword);
            parameters.Add("@DisplayName", loginUser.DisplayName);
            parameters.Add("@FirstName", loginUser.FirstName);
            parameters.Add("@LastName", loginUser.LastName);
            parameters.Add("@ImageId", loginUser.Image_ImageId);
            parameters.Add("@Password", loginUser.Password);
            parameters.Add("@UserSALT", loginUser.UserSALT);
            parameters.Add("@Email", loginUser.Email);
            parameters.Add("@Title", loginUser.Title);
            parameters.Add("@Department", loginUser.Department);
            if (loginUser.GroupMembership != null)
                parameters.Add("@GroupMembership", SqlConnectionWrapper.ToDataTable(loginUser.GroupMembership, "Int32Collection", "Int32Value"));
            parameters.Add("@Guest", loginUser.Guest);
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var userId = await _connectionWrapper.ExecuteScalarAsync<int>("AddUser", parameters, commandType: CommandType.StoredProcedure);
            var errorCode = parameters.Get<int?>("ErrorCode");

            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.GeneralSqlError:
                        throw new BadRequestException(ErrorMessages.GeneralErrorOfCreatingUser);

                    case (int)SqlErrorCodes.UserLoginExist:
                        throw new BadRequestException(ErrorMessages.LoginNameUnique);

                    default:
                        return userId;
                }
            }
            return userId;
        }

        internal class HashedPassword
        {
            internal string Password { get; set; }
            internal Guid UserSALT { get; set; }
        }
    }
}
