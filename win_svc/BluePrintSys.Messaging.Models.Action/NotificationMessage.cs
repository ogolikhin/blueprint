using NServiceBus;

namespace BluePrintSys.Messaging.Models.Action
{
    [Express]
    public class NotificationMessage : ActionMessage
    {
        public NotificationMessage(int tenantId, int workflowId) : base(MessageActionType.Notification, tenantId, workflowId)
        {
        }
    }
}
