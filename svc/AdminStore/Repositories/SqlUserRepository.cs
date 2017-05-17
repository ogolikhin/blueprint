using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Helpers;
using AdminStore.Models;
using AdminStore.Models.Enums;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Repositories;
using ServiceLibrary.Helpers;

namespace AdminStore.Repositories
{
    public class SqlUserRepository : IUserRepository
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
            prm.Add("@UserIds", SqlConnectionWrapper.ToDataTable(new[] { userId }, "Int32Collection", "Int32Value"));
            var result = (await _connectionWrapper.QueryAsync<UserLicense>("GetEffectiveUserLicense", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();

            return result != null ? result.LicenseType : 0;
        }

        public async Task<IEnumerable<UserLicense>> GetEffectiveUserLicensesAsync(IEnumerable<int> userIds)
        {
            var prm = new DynamicParameters();
            prm.Add("@UserIds", SqlConnectionWrapper.ToDataTable(userIds, "Int32Collection", "Int32Value"));
            return (await _connectionWrapper.QueryAsync<UserLicense>("GetEffectiveUserLicense", prm, commandType: CommandType.StoredProcedure)).ToList();
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

        public async Task<QueryResult<UserDto>> GetUsersAsync(Pagination pagination, Sorting sorting = null, string search = null, Func<Sorting, string> sort = null)
        {
            var orderField = string.Empty;
            if (sort != null && sorting != null)
            {
                orderField = sort(sorting);
            }
            var result = await GetUsersInternalAsync(pagination, orderField, search);
            await PopulateEffectiveLicenseTypes(result.Items);
            if (sorting?.Sort != null && sorting.Sort.ToLower() == "licensetype")
            {
                result.Items = sorting.Order == SortOrder.Asc ? result.Items.OrderBy(e => e.LicenseType) : result.Items.OrderByDescending(e => e.LicenseType);
            }
            return new QueryResult<UserDto>()
            {
                Items = UserMapper.Map(result.Items),
                Total = result.Total
            };
        }

        private async Task PopulateEffectiveLicenseTypes(IEnumerable<User> users)
        {
            var licenseTypes = (await GetEffectiveUserLicensesAsync(users.Select(u => u.Id)))
                .ToDictionary(l => l.UserId);

            foreach (var user in users)
            {
                user.LicenseType = licenseTypes[user.Id].LicenseType;
            }
        }

        private async Task<QueryResult<User>> GetUsersInternalAsync(Pagination pagination, string orderField, string search)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Offset", pagination.Offset);
            parameters.Add("@Limit", pagination.Limit);
            parameters.Add("@Search", search ?? string.Empty);
            parameters.Add("@OrderField", string.IsNullOrEmpty(orderField) ? "displayName" : orderField);
            parameters.Add("@Total", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var users = (await _connectionWrapper.QueryAsync<User>("GetUsers", parameters, commandType: CommandType.StoredProcedure)).ToList();
            var total = parameters.Get<int>("Total");
            return new QueryResult<User>
            {
                Items = users,
                Total = total
            };
        }

        public async Task<User> GetUserAsync(int userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            var result = await _connectionWrapper.QueryAsync<User>("GetUserDetails", parameters, commandType: CommandType.StoredProcedure);
            var enumerable = result as IList<User> ?? result.ToList();
            return enumerable.Any() ? enumerable.First() : new User();
        }

        public async Task<UserDto> GetUserDtoAsync(int userId)
        {
            var user = await GetUserAsync(userId);

            user.LicenseType = await GetEffectiveUserLicenseAsync(userId);

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

        public async Task<int> DeleteUsers(OperationScope body, string search, int sessionUserId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserIds", SqlConnectionWrapper.ToDataTable(body.Ids));
            parameters.Add("@Search", search);
            parameters.Add("@SelectAll", body.SelectAll);
            parameters.Add("@SessionUserId", sessionUserId);
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

        public async Task UpdateUserPasswordAsync(string login, string password)
        {
            var userSalt = Guid.NewGuid();
            var newPassword = HashingUtilities.GenerateSaltedHash(password, userSalt);

            var parameters = new DynamicParameters();
            parameters.Add("@Login", login);
            parameters.Add("@UserSALT", userSalt);
            parameters.Add("@Password", newPassword);
            await _connectionWrapper.ExecuteAsync("UpdateUserOnPasswordResetAsync", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task UpdateUserAsync(User loginUser)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Login", loginUser.Login);
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

        public async Task<QueryResult<GroupDto>> GetUserGroupsAsync(int userId, TabularData tabularData, Func<Sorting, string> sort = null)
        {
            var orderField = string.Empty;
            if (sort != null && tabularData.Sorting != null)
            {
                orderField = sort(tabularData.Sorting);
            }
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@Offset", tabularData.Pagination.Offset);
            parameters.Add("@Limit", tabularData.Pagination.Limit);
            parameters.Add("@OrderField", orderField);
            parameters.Add("@Search", tabularData.Search);
            parameters.Add("@Total", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var userGroups = await _connectionWrapper.QueryAsync<Group>("GetUsersGroups", parameters, commandType: CommandType.StoredProcedure);
            var total = parameters.Get<int?>("Total");
            var errorCode = parameters.Get<int?>("ErrorCode");

            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.GeneralSqlError:
                        throw new BadRequestException(ErrorMessages.GeneralErrorOfGettingUserGroups);

                    case (int)SqlErrorCodes.UserLoginNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.UserNotExist);
                }
            }

            if (!total.HasValue)
            {
                throw new BadRequestException(ErrorMessages.TotalNull);
            }

            var mappedGroups = GroupMapper.Map(userGroups);

            var queryDataResult = new QueryResult<GroupDto>() { Items = mappedGroups, Total = total.Value };
            return queryDataResult;
        }

        public async Task<int> DeleteUserFromGroupsAsync(int userId, OperationScope body)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@SelectAll", body.SelectAll);
            if (body.Ids != null)
                parameters.Add("@GroupMembership", SqlConnectionWrapper.ToDataTable(body.Ids, "Int32Collection", "Int32Value"));
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = await _connectionWrapper.ExecuteScalarAsync<int>("DeleteUserFromGroups", parameters, commandType: CommandType.StoredProcedure);
            var errorCode = parameters.Get<int?>("ErrorCode");
            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.UserLoginNotExist:
                        throw new BadRequestException(ErrorMessages.UserNotExist);

                    case (int)SqlErrorCodes.GeneralSqlError:
                        throw new BadRequestException(ErrorMessages.GeneralErrorOfDeletingUserFromGroups);
                }
            }
            return result;
        }

        internal class HashedPassword
        {
            internal string Password { get; set; }
            internal Guid UserSALT { get; set; }
        }
    }
}
