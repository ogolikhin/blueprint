using NServiceBus;

namespace BluePrintSys.Messaging.Models.Actions
{
    [Express]
    public abstract class ActionMessage : IMessage
    {
        public abstract MessageActionType ActionType { get; }
    }
}
