using ActionHandlerService.Models;

namespace ActionHandlerService.MessageHandlers.GenerateDescendants
{
    public class GenerateDescendantsActionHelper : IActionHelper
    {
        public bool HandleAction(TenantInformation tenant)
        {
            return true;
        }
    }
}
