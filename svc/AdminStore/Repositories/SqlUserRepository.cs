﻿using AdminStore.Helpers;
using AdminStore.Models;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace AdminStore.Repositories
{
    public class SqlUserRepository : IUserRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;
        private readonly ISqlConnectionWrapper _adminStorageConnectionWrapper;

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
            prm.Add("@UserIds", SqlConnectionWrapper.ToDataTable(new[] { userId }));
            var result = (await _connectionWrapper.QueryAsync<UserLicense>("GetEffectiveUserLicense", prm, commandType: CommandType.StoredProcedure)).FirstOrDefault();

            return result?.LicenseType ?? 0;
        }

        public async Task<IEnumerable<UserLicense>> GetEffectiveUserLicensesAsync(IEnumerable<int> userIds)
        {
            var prm = new DynamicParameters();
            prm.Add("@UserIds", SqlConnectionWrapper.ToDataTable(userIds));

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
            prm.Add("@UserIds", SqlConnectionWrapper.ToDataTable(userIds));

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
            await _adminStorageConnectionWrapper.QueryAsync<int>("[AdminStore].SetUserPasswordRecoveryToken", prm, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<PasswordRecoveryToken>> GetPasswordRecoveryTokensAsync(Guid token)
        {
            var prm = new DynamicParameters();
            prm.Add("@token", token);

            return await _adminStorageConnectionWrapper.QueryAsync<PasswordRecoveryToken>("[AdminStore].GetUserPasswordRecoveryTokens", prm, commandType: CommandType.StoredProcedure);
        }

        public async Task<QueryResult<UserDto>> GetUsersAsync(Pagination pagination, Sorting sorting = null, string search = null, Func<Sorting, string> sort = null)
        {
            var orderField = string.Empty;
            if (sort != null && sorting != null)
            {
                orderField = sort(sorting);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = UsersHelper.ReplaceWildcardCharacters(search);
            }

            var result = await GetUsersInternalAsync(pagination, orderField, search);

            return new QueryResult<UserDto>
            {
                Items = UserMapper.Map(result.Items),
                Total = result.Total
            };
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
            var result = (await _connectionWrapper.QueryAsync<User>("GetUserDetails", parameters, commandType: CommandType.StoredProcedure)).FirstOrDefault();

            return result;
        }

        public async Task<UserDto> GetUserDtoAsync(int userId)
        {
            var user = await GetUserAsync(userId);

            return user == null ? null : UserMapper.Map(user);
        }

        public async Task<bool> HasUserExceededPasswordRequestLimitAsync(string login)
        {
            const int passwordRequestLimit = 3;

            var prm = new DynamicParameters();
            prm.Add("@login", login);
            var result = (await _adminStorageConnectionWrapper.QueryAsync<int>("[AdminStore].GetUserPasswordRecoveryRequestCount", prm, commandType: CommandType.StoredProcedure));

            return result.FirstOrDefault() >= passwordRequestLimit;
        }

        public async Task<int> AddUserAsync(User loginUser, IDbTransaction transaction)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Login", loginUser.Login);
            parameters.Add("@Source", (int)loginUser.Source);
            parameters.Add("@InstanceAdminRoleId", loginUser.InstanceAdminRoleId);
            parameters.Add("@AllowFallback", loginUser.AllowFallback);
            parameters.Add("@Enabled", loginUser.Enabled);
            parameters.Add("@ExpirePassword", loginUser.ExpirePassword);
            parameters.Add("@DisplayName", loginUser.DisplayName);
            parameters.Add("@FirstName", string.IsNullOrWhiteSpace(loginUser.FirstName) ? string.Empty : loginUser.FirstName);
            parameters.Add("@LastName", string.IsNullOrWhiteSpace(loginUser.LastName) ? string.Empty : loginUser.LastName);
            parameters.Add("@ImageId", loginUser.Image_ImageId);
            parameters.Add("@Password", loginUser.Password);
            parameters.Add("@UserSALT", loginUser.UserSALT);
            parameters.Add("@Email", string.IsNullOrWhiteSpace(loginUser.Email) ? string.Empty : loginUser.Email);
            parameters.Add("@Title", string.IsNullOrWhiteSpace(loginUser.Title) ? string.Empty : loginUser.Title);
            parameters.Add("@Department", string.IsNullOrWhiteSpace(loginUser.Department) ? string.Empty : loginUser.Department);

            if (loginUser.GroupMembership != null)
            {
                parameters.Add("@GroupMembership", SqlConnectionWrapper.ToDataTable(loginUser.GroupMembership));
            }

            parameters.Add("@Guest", loginUser.Guest);
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            int userId;
            if (transaction == null)
            {
                userId = await _connectionWrapper.ExecuteScalarAsync<int>("AddUser", parameters, commandType: CommandType.StoredProcedure);
            }
            else
            {
                userId = await transaction.Connection.ExecuteScalarAsync<int>("AddUser", parameters, transaction, commandType: CommandType.StoredProcedure);
            }

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

        public async Task<List<int>> DeleteUsersAsync(OperationScope scope, string search, int sessionUserId, IDbTransaction transaction)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = UsersHelper.ReplaceWildcardCharacters(search);
            }

            var parameters = new DynamicParameters();
            parameters.Add("@UserIds", SqlConnectionWrapper.ToDataTable(scope.Ids));
            parameters.Add("@Search", search);
            parameters.Add("@SelectAll", scope.SelectAll);
            parameters.Add("@SessionUserId", sessionUserId);
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            List<int> result;
            if (transaction == null)
            {
                result = (await _connectionWrapper.QueryAsync<int>("DeleteUsers", parameters, commandType: CommandType.StoredProcedure)).ToList();
            }
            else
            {
                result = (await transaction.Connection.QueryAsync<int>("DeleteUsers", parameters, transaction, commandType: CommandType.StoredProcedure)).ToList();
            }

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

        public async Task UpdateUserAsync(User loginUser, IDbTransaction transaction)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Login", loginUser.Login);
            parameters.Add("@InstanceAdminRoleId", loginUser.InstanceAdminRoleId);
            parameters.Add("@AllowFallback", loginUser.AllowFallback);
            parameters.Add("@Enabled", loginUser.Enabled);
            parameters.Add("@ExpirePassword", loginUser.ExpirePassword);
            parameters.Add("@DisplayName", loginUser.DisplayName);
            parameters.Add("@FirstName", string.IsNullOrWhiteSpace(loginUser.FirstName) ? string.Empty : loginUser.FirstName);
            parameters.Add("@LastName", string.IsNullOrWhiteSpace(loginUser.LastName) ? string.Empty : loginUser.LastName);
            parameters.Add("@ImageId", loginUser.Image_ImageId);
            parameters.Add("@Email", string.IsNullOrWhiteSpace(loginUser.Email) ? string.Empty : loginUser.Email);
            parameters.Add("@Title", string.IsNullOrWhiteSpace(loginUser.Title) ? string.Empty : loginUser.Title);
            parameters.Add("@Department", string.IsNullOrWhiteSpace(loginUser.Department) ? string.Empty : loginUser.Department);
            parameters.Add("@Guest", loginUser.Guest);
            parameters.Add("@UserId", loginUser.Id);
            parameters.Add("@CurrentVersion", loginUser.CurrentVersion);
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            if (transaction == null)
            {
                await _connectionWrapper.ExecuteAsync("UpdateUser", parameters, commandType: CommandType.StoredProcedure);
            }
            else
            {
                await transaction.Connection.ExecuteAsync("UpdateUser", parameters, transaction, commandType: CommandType.StoredProcedure);
            }

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

            if (!string.IsNullOrWhiteSpace(tabularData.Search))
            {
                tabularData.Search = UsersHelper.ReplaceWildcardCharacters(tabularData.Search);
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
                        throw new ResourceNotFoundException(ErrorMessages.UserNotExist, ErrorCodes.ResourceNotFound);
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

        public async Task<int> AddUserToGroupsAsync(int userId, OperationScope body, string search)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = UsersHelper.ReplaceWildcardCharacters(search);
            }

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@GroupMembership", SqlConnectionWrapper.ToDataTable(body.Ids, "Int32Collection", "Int32Value"));
            parameters.Add("@SelectAll", body.SelectAll);
            parameters.Add("@Search", search);
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = await _connectionWrapper.ExecuteScalarAsync<int>("AddUserToGroups", parameters, commandType: CommandType.StoredProcedure);
            var errorCode = parameters.Get<int?>("ErrorCode");
            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.UserLoginNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.UserNotExist, ErrorCodes.ResourceNotFound);
                }
            }

            return result;
        }

        public async Task<int> DeleteUserFromGroupsAsync(int userId, OperationScope body)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@SelectAll", body.SelectAll);

            if (body.Ids != null)
            {
                parameters.Add("@GroupMembership", SqlConnectionWrapper.ToDataTable(body.Ids));
            }

            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = await _connectionWrapper.ExecuteScalarAsync<int>("DeleteUserFromGroups", parameters, commandType: CommandType.StoredProcedure);
            var errorCode = parameters.Get<int?>("ErrorCode");

            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.GeneralSqlError:
                        throw new BadRequestException(ErrorMessages.GeneralErrorOfDeletingUserFromGroups);

                    case (int)SqlErrorCodes.UserLoginNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.UserNotExist, ErrorCodes.ResourceNotFound);
                }
            }

            return result;
        }

        public async Task<bool> CheckUserHasProjectAdminRoleAsync(int sessionUserId)
        {
            var prm = new DynamicParameters();
            prm.Add("@UserId", sessionUserId);

            return await _connectionWrapper.ExecuteScalarAsync<bool>("IsProjectAdminForAnyNonDeletedProject", prm, commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> CheckIfAdminCanCreateUsers()
        {
            var result = await _connectionWrapper.ExecuteScalarAsync<bool>("select dbo.CanCreateUsers()", commandType: CommandType.Text);
            return result;
        }

        internal class HashedPassword
        {
            internal string Password { get; set; }

            internal Guid UserSALT { get; set; }
        }
    }
}
