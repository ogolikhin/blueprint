﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdminStore.Models;
using ServiceLibrary.Models;

namespace AdminStore.Repositories
{
    public interface ISqlUserRepository
    {
        Task<AuthenticationUser> GetUserByLoginAsync(string login);

        Task<LoginUser> GetLoginUserByIdAsync(int userId);

        Task<UserIcon> GetUserIconByUserIdAsync(int userId);

        Task<IEnumerable<LicenseTransactionUser>> GetLicenseTransactionUserInfoAsync(IEnumerable<int> userIds);

        Task<int> GetEffectiveUserLicenseAsync(int userId);

        Task UpdateUserOnInvalidLoginAsync(AuthenticationUser login);

        Task UpdateUserOnPasswordResetAsync(AuthenticationUser user);

        Task<bool> ValidateUserPasswordForHistoryAsync(int userId, string newPassword);

        Task<bool> CanUserResetPasswordAsync(string login);

        Task UpdatePasswordRecoveryTokensAsync(string login, Guid recoveryToken);

        Task<bool> HasUserExceededPasswordRequestLimitAsync(string login);

        Task<IEnumerable<PasswordRecoveryToken>> GetPasswordRecoveryTokensAsync(Guid token);

        Task<int> AddUserAsync(User loginUser);

        Task<bool> HasPermissionsAsync(int userId, InstanceAdminPrivileges[] instanceAdminPrivilegeses);
    }
}
