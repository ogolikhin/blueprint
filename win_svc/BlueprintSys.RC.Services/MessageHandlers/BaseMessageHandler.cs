using System;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BlueprintSys.RC.Services.MessageHandlers.ArtifactsChanged;
using BlueprintSys.RC.Services.MessageHandlers.ArtifactsPublished;
using BlueprintSys.RC.Services.MessageHandlers.Notifications;
using BlueprintSys.RC.Services.MessageHandlers.ProjectsChanged;
using BlueprintSys.RC.Services.MessageHandlers.PropertyItemTypesChanged;
using BlueprintSys.RC.Services.MessageHandlers.UsersGroupsChanged;
using BlueprintSys.RC.Services.MessageHandlers.WorkflowsChanged;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.CrossCutting.Host;
using BluePrintSys.Messaging.CrossCutting.Logging;
using BluePrintSys.Messaging.CrossCutting.Models.Exceptions;
using BluePrintSys.Messaging.Models.Actions;
using NServiceBus;
using ServiceLibrary.Helpers;
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

        private string GetMessageHeaderValue(string header, IMessageHandlerContext context)
        {
            string headerValue;
            if (!context.MessageHeaders.TryGetValue(header, out headerValue))
            {
                throw new MessageHeaderValueNotFoundException($"Failed to find Message Header Value: {header}");
            }
            return headerValue;
        }

        public async Task Handle(T message, IMessageHandlerContext context)
        {
            try
            {
                var actionType = message.ActionType;
                Log.Info($"Received {actionType} message: {message.ToJSON()}");

                if ((ConfigHelper.SupportedActionTypes & actionType) != actionType)
                {
                    throw new UnsupportedActionTypeException($"Unsupported Action Type: {actionType}");
                }

                var tenantId = GetMessageHeaderValue(ActionMessageHeaders.TenantId, context);
                var tenants = await TenantInfoRetriever.GetTenants();
                TenantInformation tenant;
                if (!tenants.TryGetValue(tenantId, out tenant))
                {
                    throw new EntityNotFoundException($"Failed to find Tenant Info for Tenant ID {tenantId}.");
                }

                var messageId = GetMessageHeaderValue(Headers.MessageId, context);
                var timeSent = GetMessageHeaderValue(Headers.TimeSent, context);

                IBaseRepository repository;
                switch (actionType)
                {
                    case MessageActionType.ArtifactsPublished:
                        repository = new ArtifactsPublishedRepository(tenant.BlueprintConnectionString);
                        break;
                    case MessageActionType.ArtifactsChanged:
                        repository = new ArtifactsChangedRepository(tenant.BlueprintConnectionString);
                        break;
                    case MessageActionType.GenerateChildren:
                    case MessageActionType.GenerateTests:
                    case MessageActionType.GenerateUserStories:
                        repository = new GenerateActionsRepository(tenant.BlueprintConnectionString);
                        break;
                    case MessageActionType.Notification:
                        repository = new NotificationRepository(tenant.BlueprintConnectionString);
                        break;
                    case MessageActionType.ProjectsChanged:
                        repository = new ProjectsChangedRepository(tenant.BlueprintConnectionString);
                        break;
                    case MessageActionType.PropertyItemTypesChanged:
                        repository = new PropertyItemTypesChangedRepository(tenant.BlueprintConnectionString);
                        break;
                    case MessageActionType.UsersGroupsChanged:
                        repository = new UsersGroupsChangedRepository(tenant.BlueprintConnectionString);
                        break;
                    case MessageActionType.WorkflowsChanged:
                        repository = new WorkflowsChangedRepository(tenant.BlueprintConnectionString);
                        break;
                    default:
                        throw new UnsupportedActionTypeException($"Failed to instantiate repository for unsupported Action Type: {actionType}");
                }

                Logger.Log($"Started handling {actionType} action. Message ID: {messageId}. Time Sent: {timeSent}", message, tenant);
                var result = await ActionHelper.HandleAction(tenant, message, repository);
                Logger.Log($"Finished handling {actionType} action. Result: {result}. Message ID: {messageId}. Time Sent: {timeSent}", message, tenant);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to handle {message.ActionType} Message {message.ToJSON()} due to an exception: {ex.Message}", ex);
                throw;
            }
        }
    }
}
