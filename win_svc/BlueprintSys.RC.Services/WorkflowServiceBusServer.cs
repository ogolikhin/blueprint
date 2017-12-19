using System;
using System.Collections.Generic;
using BlueprintSys.RC.Services.MessageHandlers.ArtifactsChanged;
using BlueprintSys.RC.Services.MessageHandlers.ArtifactsPublished;
using BlueprintSys.RC.Services.MessageHandlers.GenerateDescendants;
using BlueprintSys.RC.Services.MessageHandlers.GenerateTests;
using BlueprintSys.RC.Services.MessageHandlers.GenerateUserStories;
using BlueprintSys.RC.Services.MessageHandlers.Notifications;
using BlueprintSys.RC.Services.MessageHandlers.ProjectsChanged;
using BlueprintSys.RC.Services.MessageHandlers.PropertyItemTypesChanged;
using BlueprintSys.RC.Services.MessageHandlers.UsersGroupsChanged;
using BlueprintSys.RC.Services.MessageHandlers.WorkflowsChanged;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.CrossCutting.Host;
using BluePrintSys.Messaging.CrossCutting.Logging;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;

namespace BlueprintSys.RC.Services
{
    public class WorkflowServiceBusServer : NServiceBusServer<WorkflowServiceBusServer>, INServiceBusServer
    {
        private readonly Dictionary<MessageActionType, Type> _messageActionToHandlerMapping = new Dictionary<MessageActionType, Type>
        {
            { MessageActionType.ArtifactsPublished, typeof(ArtifactsPublishedMessageHandler) },
            { MessageActionType.ArtifactsChanged, typeof(ArtifactsChangedMessageHandler) },
            { MessageActionType.GenerateChildren, typeof(GenerateDescendantsMessageHandler) },
            { MessageActionType.GenerateTests, typeof(GenerateTestsMessageHandler) },
            { MessageActionType.GenerateUserStories, typeof(GenerateUserStoriesMessageHandler) },
            { MessageActionType.Notification, typeof(NotificationMessageHandler) },
            { MessageActionType.ProjectsChanged, typeof(ProjectsChangedMessageHandler) },
            { MessageActionType.PropertyItemTypesChanged, typeof(PropertyItemTypesChangedMessageHandler) },
            { MessageActionType.UsersGroupsChanged, typeof(UsersGroupsChangedMessageHandler) },
            { MessageActionType.WorkflowsChanged, typeof(WorkflowsChangedMessageHandler) }
        };

        protected override Dictionary<MessageActionType, Type> GetMessageActionToHandlerMapping()
        {
            return _messageActionToHandlerMapping;
        }

        public WorkflowServiceBusServer()
        {
            ConfigHelper = new ConfigHelper();
            MessageQueue = ConfigHelper.MessageQueue;
        }

        protected override void LogInfo(string tenantId, IWorkflowMessage message, Exception exception)
        {
            var actionMessage = message as ActionMessage;
            if (actionMessage == null)
            {
                base.LogInfo(tenantId, message, exception);
                return;
            }
            if (exception == null)
            {
                Log.Info($"Sending {actionMessage.ActionType} message for tenant {tenantId}");
            }
            else
            {
                Log.Error($"Failed to send {actionMessage.ActionType.ToString()} message for tenant {tenantId} due to an exception: {exception.Message}", exception);
            }
        }
    }
}
