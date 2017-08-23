using NServiceBus;
using ServiceLibrary.Models.Enums;

namespace BluePrintSys.Messaging.Models.Actions
{
    public abstract class ActionMessage : IMessage
    {
        public abstract MessageActionType ActionType { get; }
    }
}
