using NServiceBus;

namespace BluePrintSys.Messaging.Models.Actions
{
    [Express]
    public abstract class ActionMessage : IMessage
    {
        protected ActionMessage(MessageActionType actionType, int tenantId, int workflowId)
        {
            ActionType = actionType;
            TenantId = tenantId;
            WorkflowId = workflowId;
        }

        public MessageActionType ActionType { get; set; }
        public int TenantId { get; set; }
        public int WorkflowId { get; set; }
    }
}
