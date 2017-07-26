using System.Threading.Tasks;
using ActionHandlerService.Models;
using ActionHandlerService.Repositories;
using BluePrintSys.Messaging.Models.Actions;

namespace ActionHandlerService.MessageHandlers
{
    public interface IActionHelper
    {
        Task<bool> HandleAction(TenantInformation tenantInformation, ActionMessage actionMessage, IActionHandlerServiceRepository repository);
    }
}
