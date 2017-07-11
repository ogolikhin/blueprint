using NServiceBus;

namespace BluePrintSys.Messaging.Models.Actions
{
    [Express]
    public abstract class ActionMessage : IMessage
    {
        protected ActionMessage() : this(0, 0)
        {
            
        }

        protected ActionMessage(int tenantId, int workflowId)
        {
            TenantId = tenantId;
            WorkflowId = workflowId;
        }

        public abstract MessageActionType ActionType { get; }
        public int TenantId { get; set; }
        public int WorkflowId { get; set; }
    }
}
