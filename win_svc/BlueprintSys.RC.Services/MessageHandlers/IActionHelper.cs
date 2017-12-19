using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.Models.Actions;

namespace BlueprintSys.RC.Services.MessageHandlers
{
    public interface IActionHelper
    {
        Task<bool> HandleAction(TenantInformation tenant, ActionMessage actionMessage, IBaseRepository baseRepository);
    }
}
