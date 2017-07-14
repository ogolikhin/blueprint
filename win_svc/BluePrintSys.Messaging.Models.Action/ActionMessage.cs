using NServiceBus;

namespace BluePrintSys.Messaging.Models.Actions
{
    public class ActionMessageHeaders
    {
        public const string TenantId = "TenantId";
    }

    [Express]
    public abstract class ActionMessage : IMessage
    {
        public abstract MessageActionType ActionType { get; }
    }
}
