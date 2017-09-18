using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLibrary.Models.Workflow
{
    public class WorkflowEventTriggers : List<WorkflowEventTrigger>
    {
        public async Task<IDictionary<string, string>> ProcessTriggers(IExecutionParameters executionParameters)
        {
            var errors = ValidateActions(executionParameters);
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

        protected Dictionary<string, string> ValidateActions(IExecutionParameters executionParameters)
        {
            var result = new Dictionary<string, string>();
            foreach (var triggerExecutor in this)
            {
                var validationResult = triggerExecutor.Action.ValidateAction(executionParameters);
                if (validationResult != null)
                {
                    var message = $"Property type id {validationResult.PropertyTypeId} had the following error: {validationResult.Message}";
                    var resultKey = triggerExecutor.Name + validationResult.PropertyTypeId;
                    if (!result.ContainsKey(resultKey))
                    {
                        result.Add(resultKey, message);
                    }
                }
            }
            return result;
        }
    }

}
