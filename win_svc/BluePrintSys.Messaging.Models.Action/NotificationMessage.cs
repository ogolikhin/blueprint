using NServiceBus;

namespace BluePrintSys.Messaging.Models.Actions
{
    [Express]
    public class NotificationMessage : ActionMessage
    {
        public NotificationMessage(int tenantId, int workflowId) : base(MessageActionType.Notification, tenantId, workflowId)
        {
        }
    }
}
