using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdminStore.Models;

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

        Task<QueryResult<UserDto>> GetUsersAsync(Pagination pagination, Sorting sorting = null, string search = null,
            Func<Sorting, string> sort = null);
        Task<UserDto> GetUserDtoAsync(int userId);

        Task<int> AddUserAsync(User loginUser);

        Task UpdateUserAsync(User loginUser);
        Task<int> DeleteUsers(OperationScope body, string search, int sessionUserId);
        Task UpdateUserPasswordAsync(string login, string password);

        Task<QueryResult<GroupDto>> GetUserGroupsAsync(int userId, TabularData tabularData,
            Func<Sorting, string> sort = null);

        Task<int> DeleteUserFromGroupsAsync(int userId, OperationScope body);
    }
}