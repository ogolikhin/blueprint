﻿using System;
using System.Threading.Tasks;
using AdminStore.Models;

namespace AdminStore.Repositories
{
    public interface IUserRepository
    {
        Task<LoginUser> GetUserByLogin(string login);

        Task UpdateUserOnInvalidLogin(LoginUser login);
    }
}