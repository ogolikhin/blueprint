using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;

namespace ServiceLibrary.Repositories
{
    public interface ISqlConnectionWrapper
    {
        DbConnection CreateConnection();
        Task<int> ExecuteAsync(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);
    }

    public class SqlConnectionWrapper : ISqlConnectionWrapper
    {
        private readonly string _connectionString;

        public SqlConnectionWrapper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<int> ExecuteAsync(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                return await connection.ExecuteAsync(sql, param, transaction, commandTimeout, commandType);
            }
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                return await connection.QueryAsync<T>(sql, param, transaction, commandTimeout, commandType);
            }
        }

        public static T Get<T>(object parameters, string name)
        {
            return new DynamicParameters(parameters).Get<T>(name);
        }

        public static void Set(object parameters, string name, object value)
        {
            ((DynamicParameters)parameters).Add(name, value);
        }
    }
}
