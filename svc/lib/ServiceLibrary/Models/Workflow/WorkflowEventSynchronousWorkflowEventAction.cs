using ServiceLibrary.Models.PropertyType;

namespace ServiceLibrary.Models.Workflow
{
    public abstract class WorkflowEventSynchronousWorkflowEventAction : WorkflowEventAction, IWorkflowEventSynchronousAction
    {
        public override PropertySetResult ValidateAction(IExecutionParameters executionParameters)
        {
            return ValidateActionToBeProcessed(executionParameters);
        }

        protected abstract PropertySetResult ValidateActionToBeProcessed(IExecutionParameters executionParameters);

    }

}
