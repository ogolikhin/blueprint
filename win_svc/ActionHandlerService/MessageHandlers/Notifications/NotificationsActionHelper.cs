using ActionHandlerService.Models;

namespace ActionHandlerService.MessageHandlers.Notifications
{
    public class NotificationsActionHelper : IActionHelper
    {
        public bool HandleAction(TenantInformation tenant)
        {
            return true;
        }
    }
}
