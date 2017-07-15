using ActionHandlerService.Models;
using BluePrintSys.Messaging.Models.Actions;

namespace ActionHandlerService.MessageHandlers.GenerateUserStories
{
    public class GenerateUserStoriesActionHelper : IActionHelper
    {
        public bool HandleAction(TenantInformation tenantInformation, ActionMessage actionMessage)
        {
            return true;
        }
    }
}
