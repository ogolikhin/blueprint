using System;
using System.Threading.Tasks;
using ActionHandlerService.Helpers;
using ActionHandlerService.Models;
using ActionHandlerService.Models.Exceptions;
using ActionHandlerService.Repositories;
using BluePrintSys.Messaging.CrossCutting.Logging;
using BluePrintSys.Messaging.Models.Actions;
using NServiceBus;

namespace ActionHandlerService.MessageHandlers
{
    public abstract class BaseMessageHandler<T> : IHandleMessages<T> where T : ActionMessage
    {
        private IActionHelper ActionHelper { get; }
        private ITenantInfoRetriever TenantInfoRetriever { get; }
        private IConfigHelper ConfigHelper { get; }

        protected BaseMessageHandler(IActionHelper actionHelper, ITenantInfoRetriever tenantInfoRetriever = null, IConfigHelper configHelper = null)
        {
            ActionHelper = actionHelper;
            TenantInfoRetriever = tenantInfoRetriever ?? new TenantInfoRetriever();
            ConfigHelper = configHelper ?? new ConfigHelper();
        }

        public async Task Handle(T message, IMessageHandlerContext context)
        {
            try
            {
                Log.Info($"Received Action Message {message.ActionType.ToString()}");
                if ((ConfigHelper.SupportedActionTypes & message.ActionType) == message.ActionType)
                {
                    var tentantId = GetMessageHeaderValue(ActionMessageHeaders.TenantId, context);
                    var tenants = TenantInfoRetriever.GetTenants();
                    TenantInformation tenant;
                    if (!tenants.TryGetValue(tentantId, out tenant))
                    {
                        throw new TenantInfoNotFoundException($"Tentant Info not found for Tenant ID {tentantId}");
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

        private string GetMessageHeaderValue(string header, IMessageHandlerContext context)
        {
            string headerValue;
            if (!context.MessageHeaders.TryGetValue(header, out headerValue))
            {
                throw new MessageHeaderValueNotFoundException($"Message Header Value Not Found: {header}");
            }
            return headerValue;
        }

        protected virtual async Task<bool> ProcessAction(TenantInformation tenant, T message, IMessageHandlerContext context)
        {
            Log.Info($"Action handling started for {message.ActionType.ToString()}");
            var repository = new ActionHandlerServiceRepository(tenant.ConnectionString);
            return await ActionHelper.HandleAction(tenant, message, repository);
        }
    }
}
