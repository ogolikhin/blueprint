﻿using System;
using System.Data;
using System.Threading.Tasks;

namespace ServiceLibrary.Helpers
{
    public interface ISqlHelper
    {
        Task RunInTransactionAsync(string connectionString, Func<IDbTransaction, Task> action);

        Task<int> CreateRevisionInTransactionAsync(IDbTransaction transaction, int userId, string comment);
    }
}