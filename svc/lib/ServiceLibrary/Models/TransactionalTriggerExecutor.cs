using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceLibrary.Exceptions;

namespace ServiceLibrary.Models
{
    public abstract class TransactionalTriggerExecutor<T, TK>
    {
        private readonly IEnumerable<IConstraint> _preOps;
        private readonly IEnumerable<IAction> _postOps;
        private readonly T _input;

        protected TransactionalTriggerExecutor(IEnumerable<IConstraint> preOps, IEnumerable<IAction> postOps, T input)
        {
            _preOps = preOps ?? new List<IConstraint>();
            _postOps = postOps ?? new List<IAction>();
            _input = input;
        }

        public async Task<TK> Execute()
        {
            //Start a transaction. we need to import code from blueprint-current for creating transactions

            foreach (var constraint in _preOps)
            {
                if (!(await constraint.IsFulfilled()))
                {
                    throw new ConflictException("State cannot be modified as the constrating is not fulfilled");
                }
            }
            var result = await ExecuteInternal(_input);
            foreach (var triggerExecutor in _postOps)
            {
                if (!await triggerExecutor.Execute())
                {
                    throw new ConflictException("State cannot be modified as the trigger cannot be executed");
                }
            }
            return result;
        }

        protected abstract Task<TK> ExecuteInternal(T input);
    }
}
