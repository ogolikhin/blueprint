using NServiceBus;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;

namespace BluePrintSys.Messaging.Models.Actions
{
    public class StatusCheckMessage : IMessage
    {
        public MessageActionType ActionType { get; } = MessageActionType.StatusCheck;
    }

}
