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
using BlueprintSys.RC.Services.MessageHandlers.Webhooks;
using BlueprintSys.RC.Services.MessageHandlers.WorkflowsChanged;
using BluePrintSys.Messaging.CrossCutting.Host;
using ServiceLibrary.Models.Enums;

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
            { MessageActionType.WorkflowsChanged, typeof(WorkflowsChangedMessageHandler) },
            { MessageActionType.Webhooks, typeof(WebhooksHandler) }
        };

        protected override Dictionary<MessageActionType, Type> GetMessageActionToHandlerMapping()
        {
            return _messageActionToHandlerMapping;
        }
    }
}
