using System;
using System.Data;
using System.Threading.Tasks;

namespace ServiceLibrary.Helpers
{
    public class SqlHelperMock : ISqlHelper
    {
        public async Task RunInTransactionAsync(string connectionString, Func<IDbTransaction, Task> action)
        {
            await action(null);
        }

        public async Task<T> RunInTransactionAsync<T>(string connectionString, Func<IDbTransaction, Task<T>> action)
        {
            return await action(null);
        }

        public async Task<int> CreateRevisionInTransactionAsync(IDbTransaction transaction, int userId, string comment)
        {
            return await Task.FromResult(1);
        }
    }
}
