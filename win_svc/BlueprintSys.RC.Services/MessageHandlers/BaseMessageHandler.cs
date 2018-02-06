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
        private ITransactionValidator TransactionValidator { get; }

        protected BaseMessageHandler(IActionHelper actionHelper, ITenantInfoRetriever tenantInfoRetriever, IConfigHelper configHelper, ITransactionValidator transactionValidator)
        {
            ActionHelper = actionHelper;
            TenantInfoRetriever = tenantInfoRetriever;
            ConfigHelper = configHelper;
            TransactionValidator = transactionValidator;
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

                var tenantConnectionString = tenant.BlueprintConnectionString;
                if (string.IsNullOrWhiteSpace(tenantConnectionString))
                {
                    throw new EntityNotFoundException($"Invalid Connection String provided for tenant {tenantId}.");
                }

                Logger.Log($"Creating repository with tenant connection string: {tenantConnectionString}", message, tenant);
                var repository = CreateRepository(actionType, tenantConnectionString);

                var transactionStatus = await TransactionValidator.GetStatus(message, tenant, repository);
                if (transactionStatus == TransactionStatus.RolledBack)
                {
                    Logger.Log("Discarding message for rolled back transaction", message, tenant);
                    return;
                }

                var messageId = GetMessageHeaderValue(Headers.MessageId, context);
                var timeSent = GetMessageHeaderValue(Headers.TimeSent, context);

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

        private string GetMessageHeaderValue(string header, IMessageHandlerContext context)
        {
            string headerValue;
            if (!context.MessageHeaders.TryGetValue(header, out headerValue))
            {
                throw new MessageHeaderValueNotFoundException($"Failed to find Message Header Value: {header}");
            }
            return headerValue;
        }

        private static IBaseRepository CreateRepository(MessageActionType actionType, string connectionString)
        {
            switch (actionType)
            {
                case MessageActionType.ArtifactsPublished:
                    return new ArtifactsPublishedRepository(connectionString);
                case MessageActionType.ArtifactsChanged:
                    return new ArtifactsChangedRepository(connectionString);
                case MessageActionType.GenerateChildren:
                case MessageActionType.GenerateTests:
                case MessageActionType.GenerateUserStories:
                    return new GenerateActionsRepository(connectionString);
                case MessageActionType.Notification:
                    return new NotificationRepository(connectionString);
                case MessageActionType.ProjectsChanged:
                    return new ProjectsChangedRepository(connectionString);
                case MessageActionType.PropertyItemTypesChanged:
                    return new PropertyItemTypesChangedRepository(connectionString);
                case MessageActionType.UsersGroupsChanged:
                    return new UsersGroupsChangedRepository(connectionString);
                case MessageActionType.WorkflowsChanged:
                    return new WorkflowsChangedRepository(connectionString);
                default:
                    throw new UnsupportedActionTypeException($"Failed to instantiate repository for unsupported Action Type: {actionType}");
            }
        }
    }
}
