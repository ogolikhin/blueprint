using ServiceLibrary.Models.Enums;

namespace ServiceLibrary.Models.Workflow
{
    public interface IWorkflowEventAction
    {
        bool ValidateAction(IExecutionParameters executionParameters);
        MessageActionType ActionType { get; }
    }
    public abstract class WorkflowEventAction: IWorkflowEventAction
    {
        public abstract MessageActionType ActionType { get; }

        public abstract bool ValidateAction(IExecutionParameters executionParameters);
    }
}
