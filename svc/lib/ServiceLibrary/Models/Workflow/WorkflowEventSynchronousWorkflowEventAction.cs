using System.Threading.Tasks;

namespace ServiceLibrary.Models.Workflow
{
    public abstract class WorkflowEventSynchronousWorkflowEventAction : WorkflowEventAction, IWorkflowEventSynchronousAction
    {
        public override async Task<bool> Execute(IExecutionParameters executionParameters)
        {
            var result = ValidateActionToBeProcessed(executionParameters);
            return await Task.FromResult(result);
        }

        protected abstract bool ValidateActionToBeProcessed(IExecutionParameters executionParameters);

    }

}
