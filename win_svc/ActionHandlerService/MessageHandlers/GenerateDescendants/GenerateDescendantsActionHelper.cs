using System.Threading.Tasks;
using ActionHandlerService.Models;
using ActionHandlerService.Repositories;
using BluePrintSys.Messaging.Models.Actions;

namespace ActionHandlerService.MessageHandlers.GenerateDescendants
{
    public class GenerateDescendantsActionHelper : IActionHelper
    {
        public async Task<bool> HandleAction(TenantInformation tenantInformation, ActionMessage actionMessage, IActionHandlerServiceRepository repository)
        {
            return await Task.FromResult(true);
        }
    }
}
