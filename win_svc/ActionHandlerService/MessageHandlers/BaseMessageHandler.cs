using System;
using System.Threading.Tasks;
using ActionHandlerService.Helpers;
using ActionHandlerService.Models;
using BluePrintSys.Messaging.CrossCutting.Logging;
using BluePrintSys.Messaging.Models.Actions;
using NServiceBus;

namespace ActionHandlerService.MessageHandlers
{
    public abstract class BaseMessageHandler<T> : IHandleMessages<T> where T : ActionMessage
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
                if ((ConfigHelper.SupportedActionTypes & message.ActionType) == message.ActionType)
                {
                    var tenantIdString = GetMessageHeaderValue(ActionMessageHeaders.TenantId, context);
                    int tenantId;
                    if (!int.TryParse(tenantIdString, out tenantId))
                    {
                        throw new InvalidTenantIdException($"Invalid Tenant ID: {tenantIdString}");
                    }
                    var tenants = TenantInfoRetriever.GetTenants();
                    TenantInformation tenant;
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

        private string GetMessageHeaderValue(string header, IMessageHandlerContext context)
        {
            string headerValue;
            if (!context.MessageHeaders.TryGetValue(header, out headerValue))
            {
                throw new MessageHeaderValueNotFoundException($"Message Header Value Not Found: {header}");
            }
            return headerValue;
        }

        protected virtual Task ProcessAction(TenantInformation tenant, T message, IMessageHandlerContext context)
        {
            Log.Info($"Action handling started for {message.ActionType.ToString()}");
            ActionHelper.HandleAction(tenant, message);
            Log.Info($"Action handling finished for {message.ActionType.ToString()}");
            return Task.Factory.StartNew(() => { });
        }
    }
}
