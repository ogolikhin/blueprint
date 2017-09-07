using System;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BlueprintSys.RC.Services.Models;
using BlueprintSys.RC.Services.Repositories;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.CrossCutting.Host;
using BluePrintSys.Messaging.CrossCutting.Logging;
using BluePrintSys.Messaging.CrossCutting.Models.Exceptions;
using BluePrintSys.Messaging.Models.Actions;
using NServiceBus;
using ServiceLibrary.Models.Enums;

namespace BlueprintSys.RC.Services.MessageHandlers
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
                Log.Info($"Received Message: {message.ActionType}");
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
                    Log.Info($"Action handling started. Message: {message.ActionType}. Tenant ID: {tenantId}. Message ID: {messageId}. Time Sent: {timeSent}");
                    var result = await ProcessAction(tenant, message, context);
                    Log.Info($"Action handling completed with result={result}. Message: {message.ActionType}. Tenant ID: {tenantId}. Message ID: {messageId}. Time Sent: {timeSent}");
                }
                else
                {
                    throw new UnsupportedActionTypeException($"Unsupported Action Type: {message.ActionType}");
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

        protected virtual async Task<bool> ProcessAction(TenantInformation tenant, T message, IMessageHandlerContext context)
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
                case MessageActionType.GenerateChildren:
                case MessageActionType.GenerateTests:
                case MessageActionType.GenerateUserStories:
                    serviceRepository = new GenerateActionRepository(tenant.BlueprintConnectionString);
                    break;
                default:
                    serviceRepository = new ActionHandlerServiceRepository(tenant.BlueprintConnectionString);
                    break;
            }
            return await ActionHelper.HandleAction(tenant, message, serviceRepository);
        }
    }
}
