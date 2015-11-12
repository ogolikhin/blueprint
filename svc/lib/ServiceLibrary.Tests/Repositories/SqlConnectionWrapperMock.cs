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
            Expression<Func<object, bool>> match = p => param == null || param.All(kv => Equals(kv.Value, SqlConnectionWrapper.Get<object>(p, kv.Key)));
            var setup = Setup(c => c.ExecuteAsync(sql, It.Is(match), It.IsAny<IDbTransaction>(), It.IsAny<int?>(), CommandType.StoredProcedure))
                .Returns(Task.FromResult(result));
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

        public void SetupQueryAsync<T>(string sql, Dictionary<string, object> param, IEnumerable<T> result)
        {
            Expression<Func<object, bool>> match = p => param == null || param.All(kv => Equals(kv.Value, SqlConnectionWrapper.Get<object>(p, kv.Key)));
            Setup(c => c.QueryAsync<T>(sql, It.Is(match), It.IsAny<IDbTransaction>(), It.IsAny<int?>(), CommandType.StoredProcedure))
                .Returns(Task.FromResult(result))
                .Verifiable();
        }
    }
}
