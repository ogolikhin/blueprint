using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Moq;

namespace ServiceLibrary.Repositories
{
    public class SqlConnectionWrapperMock : Mock<ISqlConnectionWrapper>
    {
        public SqlConnectionWrapperMock()
        {
        }

        public SqlConnectionWrapperMock(MockBehavior behavior)
            : base(behavior)
        {
        }

        public void SetupExecuteAsync(string sql, Dictionary<string, object> param, int result, Dictionary<string, object> outParameters = null)
        {
            Expression<Func<object, bool>> match = p => param == null || param.All(kv => Matches(kv.Value, SqlConnectionWrapper.Get<object>(p, kv.Key)));
            Setup(c => c.ExecuteAsync(sql, It.Is(match), It.IsAny<IDbTransaction>(), It.IsAny<int?>(), CommandType.StoredProcedure), result, outParameters);
        }

        public void SetupExecuteScalarAsync<T>(string sql, Dictionary<string, object> param, T result, Dictionary<string, object> outParameters = null)
        {
            Expression<Func<object, bool>> match = p => param == null || param.All(kv => Matches(kv.Value, SqlConnectionWrapper.Get<object>(p, kv.Key)));
            Setup(c => c.ExecuteScalarAsync<T>(sql, It.Is(match), It.IsAny<IDbTransaction>(), It.IsAny<int?>(), CommandType.StoredProcedure), result, outParameters);
        }

        public void SetupExecuteScalarAsync<T>(Expression<Func<string, bool>> sqlMatcher, Dictionary<string, object> param, T result, Dictionary<string, object> outParameters = null)
        {
            Expression<Func<object, bool>> match = p => param == null || param.All(kv => Matches(kv.Value, SqlConnectionWrapper.Get<object>(p, kv.Key)));
            Setup(c => c.ExecuteScalarAsync<T>(It.Is(sqlMatcher), It.Is(match), It.IsAny<IDbTransaction>(), It.IsAny<int?>(), It.IsAny<CommandType?>()), result, outParameters);
        }

        public void SetupQueryAsync<T>(string sql, Dictionary<string, object> param, IEnumerable<T> result, Dictionary<string, object> outParameters = null)
        {
            Expression<Func<object, bool>> match = p => param == null || param.All(kv => Matches(kv.Value, SqlConnectionWrapper.Get<object>(p, kv.Key)));
            Setup(c => c.QueryAsync<T>(sql, It.Is(match), It.IsAny<IDbTransaction>(), It.IsAny<int?>(), CommandType.StoredProcedure), result, outParameters);
        }

        public void SetupQueryMultipleAsync<T1, T2>(string sql, Dictionary<string, object> param, Tuple<IEnumerable<T1>, IEnumerable<T2>> result, Dictionary<string, object> outParameters = null)
        {
            Expression<Func<object, bool>> match = p => param == null || param.All(kv => Matches(kv.Value, SqlConnectionWrapper.Get<object>(p, kv.Key)));
            Setup(c => c.QueryMultipleAsync<T1, T2>(sql, It.Is(match), It.IsAny<IDbTransaction>(), It.IsAny<int?>(), CommandType.StoredProcedure), result, outParameters);
        }

        public void SetupQueryMultipleAsync<T1, T2, T3>(string sql, Dictionary<string, object> param, Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>> result, Dictionary<string, object> outParameters = null)
        {
            Expression<Func<object, bool>> match = p => param == null || param.All(kv => Matches(kv.Value, SqlConnectionWrapper.Get<object>(p, kv.Key)));
            Setup(c => c.QueryMultipleAsync<T1, T2, T3>(sql, It.Is(match), It.IsAny<IDbTransaction>(), It.IsAny<int?>(), CommandType.StoredProcedure), result, outParameters);
        }

        public void SetupQueryMultipleAsync<T1, T2, T3, T4>(string sql, Dictionary<string, object> param, Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>, IEnumerable<T4>> result, Dictionary<string, object> outParameters = null)
        {
            Expression<Func<object, bool>> match = p => param == null || param.All(kv => Matches(kv.Value, SqlConnectionWrapper.Get<object>(p, kv.Key)));
            Setup(c => c.QueryMultipleAsync<T1, T2, T3, T4>(sql, It.Is(match), It.IsAny<IDbTransaction>(), It.IsAny<int?>(), CommandType.StoredProcedure), result, outParameters);
        }

        private void Setup<T>(Expression<Func<ISqlConnectionWrapper, Task<T>>> expression, T result, Dictionary<string, object> outParameters = null)
        {
            var setup = Setup(expression).ReturnsAsync(result);
            if (outParameters != null)
            {
                setup.Callback((string s, object p, IDbTransaction t, int? o, CommandType c) =>
                {
                    foreach (var kv in outParameters)
                    {
                        SqlConnectionWrapper.Set(p, kv.Key, kv.Value);
                    }
                });
            }
            setup.Verifiable();
        }

        private static bool Matches(object objA, object objB)
        {
            var tableA = objA as DataTable;
            if (tableA != null)
            {
                var tableB = objB as DataTable;
                if (tableB != null)
                {
                    int columnsCount = tableA.Columns.Count;
                    int rowsCount = tableA.Rows.Count;
                    return tableB.Columns.Count == columnsCount &&
                        tableB.Rows.Count == rowsCount &&
                        Enumerable.Range(0, rowsCount).All(i => Enumerable.Range(0, columnsCount)
                            .All(j => Equals(tableA.Rows[i][j], tableB.Rows[i][j])));
                }
                return false;
            }
            return Equals(objA, objB);
        }
    }
}
