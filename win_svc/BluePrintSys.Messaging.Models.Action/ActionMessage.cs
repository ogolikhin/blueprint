using NServiceBus;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;

namespace BluePrintSys.Messaging.Models.Actions
{
    public abstract class ActionMessage : IMessage, IWorkflowMessage
    {
        public abstract MessageActionType ActionType { get; }
    }
}
