using System;
using System.Data;
using System.Threading.Tasks;

namespace ServiceLibrary.Helpers
{
    public interface ISqlHelper
    {
        Task RunInTransactionAsync(string connectionString, Func<IDbTransaction, Task> action);

        Task<T> RunInTransactionAsync<T>(string connectionString, Func<IDbTransaction, Task<T>> action);

        Task<int> CreateRevisionInTransactionAsync(IDbTransaction transaction, int userId, string comment);

        Task<T> RetryOnSqlDealLockAsync<T>(Func<Task<T>> action, int retryCount, int delayAfterAttempt = 3, int millisecondsDelay = 2000);
    }
}