﻿using AdminStore.Models;
using ServiceLibrary.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace AdminStore.Repositories
{
    public interface IUserRepository
    {
        Task<AuthenticationUser> GetUserByLoginAsync(string login);

        Task<LoginUser> GetLoginUserByIdAsync(int userId);

        Task<UserIcon> GetUserIconByUserIdAsync(int userId);

        Task<IEnumerable<LicenseTransactionUser>> GetLicenseTransactionUserInfoAsync(IEnumerable<int> userIds);

        Task<int> GetEffectiveUserLicenseAsync(int userId);

        Task<IEnumerable<UserLicense>> GetEffectiveUserLicensesAsync(IEnumerable<int> userIds);

        Task UpdateUserOnInvalidLoginAsync(AuthenticationUser login);

        Task UpdateUserOnPasswordResetAsync(AuthenticationUser user);

        Task<bool> ValidateUserPasswordForHistoryAsync(int userId, string newPassword);

        Task<bool> CanUserResetPasswordAsync(string login);

        Task UpdatePasswordRecoveryTokensAsync(string login, Guid recoveryToken);

        Task<bool> HasUserExceededPasswordRequestLimitAsync(string login);

        Task<IEnumerable<PasswordRecoveryToken>> GetPasswordRecoveryTokensAsync(Guid token);

        Task<User> GetUserAsync(int userId);

        Task<QueryResult<UserDto>> GetUsersAsync(Pagination pagination, Sorting sorting = null, string search = null, Func<Sorting, string> sort = null);

        Task<UserDto> GetUserDtoAsync(int userId);

        Task<int> AddUserAsync(User loginUser, IDbTransaction transaction);

        Task<int> AddUserToGroupsAsync(int userId, OperationScope body, string search);

        Task UpdateUserAsync(User loginUser, IDbTransaction transaction);

        Task<List<int>> DeleteUsersAsync(OperationScope body, string search, int sessionUserId, IDbTransaction transaction);

        Task UpdateUserPasswordAsync(string login, string password);

        Task<QueryResult<GroupDto>> GetUserGroupsAsync(int userId, TabularData tabularData, Func<Sorting, string> sort = null);

        Task<int> DeleteUserFromGroupsAsync(int userId, OperationScope body);

        Task<bool> CheckUserHasProjectAdminRoleAsync(int sessionUserId);
        Task<bool> CheckIfAdminCanCreateUsers();
    }
}
