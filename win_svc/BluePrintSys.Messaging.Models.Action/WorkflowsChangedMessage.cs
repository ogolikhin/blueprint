using ServiceLibrary.Models.Enums;

namespace BluePrintSys.Messaging.Models.Actions
{
    public class WorkflowsChangedMessage : ActionMessage
    {
        public int WorkflowId { get; set; }
        public override MessageActionType ActionType { get; } = MessageActionType.WorkflowsChanged;
    }
}
