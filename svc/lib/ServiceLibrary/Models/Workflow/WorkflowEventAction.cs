using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.PropertyType;

namespace ServiceLibrary.Models.Workflow
{
    public interface IWorkflowEventAction
    {
        PropertySetResult ValidateAction(IExecutionParameters executionParameters);
        MessageActionType ActionType { get; }
    }
    public abstract class WorkflowEventAction : IWorkflowEventAction
    {
        public abstract MessageActionType ActionType { get; }

        public abstract PropertySetResult ValidateAction(IExecutionParameters executionParameters);
    }
}
