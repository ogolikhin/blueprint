using ServiceLibrary.Models.PropertyType;

namespace ServiceLibrary.Models.Workflow
{
    public interface IWorkflowEventSynchronousAction
    {
        PropertySetResult ValidateAction(IExecutionParameters executionParameters);
    }
}
