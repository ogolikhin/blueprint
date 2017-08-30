using System.Threading.Tasks;
using BlueprintSys.RC.Services.Models;
using BlueprintSys.RC.Services.Repositories;
using BluePrintSys.Messaging.Models.Actions;

namespace BlueprintSys.RC.Services.MessageHandlers.PropertyChange
{
    public class PropertyChangeActionHelper : IActionHelper
    {
        public async Task<bool> HandleAction(TenantInformation tenant, ActionMessage actionMessage, IActionHandlerServiceRepository actionHandlerServiceRepository)
        {
            return await Task.FromResult(true);
        }
    }
}
