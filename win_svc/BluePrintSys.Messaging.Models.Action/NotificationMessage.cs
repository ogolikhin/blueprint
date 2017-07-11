using NServiceBus;

namespace BluePrintSys.Messaging.Models.Actions
{
    [Express]
    public class NotificationMessage : ActionMessage
    {
        public NotificationMessage()
        {
            
        }

        public NotificationMessage(int tenantId, int workflowId) : base(tenantId, workflowId)
        {
        }

        public override MessageActionType ActionType { get; } = MessageActionType.Notification;
    }
}
