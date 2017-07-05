using NServiceBus;

namespace BluePrintSys.ActionMessaging.Models
{
    [Express]
    public class NotificationMessage : ActionMessage
    {
        public NotificationMessage(int tenantId, int workflowId) : base(MessageActionType.Notification, tenantId, workflowId)
        {
        }
    }
}
