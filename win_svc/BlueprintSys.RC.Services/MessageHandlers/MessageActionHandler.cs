using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.Models.Actions;

namespace BlueprintSys.RC.Services.MessageHandlers
{
    public abstract class MessageActionHandler : IActionHelper
    {
        public async Task<bool> HandleAction(TenantInformation tenant, ActionMessage actionMessage, IBaseRepository baseRepository)
        {
            if (tenant == null)
            {
                Logger.Log("Action handling failed because the tenant is null", actionMessage, tenant, LogLevel.Error);
                return false;
            }

            if (actionMessage == null)
            {
                Logger.Log("Action handling failed because the message is null", actionMessage, tenant, LogLevel.Error);
                return false;
            }

            Logger.Log("Performing Pre Action Validation", actionMessage, tenant);
            var preActionValidationResult = await PreActionValidation(tenant, actionMessage, baseRepository);
            if (!preActionValidationResult)
            {
                Logger.Log("Action handling failed because Pre Action Validation failed", actionMessage, tenant, LogLevel.Error);
                return false;
            }

            var task = await HandleActionInternal(tenant, actionMessage, baseRepository);

            PostActionValidation(tenant, actionMessage, baseRepository);
            return task;
        }

        protected virtual async Task<bool> PreActionValidation(TenantInformation tenant, ActionMessage actionMessage, IBaseRepository baseRepository)
        {
            return await Task.FromResult(true);
        }

        protected virtual void PostActionValidation(TenantInformation tenant, ActionMessage actionMessage, IBaseRepository baseRepository)
        {

        }

        protected abstract Task<bool> HandleActionInternal(TenantInformation tenant, ActionMessage actionMessage, IBaseRepository baseRepository);
    }
}