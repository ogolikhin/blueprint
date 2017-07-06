using BluePrintSys.Messaging.Models.Action;

namespace ActionHandlerService.MessageHandlers
{
    public class NotificationMessageHandler : BaseMessageHandler<NotificationMessage>
    {
        public NotificationMessageHandler() : this(new NotificationActionHelper())
        {
        }

        public NotificationMessageHandler(IActionHelper actionHelper):base(actionHelper)
        {
        }

        protected override MessageActionType ActionType { get; } = MessageActionType.Notification;
    }
}
