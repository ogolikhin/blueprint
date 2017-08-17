using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Models.Workflow.Actions;

namespace ArtifactStore.Models.Workflow
{
    public class PreopWorkflowEventTriggers : WorkflowEventTriggers
    {
        protected override async Task InternalBatchExecute()
        {
            await SavePropertyChangeActions();
        }

        private async Task SavePropertyChangeActions()
        {
            var propertyChangeActions = this.Select(t => t.Action).OfType<PropertyChangeAction>();
            
            await ExecutionParameters.SaveRepository.SavePropertyChangeActions(
                ExecutionParameters.UserId,
                propertyChangeActions,
                ExecutionParameters.CustomPropertyTypes,
                ExecutionParameters.ArtifactInfo,
                ExecutionParameters.Transaction);
        }
    }

    public class PostopWorkflowEventTriggers : WorkflowEventTriggers
    {
        protected override Task InternalBatchExecute()
        {
            return Task.Run(() => { });
        }
    }
    
    public class WorkflowEventTriggers : List<WorkflowEventTrigger>
    {
        protected ExecutionParameters ExecutionParameters;
        public async Task<IDictionary<string, string>> ProcessTriggers(ExecutionParameters executionParameters)
        {
            ExecutionParameters = executionParameters;
            var errors = await Execute();
            if (errors.Keys.Any())
            {
                return errors;
            }
            await InternalBatchExecute();
            return new Dictionary<string, string>();
        }

        protected virtual Task InternalBatchExecute()
        {
            return Task.Run(() => { });
        }

        protected async Task<Dictionary<string, string>> Execute()
        {
            var result = new Dictionary<string, string>();
            foreach (var triggerExecutor in this)
            {
                if (!await triggerExecutor.Action.Execute(ExecutionParameters))
                {
                    result.Add(triggerExecutor.Name, "State cannot be modified as the trigger cannot be executed");
                }
            }
            return result;
        }
    }

    
}
