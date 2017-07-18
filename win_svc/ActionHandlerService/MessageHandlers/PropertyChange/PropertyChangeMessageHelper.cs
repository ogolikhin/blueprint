using System.Threading.Tasks;
using ActionHandlerService.Models;
using BluePrintSys.Messaging.Models.Actions;

namespace ActionHandlerService.MessageHandlers.PropertyChange
{
    public class PropertyChangeMessageHelper : IActionHelper
    {
        public async Task<bool> HandleAction(TenantInformation tenantInformation, ActionMessage actionMessage)
        {
            return await Task.FromResult(true);
        }
    }
}
