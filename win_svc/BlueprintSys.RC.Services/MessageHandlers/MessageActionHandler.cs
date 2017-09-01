using System.Linq;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Models;
using BlueprintSys.RC.Services.Repositories;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Models;

namespace BlueprintSys.RC.Services.MessageHandlers
{
    public abstract class MessageActionHandler : IActionHelper
    {
        public async Task<bool> HandleAction(TenantInformation tenant, 
            ActionMessage actionMessage,
            IActionHandlerServiceRepository actionHandlerServiceRepository)
        {
            if (tenant == null || actionMessage == null)
            {
                return false;
            }
            PreActionValidation(tenant, actionMessage, actionHandlerServiceRepository);

            var task = await HandleActionInternal(tenant, 
                actionMessage, 
                actionHandlerServiceRepository);

            PostActionValidation(tenant, actionMessage, actionHandlerServiceRepository);
            return task;
        }

        protected virtual void PreActionValidation(TenantInformation tenant, ActionMessage actionMessage,
            IActionHandlerServiceRepository actionHandlerServiceRepository)
        {

        }

        protected virtual void PostActionValidation(TenantInformation tenant, ActionMessage actionMessage,
            IActionHandlerServiceRepository actionHandlerServiceRepository)
        {

        }

        protected abstract Task<bool> HandleActionInternal(TenantInformation tenant, 
            ActionMessage actionMessage,
            IActionHandlerServiceRepository actionHandlerServiceRepository);

        protected async Task<SqlUser> GetUserInfo(ActionMessage message, IActionHandlerServiceRepository actionHandlerServiceRepository)
        {
            return (await actionHandlerServiceRepository.UsersRepository.GetExistingUsersByIdsAsync(new[] { message.UserId })).FirstOrDefault(m => m.UserId == message.UserId);
        }
    }
}
