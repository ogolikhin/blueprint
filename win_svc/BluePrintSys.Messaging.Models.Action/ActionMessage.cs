using NServiceBus;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;

namespace BluePrintSys.Messaging.Models.Actions
{
    public abstract class ActionMessage : IMessage, IWorkflowMessage
    {
        public abstract MessageActionType ActionType { get; }
        public long TransactionId { get; set; }
        public int UserId { get; set; }
        public int RevisionId { get; set; }
        public string NSBRetryCount { get; set; }
    }

    public abstract class ProjectContainerActionMessage : ActionMessage
    {
        public int ProjectId { get; set; }
    }
}
