using System;
using System.Data;
using System.Threading.Tasks;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Models
{
    public abstract class TransactionalTriggerExecutor<T, TK>
    {
        protected ISqlHelper SqlHelper { get; }

        protected T Input { get; }

        protected TransactionalTriggerExecutor(ISqlHelper sqlHelper, T input)
        {
            SqlHelper = sqlHelper;
            Input = input;
        }

        public async Task<TK> Execute()
        {
            return await SqlHelper.RunInTransactionAsync<TK>(ServiceConstants.RaptorMain, GetTransactionAction());
        }

        protected abstract Func<IDbTransaction, Task<TK>> GetTransactionAction();

        protected abstract Task<TK> ExecuteInternal(T input, IDbTransaction transaction = null);
    }
}
