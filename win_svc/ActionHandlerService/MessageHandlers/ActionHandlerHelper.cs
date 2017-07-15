using ActionHandlerService.Models;
using BluePrintSys.Messaging.Models.Actions;

namespace ActionHandlerService.MessageHandlers
{
    public interface IActionHelper
    {
        bool HandleAction(TenantInformation tenantInformation, ActionMessage actionMessage);
    }
}
