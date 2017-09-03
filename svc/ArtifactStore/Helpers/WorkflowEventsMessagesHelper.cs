using System;
using System.Collections.Generic;
using System.Data;
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

namespace ArtifactStore.Helpers
{
    public class WorkflowEventsMessagesHelper
    {
        private const string LogSource = "ArtifactStore.Helpers.WorkflowEventsMessagesHelper";

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
            IServiceLogRepository serviceLogRepository,
            IDbTransaction transaction = null)
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
                            repository,
                            transaction);
                        if (notificationMessage == null)
                        {
                            await serviceLogRepository.LogInformation(LogSource, $"Skipping Email notification action for artifact {artifactInfo.Id}");
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
            string exceptionMessagePrepender,
            IDbTransaction transaction = null)
        {
            var tenantInfo = await applicationSettingsRepository.GetTenantInfo(transaction);
            if (string.IsNullOrWhiteSpace(tenantInfo?.TenantId))
            {
                throw new TenantInfoNotFoundException("No tenant information found. Please contact your administrator.");
            }

            if (messages == null || messages.Count <= 0)
            {
                return;
            }
            foreach (var actionMessage in messages.Where(a => a != null))
            {
                try
                {
                    await WorkflowMessaging.Instance.SendMessageAsync(tenantInfo.TenantId, actionMessage);
                    string message = $"Sent {actionMessage.ActionType} message: {actionMessage.ToJSON()} with tenant id: {tenantInfo.TenantId} to the Message queue";
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
            IUsersRepository repository,
            IDbTransaction transaction)
        {
            string messageHeader = I18NHelper.FormatInvariant("You are being notified because of an update to the artifact with Id: {0}.", artifactInfo.Id);
            var artifactPartUrl = artifactUrl ?? ServerUriHelper.GetArtifactUrl(artifactInfo.Id, true);
            if (artifactPartUrl == null)
            {
                return null;
            }
            var emails = new List<string>();
            if (notificationAction.PropertyTypeId.HasValue && notificationAction.PropertyTypeId.Value > 0)
            {
                var userInfos =
                    await repository.GetUserInfoForWorkflowArtifactForAssociatedUserProperty
                        (artifactInfo.Id,
                            notificationAction.PropertyTypeId.Value,
                            revisionId,
                            transaction);
                //Make sure that email is provided
                emails.AddRange(from userInfo in userInfos where !string.IsNullOrWhiteSpace(userInfo?.Email) select userInfo.Email);
            }
            else
            {
                //Take email from list of provided emails
                emails.AddRange(notificationAction.Emails ?? new List<string>());
            }

            var notificationMessage = new NotificationMessage
            {
                ArtifactName = artifactInfo.Name,
                ProjectName = projectName,
                Subject = I18NHelper.FormatInvariant("Artifact {0} has been updated.", artifactInfo.Id),
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