using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BluePrintSys.Messaging.CrossCutting.Helpers;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.VersionControl;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Models.Workflow.Actions;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;

namespace BlueprintSys.RC.Services.Helpers
{
    public class WorkflowEventsMessagesHelper
    {
        private const string LogSource = "StateChange.WorkflowEventsMessagesHelper";

        public static async Task<IList<IWorkflowMessage>> GenerateMessages(int userId,
            int revisionId,
            string userName,
            WorkflowEventTriggers postOpTriggers,
            IBaseArtifactVersionControlInfo artifactInfo,
            string projectName,
            IDictionary<int, IList<Property>> modifiedProperties,
            bool sendArtifactPublishedMessage,
            string artifactUrl,
            string baseUrl,
            IUsersRepository repository,
            IServiceLogRepository serviceLogRepository)
        {
            var resultMessages = new List<IWorkflowMessage>();
            //var project = artifactResultSet?.Projects?.FirstOrDefault(d => d.Id == artifactInfo.ProjectId);
            var baseHostUri = baseUrl ?? ServerUriHelper.GetBaseHostUri()?.ToString();

            foreach (var workflowEventTrigger in postOpTriggers)
            {
                if (workflowEventTrigger?.Action == null)
                {
                    continue;
                }
                switch (workflowEventTrigger.ActionType)
                {
                    case MessageActionType.Notification:
                        var notificationAction = workflowEventTrigger.Action as EmailNotificationAction;
                        if (notificationAction == null)
                        {
                            continue;
                        }
                        var notificationMessage = await GetNotificationMessage(userId, 
                            revisionId, 
                            artifactInfo,
                            projectName,
                            notificationAction,
                            artifactUrl,
                            repository);
                        if (notificationMessage == null)
                        {
                            await serviceLogRepository.LogInformation(LogSource, $"Skipping Email notification action for artifact {artifactInfo.Id}");
                            Logger.Log($"Skipping Email notification action for artifact {artifactInfo.Id}", 
                                MessageActionType.Notification, 
                                null, 
                                LogLevel.Debug);
                            continue;
                        }
                        resultMessages.Add(notificationMessage);
                        break;
                    case MessageActionType.GenerateChildren:
                        var generateChildrenAction = workflowEventTrigger.Action as GenerateChildrenAction;
                        if (generateChildrenAction == null)
                        {
                            continue;
                        }
                        var generateChildrenMessage = new GenerateDescendantsMessage
                        {
                            ChildCount = generateChildrenAction.ChildCount.GetValueOrDefault(10),
                            DesiredArtifactTypeId = generateChildrenAction.ArtifactTypeId,
                            ArtifactId = artifactInfo.Id,
                            RevisionId = revisionId,
                            UserId = userId,
                            ProjectId = artifactInfo.ProjectId,
                            UserName = userName,
                            BaseHostUri = baseHostUri,
                            ProjectName = projectName,
                            TypePredefined = (int)artifactInfo.PredefinedType
                        };
                        resultMessages.Add(generateChildrenMessage);
                        break;
                    case MessageActionType.GenerateTests:
                        var generateTestsAction = workflowEventTrigger.Action as GenerateTestCasesAction;
                        if (generateTestsAction == null || artifactInfo.PredefinedType != ItemTypePredefined.Process)
                        {
                            await serviceLogRepository.LogInformation(LogSource, $"Skipping GenerateTestCasesAction for artifact {artifactInfo.Id} as it is not a process");
                            Logger.Log($"Skipping GenerateTestCasesAction for artifact {artifactInfo.Id} as it is not a process",
                                MessageActionType.Notification,
                                null,
                                LogLevel.Debug);
                            continue;
                        }
                        var generateTestsMessage = new GenerateTestsMessage
                        {
                            ArtifactId = artifactInfo.Id,
                            RevisionId = revisionId,
                            UserId = userId,
                            ProjectId = artifactInfo.ProjectId,
                            UserName = userName,
                            BaseHostUri = baseHostUri,
                            ProjectName = projectName
                        };
                        resultMessages.Add(generateTestsMessage);
                        break;
                    case MessageActionType.GenerateUserStories:
                        var generateUserStories = workflowEventTrigger.Action as GenerateUserStoriesAction;
                        if (generateUserStories == null || artifactInfo.PredefinedType != ItemTypePredefined.Process)
                        {
                            await serviceLogRepository.LogInformation(LogSource, $"Skipping GenerateUserStories for artifact {artifactInfo.Id} as it is not a process");
                            Logger.Log($"Skipping GenerateUserStories for artifact {artifactInfo.Id} as it is not a process",
                                MessageActionType.Notification,
                                null,
                                LogLevel.Debug);
                            continue;
                        }
                        var generateUserStoriesMessage = new GenerateUserStoriesMessage
                        {
                            ArtifactId = artifactInfo.Id,
                            RevisionId = revisionId,
                            UserId = userId,
                            ProjectId = artifactInfo.ProjectId,
                            UserName = userName,
                            BaseHostUri = baseHostUri,
                            ProjectName = projectName
                        };
                        resultMessages.Add(generateUserStoriesMessage);
                        break;
                }
            }

            //Add published artifact message
            if (sendArtifactPublishedMessage)
            {
                var publishedMessage =
                    GetPublishedMessage(userId, revisionId, artifactInfo, modifiedProperties) as
                        ArtifactsPublishedMessage;

                if (publishedMessage != null && publishedMessage.Artifacts.Any())
                {
                    resultMessages.Add(publishedMessage);
                }
            }
            return resultMessages;
        }

        public static async Task ProcessMessages(string logSource,
            IApplicationSettingsRepository applicationSettingsRepository,
            IServiceLogRepository serviceLogRepository,
            IList<IWorkflowMessage> messages,
            string exceptionMessagePrepender)
        {
            var tenantInfo = await applicationSettingsRepository.GetTenantInfo();
            if (string.IsNullOrWhiteSpace(tenantInfo?.TenantId))
            {
                throw new TenantInfoNotFoundException("No tenant information found. Please contact your administrator.");
            }

            await
                ProcessMessages(logSource, tenantInfo.TenantId, serviceLogRepository, messages,
                    exceptionMessagePrepender);
        }

        public static async Task ProcessMessages(string logSource,
            string tenantId,
            IServiceLogRepository serviceLogRepository,
            IList<IWorkflowMessage> messages,
            string exceptionMessagePrepender)
        {
            if (messages == null || messages.Count <= 0)
            {
                return;
            }
            foreach (var actionMessage in messages.Where(a => a != null))
            {
                try
                {
                    await WorkflowMessaging.Instance.SendMessageAsync(tenantId, actionMessage);
                    string message = $"Sent {actionMessage.ActionType} message: {actionMessage.ToJSON()} with tenant id: {tenantId} to the Message queue";
                    await
                        serviceLogRepository.LogInformation(logSource, message);
                }
                catch (Exception ex)
                {
                    string message =
                        $"Error while sending {actionMessage.ActionType} message with content {actionMessage.ToJSON()}. {exceptionMessagePrepender}. Exception: {ex.Message}. StackTrace: {ex.StackTrace ?? string.Empty}";
                    await
                        serviceLogRepository.LogError(logSource, message);
                    throw;
                }
            }
        }

        private static async Task<IWorkflowMessage> GetNotificationMessage(int userId,
            int revisionId,
            IBaseArtifactVersionControlInfo artifactInfo,
            string projectName,
            EmailNotificationAction notificationAction,
            string artifactUrl,
            IUsersRepository repository)
        {
            string messageHeader = I18NHelper.FormatInvariant("You are being notified because artifact with Id: {0} has been created.", artifactInfo.Id);
            var artifactPartUrl = artifactUrl ?? ServerUriHelper.GetArtifactUrl(artifactInfo.Id, true);
            if (artifactPartUrl == null)
            {
                return null;
            }
            
           var  emails = await GetEmailValues(revisionId, artifactInfo.Id, notificationAction, repository);

            var notificationMessage = new NotificationMessage
            {
                ArtifactName = artifactInfo.Name,
                ProjectName = projectName,
                Subject = I18NHelper.FormatInvariant("Artifact {0} has been created.",artifactInfo.Id),
                From = notificationAction.FromDisplayName,
                To = emails,
                MessageTemplate = notificationAction.Message,
                RevisionId = revisionId,
                UserId = userId,
                ArtifactTypeId = artifactInfo.ItemTypeId,
                ArtifactId = artifactInfo.Id,
                ArtifactUrl = artifactPartUrl,
                ArtifactTypePredefined = (int)artifactInfo.PredefinedType,
                ProjectId = artifactInfo.ProjectId,
                Header = messageHeader
            };
            return notificationMessage;
        }

        internal static async Task<List<string>>  GetEmailValues(int revisionId, int artifactId,
            EmailNotificationAction notificationAction, IUsersRepository repository)
        {
            var emails = new List<string>();
            if (notificationAction.PropertyTypeId.HasValue && notificationAction.PropertyTypeId.Value > 0)
            {
                var userInfos =
                    await repository.GetUserInfoForWorkflowArtifactForAssociatedUserProperty
                        (artifactId,
                            notificationAction.PropertyTypeId.Value,
                            revisionId);
                //Make sure that email is provided
                emails.AddRange(from userInfo in userInfos
                    where !string.IsNullOrWhiteSpace(userInfo?.Email)
                    select userInfo.Email);
            }
            else
            {
                //Take email from list of provided emails
                emails.AddRange(notificationAction.Emails ?? new List<string>());
            }
            return emails;
        }

        private static IWorkflowMessage GetPublishedMessage(int userId,
            int revisionId,
            IBaseArtifactVersionControlInfo artifactInfo,
            IDictionary<int, IList<Property>> modifiedProperties)
        {
            var message = new ArtifactsPublishedMessage
            {
                UserId = userId,
                RevisionId = revisionId
            };
            var artifacts = new List<PublishedArtifactInformation>();
            var artifact = new PublishedArtifactInformation
            {
                Id = artifactInfo.Id,
                Name = artifactInfo.Name,
                Predefined = (int)artifactInfo.PredefinedType,
                IsFirstTimePublished = false, //State change always occurs on published artifacts
                ProjectId = artifactInfo.ProjectId,
                Url = ServerUriHelper.GetArtifactUrl(artifactInfo.Id, true),
                BaseUrl = ServerUriHelper.GetBaseHostForStoryteller(),
                ModifiedProperties = new List<PublishedPropertyInformation>()
            };

            IList<Property> artifactModifiedProperties;
            if (modifiedProperties?.Count > 0 && modifiedProperties.TryGetValue(artifactInfo.Id, out artifactModifiedProperties) && artifactModifiedProperties?.Count > 0)
            {
                artifact.ModifiedProperties.AddRange(artifactModifiedProperties.Select(p => new PublishedPropertyInformation
                {
                    TypeId = p.PropertyTypeId,
                    PredefinedType = (int)p.Predefined
                }));
                //Only add artifact to list if there is a list of modified properties
                artifacts.Add(artifact);
            }

            message.Artifacts = artifacts;
            return message;
        }

    }
}
