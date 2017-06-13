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
        public async Task RunInTransactionAsync(string connectionString, Func<IDbTransaction, Task> action)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    await action(transaction);
                    transaction.Commit();
                }
            }
        }

        public async Task<int> CreateRevisionInTransactionAsync(IDbTransaction transaction, int userId, string comment)
        {
            var prm = new DynamicParameters();
            prm.Add("@userId", userId);
            prm.Add("@comment", comment);

            var revision = (await transaction.Connection.QueryAsync<int>("CreateRevision", prm, transaction, commandType: CommandType.StoredProcedure)).FirstOrDefault();
            return revision;
        }
    }
}