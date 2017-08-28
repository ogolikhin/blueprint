using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLibrary.Models.Workflow
{
    public class WorkflowEventTriggers : List<WorkflowEventTrigger>
    {
        public async Task<IDictionary<string, string>> ProcessTriggers(IExecutionParameters executionParameters)
        {
            var errors = await Execute(executionParameters);
            if (errors.Keys.Any())
            {
                return errors;
            }
            await InternalBatchExecute(executionParameters);
            return new Dictionary<string, string>();
        }

        protected virtual Task InternalBatchExecute(IExecutionParameters executionParameters)
        {
            return Task.Run(() => { });
        }

        protected async Task<Dictionary<string, string>> Execute(IExecutionParameters executionParameters)
        {
            var result = new Dictionary<string, string>();
            foreach (var triggerExecutor in this)
            {
                if (!await triggerExecutor.Action.Execute(executionParameters))
                {
                    result.Add(triggerExecutor.Name, "State cannot be modified as the trigger cannot be executed");
                }
            }
            return result;
        }
    }

}
