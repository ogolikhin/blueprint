using ActionHandlerService.Models;
using BluePrintSys.Messaging.Models.Actions;

namespace ActionHandlerService.MessageHandlers.GenerateDescendants
{
    public class GenerateDescendantsActionHelper : IActionHelper
    {
        public bool HandleAction(TenantInformation tenantInformation, ActionMessage actionMessage)
        {
            return true;
        }
    }
}
