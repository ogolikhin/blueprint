using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Repositories;
using BluePrintSys.Messaging.CrossCutting.Helpers;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Exceptions;
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

namespace ArtifactStore.Helpers
{
    public interface IWorkflowEventsMessagesHelper
    {
        Task<IList<IWorkflowMessage>> GenerateMessages(int userId,
            int revisionId,
            string userName,
            long transactionId,
            WorkflowEventTriggers postOpTriggers,
            IBaseArtifactVersionControlInfo artifactInfo,
            string projectName,
            IDictionary<int, IList<Property>> modifiedProperties,
            WorkflowState currentState,
            WorkflowState newState,
            bool sendArtifactPublishedMessage,
            string artifactUrl,
            string baseUrl,
            IUsersRepository usersRepository,
            IServiceLogRepository serviceLogRepository,
            IWebhooksRepository webhooksRepository,
            IProjectMetaRepository projectMetaRepository,
            IArtifactVersionsRepository artifactVersionsRepository,
            IDbTransaction transaction = null);

        Task ProcessMessages(string logSource,
            IApplicationSettingsRepository applicationSettingsRepository,
            IServiceLogRepository serviceLogRepository,
            IList<IWorkflowMessage> messages,
            string exceptionMessagePrepender,
            IDbTransaction transaction = null);
    }

    public class WorkflowEventsMessagesHelper : IWorkflowEventsMessagesHelper
    {
        private const string LogSource = "ArtifactStore.Helpers.WorkflowEventsMessagesHelper";
        private const string WebhookEventType = "ArtifactStateChanged";
        private const string WebhookPublisherId = "storyteller";
        private const string WebhookType = "Workflow";
        private const string WebhookGroupType = "Group";
        private const string WebhookUserType = "User";

        public async Task<IList<IWorkflowMessage>> GenerateMessages(int userId,
            int revisionId,
            string userName,
            long transactionId,
            WorkflowEventTriggers postOpTriggers,
            IBaseArtifactVersionControlInfo artifactInfo,
            string projectName,
            IDictionary<int, IList<Property>> modifiedProperties,
            WorkflowState currentState,
            WorkflowState newState,
            bool sendArtifactPublishedMessage,
            string artifactUrl,
            string baseUrl,
            IUsersRepository usersRepository,
            IServiceLogRepository serviceLogRepository,
            IWebhooksRepository webhooksRepository,
            IProjectMetaRepository projectMetaRepository,
            IArtifactVersionsRepository artifactVersionsRepository,
            IDbTransaction transaction = null)
        {
            var resultMessages = new List<IWorkflowMessage>();
            // var project = artifactResultSet?.Projects?.FirstOrDefault(d => d.Id == artifactInfo.ProjectId);
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
                            usersRepository,
                            transaction);
                        if (notificationMessage == null)
                        {
                            await serviceLogRepository.LogInformation(LogSource,
                                $"Skipping Email notification action for artifact {artifactInfo.Id}");
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
                            TransactionId = transactionId,
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

                        var customTypes =
                            await projectMetaRepository.GetCustomProjectTypesAsync(artifactInfo.ProjectId, userId);
                        var artifactType =
                            customTypes.ArtifactTypes.FirstOrDefault(at => at.Id == artifactInfo.ItemTypeId);

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
                            artifactType.CustomPropertyTypeIds,
                            transaction);

                        var revisionInfo = await webhooksRepository.GetRevisionInfos(new List<int> { revisionId }, transaction);

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
                                ParentId = ((VersionControlArtifactInfo)artifactInfo).ParentId,
                                ArtifactTypeId = artifactInfo.ItemTypeId,
                                ArtifactTypeName = artifactType?.Name,
                                BaseArtifactType = artifactType?.PredefinedType?.ToString(),
                                ArtifactPropertyInfo =
                                    await ConvertToWebhookPropertyInfo(artifactPropertyInfos, customTypes.PropertyTypes, usersRepository),
                                ChangedState = new WebhookStateChangeInfo
                                {
                                    NewValue = new WebhookStateInfo
                                    {
                                        Id = newState.Id,
                                        Name = newState.Name,
                                        WorkflowId = newState.WorkflowId
                                    },
                                    OldValue = new WebhookStateInfo
                                    {
                                        Id = currentState.Id,
                                        Name = currentState.Name,
                                        WorkflowId = currentState.WorkflowId
                                    }
                                },
                                Revision = revisionId,
                                RevisionTimestamp = revisionInfo?.FirstOrDefault()?.Timestamp,
                                Version = ((VersionControlArtifactInfo)artifactInfo).Version
                                    ?? ((VersionControlArtifactInfo)artifactInfo).VersionCount,
                                Id = artifactInfo.Id,
                                BlueprintUrl = string.Format($"{baseHostUri}?ArtifactId={artifactInfo.Id}"),
                                Link = string.Format(
                                    $"{baseHostUri}api/v1/projects/{artifactInfo.ProjectId}/artifacts/{artifactInfo.Id}")
                            }
                        };

                        var webhookMessage = await GetWebhookMessage(userId, revisionId, transactionId, webhookAction,
                            webhooksRepository, webhookArtifactInfo, transaction);

                        if (webhookMessage == null)
                        {
                            await serviceLogRepository.LogInformation(LogSource,
                                $"Skipping Webhook action for artifact {artifactInfo.Id}: {artifactInfo.Name}.");
                            continue;
                        }
                        resultMessages.Add(webhookMessage);
                        break;
                }
            }

            // Add published artifact message
            if (sendArtifactPublishedMessage)
            {
                var publishedMessage =
                    GetPublishedMessage(userId, revisionId, transactionId, artifactInfo, modifiedProperties) as
                        ArtifactsPublishedMessage;
                if (publishedMessage != null && publishedMessage.Artifacts.Any())
                {
                    resultMessages.Add(publishedMessage);
                }
            }

            var artifactIds = new List<int>
            {
                artifactInfo.Id
            };
            var artifactsChangedMessage = new ArtifactsChangedMessage(artifactIds)
            {
                TransactionId = transactionId,
                UserId = userId,
                RevisionId = revisionId,
                ChangeType = ArtifactChangedType.Publish
            };
            resultMessages.Add(artifactsChangedMessage);

            return resultMessages;
        }

        private async Task<IEnumerable<WebhookPropertyInfo>> ConvertToWebhookPropertyInfo(
            IEnumerable<ArtifactPropertyInfo> artifactPropertyInfos, List<PropertyType> propertyTypes, IUsersRepository usersRepository)
        {
            var webhookPropertyInfos = new Dictionary<int, WebhookPropertyInfo>();
            foreach (var artifactPropertyInfo in artifactPropertyInfos)
            {
                if (!artifactPropertyInfo.PropertyTypeId.HasValue)
                {
                    continue;
                }

                WebhookUserPropertyValue userProperty = null;

                if (artifactPropertyInfo.PrimitiveType == PropertyPrimitiveType.User)
                {
                    if (!artifactPropertyInfo.ValueId.HasValue)
                    {
                        continue;
                    }
                    var userInfo = (await usersRepository.GetUserInfos(new List<int> { artifactPropertyInfo.ValueId.Value })).FirstOrDefault();
                    if (userInfo != null)
                    {
                        userProperty = new WebhookUserPropertyValue
                        {
                            DisplayName = userInfo.DisplayName,
                            Id = userInfo.UserId,
                            Type = WebhookUserType
                        };
                    }
                    else
                    {
                        var group = (await usersRepository.GetExistingGroupsByIds(new List<int> { artifactPropertyInfo.ValueId.Value }, false)).FirstOrDefault();
                        if (group != null)
                        {
                            userProperty = new WebhookUserPropertyValue
                            {
                                Id = group.GroupId,
                                Name = group.Name,
                                Type = WebhookGroupType
                            };
                        }
                    }
                }
                if (webhookPropertyInfos.ContainsKey(artifactPropertyInfo.PropertyTypeId.Value))
                {
                    if (artifactPropertyInfo.PrimitiveType == PropertyPrimitiveType.Choice &&
                        webhookPropertyInfos[artifactPropertyInfo.PropertyTypeId.Value].Choices != null &&
                        artifactPropertyInfo.FullTextValue != null)
                    {
                        webhookPropertyInfos[artifactPropertyInfo.PropertyTypeId.Value].Choices.Add(artifactPropertyInfo.FullTextValue);
                    } else if (artifactPropertyInfo.PrimitiveType == PropertyPrimitiveType.User &&
                        webhookPropertyInfos[artifactPropertyInfo.PropertyTypeId.Value].UsersAndGroups != null &&
                        userProperty != null)
                    {
                        webhookPropertyInfos[artifactPropertyInfo.PropertyTypeId.Value].UsersAndGroups.Add(userProperty);
                    }
                }
                else
                {
                    var propertyType = propertyTypes.FirstOrDefault(pt => pt.Id == artifactPropertyInfo.PropertyTypeId);
                    bool isCustomChoice = false;
                    if (artifactPropertyInfo.PrimitiveType == PropertyPrimitiveType.Choice && propertyType != null)
                    {
                        isCustomChoice = propertyType.ValidValues.FirstOrDefault(vv =>
                                vv.Value.Equals(artifactPropertyInfo.FullTextValue)) == null;
                    }
                    webhookPropertyInfos[artifactPropertyInfo.PropertyTypeId.Value] = new WebhookPropertyInfo
                    {
                        BasePropertyType = artifactPropertyInfo.PrimitiveType.ToString(),
                        Choices = artifactPropertyInfo.PrimitiveType == PropertyPrimitiveType.Choice && !isCustomChoice
                            ? new List<string> { artifactPropertyInfo.FullTextValue }
                            : null,
                        DateValue = artifactPropertyInfo.DateTimeValue,
                        Name = artifactPropertyInfo.PropertyName,
                        NumberValue = artifactPropertyInfo.DecimalValue,
                        PropertyTypeId = artifactPropertyInfo.PropertyTypeId,
                        TextOrChoiceValue = artifactPropertyInfo.PrimitiveType == PropertyPrimitiveType.Text || isCustomChoice
                            ? (artifactPropertyInfo.IsRichText ? artifactPropertyInfo.HtmlTextValue : artifactPropertyInfo.FullTextValue)
                            : null,
                        UsersAndGroups = artifactPropertyInfo.PrimitiveType == PropertyPrimitiveType.User
                            ? new List<WebhookUserPropertyValue> { userProperty }
                            : null,
                        IsRichText = artifactPropertyInfo.PrimitiveType == PropertyPrimitiveType.Text
                            ? (bool?)artifactPropertyInfo.IsRichText
                            : null,
                        IsReadOnly = (artifactPropertyInfo.PropertyTypePredefined == (int)PropertyTypePredefined.ID ||
                             artifactPropertyInfo.PropertyTypePredefined == (int)PropertyTypePredefined.CreatedBy ||
                             artifactPropertyInfo.PropertyTypePredefined == (int)PropertyTypePredefined.LastEditedOn ||
                             artifactPropertyInfo.PropertyTypePredefined == (int)PropertyTypePredefined.LastEditedBy ||
                             artifactPropertyInfo.PropertyTypePredefined == (int)PropertyTypePredefined.CreatedOn)
                             ? (bool?)true
                             : null
                    };
                }
            }
            return webhookPropertyInfos.Values.ToList();
        }

        public async Task ProcessMessages(string logSource,
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
                    await WorkflowMessagingProcessor.Instance.SendMessageAsync(tenantInfo.TenantId, actionMessage);
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
            long transactionId,
            IBaseArtifactVersionControlInfo artifactInfo,
            string projectName,
            EmailNotificationAction notificationAction,
            string artifactUrl,
            string blueprintUrl,
            IUsersRepository usersRepository,
            IDbTransaction transaction)
        {
            string messageHeader = I18NHelper.FormatInvariant("You are being notified because of an update to the artifact with Id: {0}.", artifactInfo.Id);
            var artifactPartUrl = artifactUrl ?? ServerUriHelper.GetArtifactUrl(artifactInfo.Id, true);
            if (artifactPartUrl == null)
            {
                return null;
            }
            var baseUrl = blueprintUrl ?? ServerUriHelper.GetBaseHostUri()?.ToString();
            var emails = new List<string>();
            if (notificationAction.PropertyTypeId.HasValue && notificationAction.PropertyTypeId.Value > 0)
            {
                var userInfos =
                    await usersRepository.GetUserInfoForWorkflowArtifactForAssociatedUserProperty
                        (artifactInfo.Id,
                            notificationAction.PropertyTypeId.Value,
                            revisionId,
                            transaction);
                // Make sure that email is provided
                emails.AddRange(from userInfo in userInfos where !string.IsNullOrWhiteSpace(userInfo?.Email) select userInfo.Email);
            }
            else
            {
                // Take email from list of provided emails
                emails.AddRange(notificationAction.Emails ?? new List<string>());
            }

            var notificationMessage = new NotificationMessage
            {
                TransactionId = transactionId,
                ArtifactName = artifactInfo.Name,
                ProjectName = projectName,
                Subject = I18NHelper.FormatInvariant("Artifact {0} has been updated.", artifactInfo.Id),
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

        private static IWorkflowMessage GetPublishedMessage(int userId, int revisionId, long transactionId, IBaseArtifactVersionControlInfo artifactInfo, IDictionary<int, IList<Property>> modifiedProperties)
        {
            var message = new ArtifactsPublishedMessage
            {
                TransactionId = transactionId,
                UserId = userId,
                RevisionId = revisionId
            };
            var artifacts = new List<PublishedArtifactInformation>();
            var artifact = new PublishedArtifactInformation
            {
                Id = artifactInfo.Id,
                Name = artifactInfo.Name,
                Predefined = (int)artifactInfo.PredefinedType,
                IsFirstTimePublished = false, // State change always occurs on published artifacts
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
                // Only add artifact to list if there is a list of modified properties
                artifacts.Add(artifact);
            }

            message.Artifacts = artifacts;
            return message;
        }

        private static async Task<IWorkflowMessage> GetWebhookMessage(int userId, int revisionId, long transactionId, WebhookAction webhookAction,
            IWebhooksRepository webhooksRepository, WebhookArtifactInfo webhookArtifactInfo, IDbTransaction transaction)
        {
            List<int> webhookId = new List<int> { webhookAction.WebhookId };
            var webhookInfos = await webhooksRepository.GetWebhooks(webhookId, transaction);
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