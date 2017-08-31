namespace ServiceLibrary.Models.Workflow
{
    public interface IWorkflowEventSynchronousAction
    {
        bool ValidateAction(IExecutionParameters executionParameters);
    }
}
