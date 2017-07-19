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
        protected ActionMessage() : this(0)
        {
        }

        protected ActionMessage(int tenantId)
        {
            TenantId = tenantId;
        }

        public abstract MessageActionType ActionType { get; }
        public int TenantId { get; set; }
    }
}
