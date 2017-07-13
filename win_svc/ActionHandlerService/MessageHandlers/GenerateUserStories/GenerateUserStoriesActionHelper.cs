using ActionHandlerService.Models;

namespace ActionHandlerService.MessageHandlers.GenerateUserStories
{
    public class GenerateUserStoriesActionHelper : IActionHelper
    {
        public bool HandleAction(TenantInformation tenant)
        {
            return true;
        }
    }
}
