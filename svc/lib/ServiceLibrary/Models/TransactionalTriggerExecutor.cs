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
            //Start a transaction. we need to import code from blueprint-current for creating transactions
            return await SqlHelper.RunInTransactionAsync<TK>(ServiceConstants.RaptorMain, GetAction());
        }

        protected abstract Func<IDbTransaction, Task<TK>> GetAction();

        protected abstract Task<TK> ExecuteInternal(T input);
    }
}
