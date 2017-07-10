using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Models
{
    public abstract class TransactionalTriggerExecutor<T, TK>
    {
        protected ISqlHelper SqlHelper { get; }

        protected IEnumerable<IConstraint> PreOps { get; }
        protected IEnumerable<IAction> PostOps { get; }
        protected T Input { get; }

        protected TransactionalTriggerExecutor(ISqlHelper sqlHelper, IEnumerable<IConstraint> preOps, IEnumerable<IAction> postOps, T input)
        {
            SqlHelper = sqlHelper;
            PreOps = preOps ?? new List<IConstraint>();
            PostOps = postOps ?? new List<IAction>();
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
