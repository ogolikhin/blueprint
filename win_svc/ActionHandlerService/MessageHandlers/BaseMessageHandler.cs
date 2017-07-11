using System;
using System.Threading.Tasks;
using BluePrintSys.Messaging.CrossCutting.Logging;
using BluePrintSys.Messaging.Models.Actions;
using NServiceBus;

namespace ActionHandlerService.MessageHandlers
{
    public abstract class BaseMessageHandler<T> : IHandleMessages<T> where T: ActionMessage
    {
        protected BaseMessageHandler(IActionHelper actionHelper)
        {
            ActionHelper = actionHelper;
        }

        protected abstract MessageActionType ActionType { get; }

        protected IActionHelper ActionHelper { get; }

        public async Task Handle(T message, IMessageHandlerContext context) 
        {
            try
            {
                Log.Info($"Received Action Message {message.ActionType.ToString()}");
                if ((ConfigHelper.AllowedActionTypes & message.ActionType) == message.ActionType)
                {
                    var tenantId = message.TenantId;
                    var tenants = TenantInfoRetriever.GetTenants();
                    TenantInfo tenant;
                    if (!tenants.TryGetValue(tenantId, out tenant))
                    {
                        throw new TenantInfoNotFoundException($"Tentant Info not found for Tenant ID {tenantId}");
                    }
                    await ProcessAction(tenant, message, context);
                }
                else
                {
                    throw new UnsupportedActionTypeException($"Unsupported Action Type: {message.ActionType.ToString()}");
                }
            }
            catch (Exception e)
            {
                Log.Error($"Action handling failed for {message.ActionType.ToString()} with an exception: {e.Message}", e);
                throw;
            }
        }

        protected virtual Task ProcessAction(TenantInfo tenant, T message, IMessageHandlerContext context)
        {
            Log.Info($"Action handling started for {message.ActionType.ToString()}");
            ActionHelper.HandleAction(tenant);
            Log.Info($"Action handling finished for {message.ActionType.ToString()}");
            return Task.CompletedTask;
        }
    }
}
