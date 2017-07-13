using ActionHandlerService.Models;

namespace ActionHandlerService.MessageHandlers
{
    public interface IActionHelper
    {
        bool HandleAction(TenantInformation tenant);
    }
}
