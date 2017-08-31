using ServiceLibrary.Models.Enums;

namespace ServiceLibrary.Models.Workflow
{
    public abstract class WorkflowEventAction
    {
        public abstract MessageActionType ActionType { get; }

        public abstract bool ValidateAction(IExecutionParameters executionParameters);
    }
}
