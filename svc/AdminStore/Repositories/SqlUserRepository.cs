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

        public QueryResult<UserDto> GetUsers(Pagination pagination, Sorting sorting = null, string search = null, Func<Sorting, string> sort = null)
        {
            int total;
            var orderField = string.Empty;
            if (sort != null && sorting != null)
            {
                orderField = sort(sorting);
            }
            var users = GetUsersList(pagination, orderField, search, out total).ToList();
            return new QueryResult<UserDto>()
            {
                Total = total,
                Items = UserMapper.Map(users)
            };
        }


        private IEnumerable<User> GetUsersList(Pagination pagination, string orderField, string search, out int total)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Offset", pagination.Offset);
            parameters.Add("@Limit", pagination.Limit);
            parameters.Add("@Search", search ?? string.Empty);
            parameters.Add("@OrderField", string.IsNullOrEmpty(orderField) ? "displayName" : orderField);
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

        public async Task<int> DeleteUsers(OperationScope body, string search)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserIds", SqlConnectionWrapper.ToDataTable(body.Ids));
            parameters.Add("@Search", search);
            parameters.Add("@SelectAll", body.SelectAll);
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var result = await _connectionWrapper.ExecuteScalarAsync<int>("DeleteUsers", parameters, commandType: CommandType.StoredProcedure);
            var errorCode = parameters.Get<int?>("ErrorCode");
            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.GeneralSqlError:
                        throw new BadRequestException(ErrorMessages.GeneralErrorOfDeletingUsers);
                }
            }
            return result;
        }

        public async Task UpdateUserAsync(User loginUser)
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
            parameters.Add("@UserSALT", loginUser.UserSALT);
            parameters.Add("@Email", loginUser.Email);
            parameters.Add("@Title", loginUser.Title);
            parameters.Add("@Department", loginUser.Department);
            if (loginUser.GroupMembership != null)
                parameters.Add("@GroupMembership", SqlConnectionWrapper.ToDataTable(loginUser.GroupMembership, "Int32Collection", "Int32Value"));
            parameters.Add("@Guest", loginUser.Guest);
            parameters.Add("@UserId", loginUser.Id);
            parameters.Add("@CurrentVersion", loginUser.CurrentVersion);
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await _connectionWrapper.ExecuteAsync("UpdateUser", parameters, commandType: CommandType.StoredProcedure);

            var errorCode = parameters.Get<int?>("ErrorCode");
            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.GeneralSqlError:
                        throw new BadRequestException(ErrorMessages.GeneralErrorOfUpdatingUser);

                    case (int)SqlErrorCodes.UserLoginExist:
                        throw new BadRequestException(ErrorMessages.LoginNameUnique);

                    case (int)SqlErrorCodes.UserLoginNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.UserNotExist);

                    case (int)SqlErrorCodes.UserVersionsNotEqual:
                        throw new ConflictException(ErrorMessages.UserVersionsNotEqual);
                }
            }
        }

        internal class HashedPassword
        {
            internal string Password { get; set; }
            internal Guid UserSALT { get; set; }
        }
    }
}
