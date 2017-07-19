using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading.Tasks;
using Dapper;

namespace ServiceLibrary.Repositories
{
    public interface ISqlConnectionWrapper
    {
        DbConnection CreateConnection();
        Task<int> ExecuteAsync(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);
        IEnumerable<T> Query<T>(string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null);
        Task<T> ExecuteScalarAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);
        T ExecuteScalar<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        Task<Tuple<IEnumerable<T1>, IEnumerable<T2>>> QueryMultipleAsync<T1, T2>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);
        Task<Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>>> QueryMultipleAsync<T1, T2, T3>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);
        Task<Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>>> QueryMultipleAsync<T1, T2, T3, T4>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);
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

        public IEnumerable<T> Query<T>(string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                return connection.Query<T>(sql, param, transaction, buffered, commandTimeout, commandType);
            }
        }

        public async Task<T> ExecuteScalarAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                return await connection.ExecuteScalarAsync<T>(sql, param, transaction, commandTimeout, commandType);
            }
        }

        public T ExecuteScalar<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                return connection.ExecuteScalar<T>(sql, param, transaction, commandTimeout, commandType);
            }
        }

        public async Task<Tuple<IEnumerable<T1>, IEnumerable<T2>>> QueryMultipleAsync<T1, T2>(string sql, object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                using (var reader = connection.QueryMultiple(sql, param, transaction, commandTimeout, commandType))
                {
                    var result1 = await reader.ReadAsync<T1>();
                    var result2 = await reader.ReadAsync<T2>();
                    return Tuple.Create(result1, result2);
                }
            }
        }

        public async Task<Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>>> QueryMultipleAsync<T1, T2, T3>(string sql, object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                using (var reader = connection.QueryMultiple(sql, param, transaction, commandTimeout, commandType))
                {
                    var result1 = await reader.ReadAsync<T1>();
                    var result2 = await reader.ReadAsync<T2>();
                    var result3 = await reader.ReadAsync<T3>();
                    return Tuple.Create(result1, result2, result3);
                }
            }
        }

        public async Task<Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>>> QueryMultipleAsync<T1, T2, T3, T4>(string sql, object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                using (var reader = connection.QueryMultiple(sql, param, transaction, commandTimeout, commandType))
                {
                    var result1 = await reader.ReadAsync<T1>();
                    var result2 = await reader.ReadAsync<T2>();
                    var result3 = await reader.ReadAsync<T3>();
                    var result4 = await reader.ReadAsync<T4>();
                    return Tuple.Create(result1, result2, result3, result4);
                }
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

        public static DataTable ToDataTable<T>(IEnumerable<T> values, string typeName = "Int32Collection", string columnName = "Int32Value")
            where T : struct
        {
            var table = new DataTable { Locale = CultureInfo.InvariantCulture };
            table.SetTypeName(typeName);
            table.Columns.Add(columnName, typeof(int));

            foreach (var value in values)
            {
                table.Rows.Add(value);
            }

            return table;
        }

        public static DataTable ToStringDataTable(IEnumerable<string> values, string typeName = "StringCollection", string columnName = "StringValue")
        {
            var table = new DataTable { Locale = CultureInfo.InvariantCulture };
            table.SetTypeName(typeName);
            table.Columns.Add(columnName, typeof(string));

            foreach (var value in values)
            {
                table.Rows.Add(value);
            }

            return table;
        }
    }
}
