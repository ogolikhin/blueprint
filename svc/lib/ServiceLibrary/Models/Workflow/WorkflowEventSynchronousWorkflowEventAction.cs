namespace ServiceLibrary.Models.Workflow
{
    public abstract class WorkflowEventSynchronousWorkflowEventAction : WorkflowEventAction, IWorkflowEventSynchronousAction
    {
        public override bool ValidateAction(IExecutionParameters executionParameters)
        {
            return ValidateActionToBeProcessed(executionParameters);
        }

        protected abstract bool ValidateActionToBeProcessed(IExecutionParameters executionParameters);

    }

}
