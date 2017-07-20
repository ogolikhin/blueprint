using BluePrintSys.Messaging.Models.Actions;

namespace ActionHandlerService.MessageHandlers.Notifications
{
    public class NotificationMessageHandler : BaseMessageHandler<NotificationMessage>
    {
        public NotificationMessageHandler() : this(new NotificationsActionHelper())
        {
        }

        public NotificationMessageHandler(IActionHelper actionHelper) : base(actionHelper)
        {
        }
    }
}
