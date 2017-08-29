using System;
using System.Threading.Tasks;
using ActionHandlerService.Helpers;
using ActionHandlerService.Models;
using ActionHandlerService.Repositories;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.CrossCutting.Host;
using BluePrintSys.Messaging.CrossCutting.Logging;
using BluePrintSys.Messaging.CrossCutting.Models.Exceptions;
using BluePrintSys.Messaging.Models.Actions;
using NServiceBus;
using ServiceLibrary.Models.Enums;

namespace ActionHandlerService.MessageHandlers
{
    public abstract class BaseMessageHandler<T> : IHandleMessages<T> where T : ActionMessage
    {
        private IActionHelper ActionHelper { get; }
        private ITenantInfoRetriever TenantInfoRetriever { get; }
        private IConfigHelper ConfigHelper { get; }

        protected BaseMessageHandler(IActionHelper actionHelper, ITenantInfoRetriever tenantInfoRetriever, IConfigHelper configHelper)
        {
            ActionHelper = actionHelper;
            TenantInfoRetriever = tenantInfoRetriever;
            ConfigHelper = configHelper;
        }

        public async Task Handle(T message, IMessageHandlerContext context)
        {
            try
            {
                var actionType = message.ActionType.ToString();
                Log.Info($"Received Message: {actionType}");
                if ((ConfigHelper.SupportedActionTypes & message.ActionType) == message.ActionType)
                {
                    var tenantId = GetMessageHeaderValue(ActionMessageHeaders.TenantId, context);
                    var tenants = await TenantInfoRetriever.GetTenants();
                    TenantInformation tenant;
                    if (!tenants.TryGetValue(tenantId, out tenant))
                    {
                        Log.Error($"Tenant Info not found for Tenant ID {tenantId}. Message is not processed.");
                        return;
                    }
                    var messageId = GetMessageHeaderValue(Headers.MessageId, context);
                    var timeSent = GetMessageHeaderValue(Headers.TimeSent, context);
                    Log.Info($"Action handling started. Message: {actionType}. Tenant ID: {tenantId}. Message ID: {messageId}. Time Sent: {timeSent}");
                    await ProcessAction(tenant, message);
                    Log.Info($"Action handling completed. Message: {actionType}. Tenant ID: {tenantId}. Message ID: {messageId}. Time Sent: {timeSent}");
                }
                else
                {
                    throw new UnsupportedActionTypeException($"Unsupported Action Type: {actionType}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Message handling failed due to an exception: {ex.Message}", ex);
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

        protected virtual async Task<bool> ProcessAction(TenantInformation tenant, T message)
        {
            IActionHandlerServiceRepository serviceRepository;
            switch (message.ActionType)
            {
                case MessageActionType.Notification:
                    serviceRepository = new NotificationRepository(tenant.BlueprintConnectionString);
                    break;
                case MessageActionType.ArtifactsPublished:
                    serviceRepository = new ArtifactsPublishedRepository(tenant.BlueprintConnectionString);
                    break;
                default:
                    serviceRepository = new ActionHandlerServiceRepository(tenant.BlueprintConnectionString);
                    break;
            }
            return await ActionHelper.HandleAction(tenant, message, serviceRepository);
        }
    }
}
