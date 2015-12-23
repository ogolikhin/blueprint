﻿using System.Collections.Generic;
using System.Threading.Tasks;
using AdminStore.Models;

namespace AdminStore.Repositories
{
    public interface ISqlUserRepository
    {
        Task<AuthenticationUser> GetUserByLoginAsync(string login);

        Task<LoginUser> GetLoginUserByIdAsync(int userId);

        Task<IEnumerable<LicenseTransactionUser>> GetLicenseTransactionUserInfoAsync(IEnumerable<int> userIds);

        Task<int> GetEffectiveUserLicenseAsync(int userId);

        Task UpdateUserOnInvalidLoginAsync(AuthenticationUser login);
    }
}
