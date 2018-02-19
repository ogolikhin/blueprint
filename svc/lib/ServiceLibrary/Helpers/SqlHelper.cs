using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace ServiceLibrary.Helpers
{
    public class SqlHelper : ISqlHelper
    {
        /// <summary>
        /// See http://msdn.microsoft.com/en-us/library/aa337376.aspx
        /// </summary>
        private const int DeadlockSqlExceptionErrorCode = 1205;

        public async Task RunInTransactionAsync(string connectionString, Func<IDbTransaction, long, Task> action)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    var transactionId = await CreateTransactionId(transaction);
                    await action(transaction, transactionId);
                    transaction.Commit();
                }
            }
        }

        public async Task<T> RunInTransactionAsync<T>(string connectionString, Func<IDbTransaction, long, Task<T>> action)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    var transactionId = await CreateTransactionId(transaction);
                    var result = await action(transaction, transactionId);
                    transaction.Commit();

                    return result;
                }
            }
        }

        private static async Task<long> CreateTransactionId(SqlTransaction transaction)
        {
            return await transaction.Connection.ExecuteScalarAsync<long>("[dbo].[CreateTransactionId]", transaction: transaction, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> CreateRevisionInTransactionAsync(IDbTransaction transaction, int userId, string comment)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            parameters.Add("@comment", comment);

            var revision = (await transaction.Connection.QueryAsync<int>("CreateRevision", parameters, transaction, commandType: CommandType.StoredProcedure)).FirstOrDefault();

            return revision;
        }

        public async Task<T> RetryOnSqlDeadlockAsync<T>(Func<Task<T>> action, int retryCount, int delayAfterAttempt = 3, int millisecondsDelay = 2000)
        {
            for (var attempt = 0; ; attempt++)
            {
                try
                {
                    return await action();
                }
                catch (SqlException ex)
                {
                    if (!IsDeadlockException(ex))
                        throw;

                    if (attempt >= retryCount)
                        throw;

                    if (attempt >= delayAfterAttempt)
                    {
                        await Task.Delay(millisecondsDelay);
                    }
                }
            }
        }

        private static bool IsDeadlockException(SqlException sqlException)
        {
            return sqlException != null && sqlException.Number == DeadlockSqlExceptionErrorCode;
        }
    }
}
