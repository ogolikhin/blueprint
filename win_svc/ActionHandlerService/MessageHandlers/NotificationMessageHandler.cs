using BluePrintSys.ActionMessaging.Models;

namespace ActionHandlerService.MessageHandlers
{
    public class NotificationMessageHandler : BaseMessageHandler<NotificationMessage>
    {
        protected override MessageActionType ActionType { get; } = MessageActionType.Notification;

        protected override IActionHelper ActionHelper { get; } = new NotificationActionHelper();
    }
}
