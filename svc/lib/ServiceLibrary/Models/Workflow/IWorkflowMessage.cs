using ServiceLibrary.Models.Enums;

namespace ServiceLibrary.Models.Workflow
{
    // marker interface
    public interface IWorkflowMessage
    {
        MessageActionType ActionType { get; }
    }
}
