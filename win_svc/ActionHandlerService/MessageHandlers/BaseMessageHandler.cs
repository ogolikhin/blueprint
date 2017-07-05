using System;
using System.Threading.Tasks;
using BluePrintSys.ActionMessaging.Models;
using NServiceBus;

namespace ActionHandlerService.MessageHandlers
{
    public abstract class BaseMessageHandler<T> : IHandleMessages<T> where T: ActionMessage
    {
        protected abstract MessageActionType ActionType { get; }

        protected abstract IActionHelper ActionHelper { get; }

        public async Task Handle(T message, IMessageHandlerContext context) 
        {
            try
            {
                if (ConfigHelper.AllowedActionTypes.Contains(ActionType))
                {
                    var tenantId = message.TenantId;
                    var tenants = TenantInfoRetriever.GetTenants();
                    TenantInfo tenant;
                    if (!tenants.TryGetValue(tenantId, out tenant))
                    {
                        throw new Exception($"Tentant Info not found for Tenant ID {tenantId}");
                    }
                    await ProcessAction(tenant, message, context);
                    
                }
                else
                {
                    throw new Exception($"Unsupported Action Type: {message.ActionType.ToString()}");
                }
            }
            catch (Exception)
            {
                //todo log exception
                throw;
            }
            return;
        }

        protected virtual Task ProcessAction(TenantInfo tenant, T message, IMessageHandlerContext context)
        {
            ActionHelper.HandleAction(tenant);
            return Task.CompletedTask;
        }
    }
}
