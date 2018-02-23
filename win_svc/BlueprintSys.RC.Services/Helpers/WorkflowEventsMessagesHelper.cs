﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.MessageHandlers;
using BluePrintSys.Messaging.CrossCutting.Helpers;
using BluePrintSys.Messaging.CrossCutting.Logging;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.VersionControl;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Models.Workflow.Actions;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;
using ServiceLibrary.Repositories.Webhooks;

namespace BlueprintSys.RC.Services.Helpers
{
    public class WorkflowEventsMessagesHelper
    {
        private const string LogSource = "StateChange.WorkflowEventsMessagesHelper";

        public static async Task<IList<IWorkflowMessage>> GenerateMessages(int userId, 
            int revisionId, 
            string userName,
            long transactionId,
            WorkflowEventTriggers postOpTriggers, 
            IBaseArtifactVersionControlInfo artifactInfo, 
            string projectName, 
            IDictionary<int, IList<Property>> modifiedProperties, 
            string artifactUrl, 
            string baseUrl,
            IEnumerable<int> ancestorArtifactTypeIds, 
            IUsersRepository usersRepository, 
            IServiceLogRepository serviceLogRepository,
            IWebhooksRepository webhooksRepository,
            IEnumerable<ArtifactPropertyInfo> artifactPropertyInfos)
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
                            transactionId,
                            artifactInfo,
                            projectName,
                            notificationAction,
                            artifactUrl,
                            baseHostUri,
                            usersRepository);
                        if (notificationMessage == null)
                        {
                            await serviceLogRepository.LogInformation(LogSource, $"Skipping Email notification action for artifact {artifactInfo.Id}");
                            Log.Debug($" Skipping Email notification action for artifact {artifactInfo.Id}. Message: Notification.");
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
                        var ancestors = new List<int>(ancestorArtifactTypeIds ?? new int[0]);
                        ancestors.Add(artifactInfo.ItemTypeId);
                        var generateChildrenMessage = new GenerateDescendantsMessage
                        {
                            TransactionId = transactionId,
                            ChildCount = generateChildrenAction.ChildCount.GetValueOrDefault(10),
                            DesiredArtifactTypeId = generateChildrenAction.ArtifactTypeId,
                            ArtifactId = artifactInfo.Id,
                            AncestorArtifactTypeIds = ancestors,
                            RevisionId = revisionId,
                            UserId = userId,
                            ProjectId = artifactInfo.ProjectId,
                            UserName = userName,
                            BaseHostUri = baseHostUri,
                            ProjectName = projectName,
                            TypePredefined = (int) artifactInfo.PredefinedType
                        };
                        resultMessages.Add(generateChildrenMessage);
                        break;
                    case MessageActionType.GenerateTests:
                        var generateTestsAction = workflowEventTrigger.Action as GenerateTestCasesAction;
                        if (generateTestsAction == null || artifactInfo.PredefinedType != ItemTypePredefined.Process)
                        {
                            await serviceLogRepository.LogInformation(LogSource, $"Skipping GenerateTestCasesAction for artifact {artifactInfo.Id} as it is not a process");
                            Log.Debug($"Skipping GenerateTestCasesAction for artifact {artifactInfo.Id} as it is not a process. Message: Notification.");
                            continue;
                        }
                        var generateTestsMessage = new GenerateTestsMessage
                        {
                            TransactionId = transactionId,
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
                            Log.Debug($"Skipping GenerateUserStories for artifact {artifactInfo.Id} as it is not a process. Message: Notification.");
                            continue;
                        }
                        var generateUserStoriesMessage = new GenerateUserStoriesMessage
                        {
                            TransactionId = transactionId,
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
                    case MessageActionType.Webhooks:
                        var webhookAction = workflowEventTrigger.Action as WebhookAction;
                        if (webhookAction == null)
                        {
                            continue;
                        }

                        var webhookArtifactInfo = new WebhookArtifactInfo
                        {
                            Id = "",
                            EventType = "",
                            PublisherId = "",
                            Scope = new WebhookArtifactInfoScope
                            {
                                Type = "",
                                WorkflowId = -1
                            },
                            Resource = new WebhookResource
                            {
                                Name = artifactInfo.Name,
                                ProjectId = artifactInfo.ProjectId,
                                ParentId = -1,
                                ArtifactTypeId = artifactInfo.ItemTypeId,
                                ArtifactTypeName = "",
                                BaseArtifactType = "",
                                ArtifactPropertyInfo = ConvertToWebhookPropertyInfo(artifactPropertyInfos),
                                State = new WebhookStateInfo
                                {
                                    Id = -1,
                                    Name = "",
                                    WorkflowId = -1
                                },
                                ChangedState = new WebhookStateChangeInfo
                                {
                                    NewValue = new WebhookStateInfo
                                    {
                                        Id = -1,
                                        Name = "",
                                        WorkflowId = -1
                                    },
                                    OldValue = new WebhookStateInfo
                                    {
                                        Id = -1,
                                        Name = "",
                                        WorkflowId = -1
                                    }
                                },
                                RevisionTime = "",
                                Revision = -1,
                                Version = -1,
                                Id = artifactInfo.Id,
                                BlueprintUrl = string.Format($"{baseHostUri}?ArtifactId={artifactInfo.Id}"),
                                Link = string.Format($"{baseHostUri}api/v1/projects/{artifactInfo.ProjectId}/artifacts/{artifactInfo.Id}")
                            }
                        };
                        var webhookMessage = await GetWebhookMessage(userId, revisionId, transactionId, webhookAction, webhooksRepository, artifactInfo);

                        if (webhookMessage == null)
                        {
                            await serviceLogRepository.LogInformation(LogSource, $"Skipping Webhook action for artifact {artifactInfo.Id}: {artifactInfo.Name}.");
                            continue;
                        }
                        resultMessages.Add(webhookMessage);
                        break;
                }
            }
            return resultMessages;
        }

        private static IEnumerable<WebhookPropertyInfo> ConvertToWebhookPropertyInfo(IEnumerable<ArtifactPropertyInfo> artifactPropertyInfos)
        {
            var webhookPropertyInfos = new List<WebhookPropertyInfo>();
            foreach (var artifactPropertyInfo in artifactPropertyInfos)
            {
                webhookPropertyInfos.Add(new WebhookPropertyInfo
                {
                    BasePropertyType = "",
                    Choices = new List<string>(),
                    DateValue = "",
                    Name = artifactPropertyInfo.PropertyName,
                    NumberValue = -1f,
                    PropertyTypeId = artifactPropertyInfo.PropertyTypeId,
                    TextOrChoiceValue = "",
                    UsersAndGroups = new List<WebhookUserPropertyValue>()
                });
            }
            return webhookPropertyInfos;
        }

        public static async Task ProcessMessages(string logSource,
            TenantInformation tenant,
            IServiceLogRepository serviceLogRepository,
            IList<IWorkflowMessage> messages,
            string exceptionMessagePrepender,
            IWorkflowMessagingProcessor workflowMessagingProcessor)
        {
            if (messages == null || messages.Count <= 0)
            {
                return;
            }

            var processor = workflowMessagingProcessor ?? WorkflowMessagingProcessor.Instance;

            foreach (var actionMessage in messages.Where(a => a != null))
            {
                try
                {
                    await ActionMessageSender.Send((ActionMessage) actionMessage, tenant, processor);
                    string message = $"Sent {actionMessage.ActionType} message: {actionMessage.ToJSON()} with tenant id: {tenant.TenantId} to the Message queue";
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
            long transactionId,
            IBaseArtifactVersionControlInfo artifactInfo,
            string projectName,
            EmailNotificationAction notificationAction,
            string artifactUrl,
            string blueprintUrl,
            IUsersRepository usersRepository)
        {
            string messageHeader = I18NHelper.FormatInvariant("You are being notified because artifact with Id: {0} has been created.", artifactInfo.Id);
            var artifactPartUrl = artifactUrl ?? ServerUriHelper.GetArtifactUrl(artifactInfo.Id, true);
            if (artifactPartUrl == null)
            {
                return null;
            }
            var baseUrl = blueprintUrl ?? ServerUriHelper.GetBaseHostUri()?.ToString();
            var emails = await GetEmailValues(revisionId, artifactInfo.Id, notificationAction, usersRepository);

            var notificationMessage = new NotificationMessage
            {
                TransactionId = transactionId,
                ArtifactName = artifactInfo.Name,
                ProjectName = projectName,
                Subject = I18NHelper.FormatInvariant("Artifact {0} has been created.",artifactInfo.Id),
                From = notificationAction.FromDisplayName,
                To = emails,
                Message = notificationAction.Message,
                RevisionId = revisionId,
                UserId = userId,
                ArtifactTypeId = artifactInfo.ItemTypeId,
                ArtifactId = artifactInfo.Id,
                ArtifactUrl = artifactPartUrl,
                ArtifactTypePredefined = (int)artifactInfo.PredefinedType,
                ProjectId = artifactInfo.ProjectId,
                Header = messageHeader,
                BlueprintUrl = baseUrl
            };
            return notificationMessage;
        }

        internal static async Task<List<string>>  GetEmailValues(int revisionId, int artifactId,
            EmailNotificationAction notificationAction, IUsersRepository usersRepository)
        {
            var emails = new List<string>();
            if (notificationAction.PropertyTypeId.HasValue && notificationAction.PropertyTypeId.Value > 0)
            {
                var userInfos =
                    await usersRepository.GetUserInfoForWorkflowArtifactForAssociatedUserProperty
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

        private static async Task<IWorkflowMessage> GetWebhookMessage(int userId, int revisionId, long transactionId, WebhookAction webhookAction,
            IWebhooksRepository webhooksRepository, IBaseArtifactVersionControlInfo artifactInfo)
        {
            List<int> webhookId = new List<int> { webhookAction.WebhookId };
            var webhookInfos = await webhooksRepository.GetWebhooks(webhookId);
            var webhookInfo = webhookInfos.FirstOrDefault();

            var securityInfo = SerializationHelper.FromXml<XmlWebhookSecurityInfo>(webhookInfo.SecurityInfo);

            var webhookMessage = new WebhookMessage
            {
                TransactionId = transactionId,
                UserId = userId,
                RevisionId = revisionId,
                // Authentication Information
                Url = webhookInfo.Url,
                IgnoreInvalidSSLCertificate = securityInfo.IgnoreInvalidSSLCertificate,
                HttpHeaders = securityInfo.HttpHeaders,
                BasicAuthUsername = securityInfo.BasicAuth?.Username,
                BasicAuthPassword = securityInfo.BasicAuth?.Password,
                SignatureSecretToken = securityInfo.Signature?.SecretToken,
                SignatureAlgorithm = securityInfo.Signature?.Algorithm,
                // Payload Information
                WebhookJsonPayload = artifactInfo.ToJSON()
            };

            return webhookMessage;
        }
    }
}
