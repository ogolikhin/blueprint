using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.MessageHandlers;
using BluePrintSys.Messaging.CrossCutting.Helpers;
using BluePrintSys.Messaging.CrossCutting.Logging;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.VersionControl;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Models.Workflow.Actions;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;
using ServiceLibrary.Repositories.ProjectMeta;
using ServiceLibrary.Repositories.Webhooks;

namespace BlueprintSys.RC.Services.Helpers
{
    public class WorkflowEventsMessagesHelper
    {
        private const string LogSource = "StateChange.WorkflowEventsMessagesHelper";
        private const string WebhookEventType = "ArtifactCreated";
        private const string WebhookPublisherId = "storyteller";
        private const string WebhookType = "Workflow";
        private const int WebhookArtifactVersion = 1;

        public static async Task<IList<IWorkflowMessage>> GenerateMessages(int userId,
            int revisionId,
            string userName,
            long transactionId,
            WorkflowEventTriggers postOpTriggers,
            IBaseArtifactVersionControlInfo artifactInfo,
            string projectName,
            IDictionary<int, IList<Property>> modifiedProperties,
            WorkflowState currentState,
            string artifactUrl,
            string baseUrl,
            IEnumerable<int> ancestorArtifactTypeIds,
            IUsersRepository usersRepository,
            IServiceLogRepository serviceLogRepository,
            IWebhooksRepository webhooksRepository,
            IProjectMetaRepository projectMetaRepository)
        {
            var resultMessages = new List<IWorkflowMessage>();
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
                            TypePredefined = (int)artifactInfo.PredefinedType
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

                        var customTypes = await projectMetaRepository.GetCustomProjectTypesAsync(artifactInfo.ProjectId, userId);
                        var artifactType = customTypes.ArtifactTypes.FirstOrDefault(at => at.Id == artifactInfo.ItemTypeId);

                        var artifactPropertyInfos = await webhooksRepository.GetArtifactsWithPropertyValuesAsync(
                            userId,
                            new List<int> { artifactInfo.Id },
                            new List<int>
                            {
                                (int)PropertyTypePredefined.Name,
                                (int)PropertyTypePredefined.Description,
                                (int)PropertyTypePredefined.ID,
                                (int)PropertyTypePredefined.CreatedBy,
                                (int)PropertyTypePredefined.LastEditedOn,
                                (int)PropertyTypePredefined.LastEditedBy,
                                (int)PropertyTypePredefined.CreatedOn
                            },
                            artifactType.CustomPropertyTypeIds);

                        var webhookArtifactInfo = new WebhookArtifactInfo
                        {
                            Id = Guid.NewGuid().ToString(),
                            EventType = WebhookEventType,
                            PublisherId = WebhookPublisherId,
                            Scope = new WebhookArtifactInfoScope
                            {
                                Type = WebhookType,
                                WorkflowId = currentState.WorkflowId
                            },
                            Resource = new WebhookResource
                            {
                                Name = artifactInfo.Name,
                                ProjectId = artifactInfo.ProjectId,
                                ParentId = ((WorkflowMessageArtifactInfo)artifactInfo).ParentId,
                                ArtifactTypeId = artifactInfo.ItemTypeId,
                                ArtifactTypeName = artifactType?.Name,
                                BaseArtifactType = artifactType?.PredefinedType?.ToString(),
                                ArtifactPropertyInfo = ConvertToWebhookPropertyInfo(artifactPropertyInfos),
                                State = new WebhookStateInfo
                                {
                                    Id = currentState.Id,
                                    Name = currentState.Name,
                                    WorkflowId = currentState.WorkflowId
                                },
                                RevisionTime = "",
                                Revision = revisionId,
                                Version = WebhookArtifactVersion,
                                Id = artifactInfo.Id,
                                BlueprintUrl = string.Format($"{baseHostUri}?ArtifactId={artifactInfo.Id}"),
                                Link = string.Format($"{baseHostUri}api/v1/projects/{artifactInfo.ProjectId}/artifacts/{artifactInfo.Id}")
                            }
                        };
                        var webhookMessage = await GetWebhookMessage(userId, revisionId, transactionId, webhookAction, webhooksRepository, webhookArtifactInfo);

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
                    BasePropertyType = artifactPropertyInfo.PrimitiveType.ToString(),
                    Choices = artifactPropertyInfo.PrimitiveType == PropertyPrimitiveType.Choice ? artifactPropertyInfo.FullTextValue.Split(',') : null,
                    DateValue = artifactPropertyInfo.PrimitiveType == PropertyPrimitiveType.Date ? artifactPropertyInfo.DateTimeValue.ToString() : null,
                    Name = artifactPropertyInfo.PropertyName,
                    NumberValue = artifactPropertyInfo.PrimitiveType == PropertyPrimitiveType.Number ? (float?)float.Parse(artifactPropertyInfo.FullTextValue, CultureInfo.InvariantCulture) : null,
                    PropertyTypeId = artifactPropertyInfo.PropertyTypeId,
                    TextOrChoiceValue = artifactPropertyInfo.FullTextValue,
                    UsersAndGroups = artifactPropertyInfo.PrimitiveType == PropertyPrimitiveType.User ? new List<WebhookUserPropertyValue>() : null
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
                    await ActionMessageSender.Send((ActionMessage)actionMessage, tenant, processor);
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
                Subject = I18NHelper.FormatInvariant("Artifact {0} has been created.", artifactInfo.Id),
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

        internal static async Task<List<string>> GetEmailValues(int revisionId, int artifactId,
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
                // Make sure that email is provided
                emails.AddRange(from userInfo in userInfos
                                where !string.IsNullOrWhiteSpace(userInfo?.Email)
                                select userInfo.Email);
            }
            else
            {
                // Take email from list of provided emails
                emails.AddRange(notificationAction.Emails ?? new List<string>());
            }
            return emails;
        }

        private static async Task<IWorkflowMessage> GetWebhookMessage(int userId, int revisionId, long transactionId, WebhookAction webhookAction,
            IWebhooksRepository webhooksRepository, WebhookArtifactInfo webhookArtifactInfo)
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
                WebhookJsonPayload = webhookArtifactInfo.ToJSON()
            };

            return webhookMessage;
        }
    }
}
