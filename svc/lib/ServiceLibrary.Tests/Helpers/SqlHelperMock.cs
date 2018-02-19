using System;
using System.Data;
using System.Threading.Tasks;

namespace ServiceLibrary.Helpers
{
    public class SqlHelperMock : ISqlHelper
    {
        public async Task RunInTransactionAsync(string connectionString, Func<IDbTransaction, long, Task> action)
        {
            await action(null, 0);
        }

        public async Task<T> RunInTransactionAsync<T>(string connectionString, Func<IDbTransaction, long, Task<T>> action)
        {
            return await action(null, 0);
        }

        public async Task<int> CreateRevisionInTransactionAsync(IDbTransaction transaction, int userId, string comment)
        {
            return await Task.FromResult(1);
        }

        public Task<T> RetryOnSqlDeadlockAsync<T>(Func<Task<T>> action, int retryCount, int delayAfterAttempt = 3, int millisecondsDelay = 2000)
        {
            return action();
        }
    }
}
