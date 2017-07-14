using ActionHandlerService.Models;
using BluePrintSys.Messaging.Models.Actions;

namespace ActionHandlerService.MessageHandlers.Notifications
{
    public class NotificationsActionHelper : IActionHelper
    {
        public bool HandleAction(TenantInformation tenantInformation, ActionMessage actionMessage)
        {
            return true;
        }
    }
}
