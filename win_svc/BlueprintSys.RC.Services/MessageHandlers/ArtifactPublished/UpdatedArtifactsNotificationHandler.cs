using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BlueprintSys.RC.Services.Models;
using BlueprintSys.RC.Services.Repositories;
using BluePrintSys.Messaging.CrossCutting.Helpers;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Models.Workflow.Actions;
using ServiceLibrary.Repositories.ConfigControl;

namespace BlueprintSys.RC.Services.MessageHandlers.ArtifactPublished
{
    internal class UpdatedArtifactsNotificationHandler
    {
        private const string LogSource = "ArtifactsPublishedActionHelper.UpdatedArtifacts";
        internal static async Task<bool> ProcessUpdatedArtifacts(TenantInformation tenant,
            
            ArtifactsPublishedMessage message,
            IArtifactsPublishedRepository repository,
            IServiceLogRepository serviceLogRepository,
            IActionsParser actionsParser,
            IWorkflowMessagingProcessor messageProcessor)
        {
            var allUpdatedArtifacts = message?.Artifacts?.Where(p => !p.IsFirstTimePublished).ToList();
            if (allUpdatedArtifacts == null || allUpdatedArtifacts.Count <= 0)
            {
                return false;
            }

            var updatedArtifacts = allUpdatedArtifacts.Where(a => a.ModifiedProperties?.Count > 0).ToList();
            if (updatedArtifacts.Count == 0)
            {
                return false;
            }

            //Get artifacts which have modified properties list populated
            var allArtifactsModifiedProperties = updatedArtifacts.ToDictionary(k => k.Id,
                v => v.ModifiedProperties ?? new List<PublishedPropertyInformation>());

            Logger.Log(
                $"{allArtifactsModifiedProperties.Count} artifacts found: {string.Join(", ", allArtifactsModifiedProperties.Select(k => k.Key))}",
                message, tenant, LogLevel.Debug);

            if (allArtifactsModifiedProperties.Count == 0)
            {
                return false;
            }

            //Get property transitions for published artifact ids.
            var publishedArtifactIds = updatedArtifacts.Select(a => a.Id).ToHashSet();
            var artifactPropertyEvents =
                await
                    repository.GetWorkflowPropertyTransitionsForArtifactsAsync(message.UserId,
                    message.RevisionId,
                        (int)TransitionType.Property, publishedArtifactIds);
            //if no property transitions found, then call does not need to proceed
            Logger.Log($"{artifactPropertyEvents?.Count ?? 0} workflow property events found", message, tenant, LogLevel.Debug);
            if (artifactPropertyEvents == null || artifactPropertyEvents.Count == 0)
            {
                return false;
            }

            //convert all property transitions to a dictionary with artifact id as key
            var activePropertyTransitions = new Dictionary<int, IList<SqlWorkflowEvent>>();
            var publishedArtifactEvents = artifactPropertyEvents.Where(ape => publishedArtifactIds.Contains(ape.VersionItemId));
            //key = artifact id, value = all events
            foreach (var artifactPropertyEvent in publishedArtifactEvents)
            {
                if (activePropertyTransitions.ContainsKey(artifactPropertyEvent.VersionItemId))
                {
                    activePropertyTransitions[artifactPropertyEvent.VersionItemId].Add(artifactPropertyEvent);
                }
                else
                {
                    activePropertyTransitions.Add(artifactPropertyEvent.VersionItemId,
                        new List<SqlWorkflowEvent> { artifactPropertyEvent });
                }
            }

            var sqlWorkflowStates =
                await
                    repository.GetWorkflowStatesForArtifactsAsync(message.UserId, activePropertyTransitions.Keys,
                        message.RevisionId);
            var workflowStates = sqlWorkflowStates.Where(w => w.WorkflowStateId > 0).ToDictionary(k => k.ArtifactId);
            Logger.Log(
                $"{workflowStates.Count} workflow states found for artifacts: {string.Join(", ", workflowStates.Select(k => k.Key))}",
                message, tenant, LogLevel.Debug);
            if (workflowStates.Count == 0)
            {
                return false;
            }

            //Get project names
            var projectIds = updatedArtifacts.Select(a => a.ProjectId).ToList();
            var projects = await repository.GetProjectNameByIdsAsync(projectIds);
            Logger.Log($"{projects.Count} project names found for project IDs: {string.Join(", ", projectIds)}", message, tenant,
                LogLevel.Debug);

            var notificationMessages = new Dictionary<int, List<IWorkflowMessage>>();

            //for artifacts in active property transitions
            foreach (var artifactId in activePropertyTransitions.Keys)
            {
                Logger.Log($"Processing artifact with ID: {artifactId}", message, tenant, LogLevel.Debug);

                var artifactTransitionInfo = activePropertyTransitions[artifactId];
                var notifications = actionsParser.GetNotificationActions(artifactTransitionInfo).ToList();
                Logger.Log($"{notifications.Count} Notification actions found", message, tenant, LogLevel.Debug);
                if (notifications.Count == 0)
                {
                    continue;
                }

                List<PublishedPropertyInformation> artifactModifiedProperties;
                if (!allArtifactsModifiedProperties.TryGetValue(artifactId, out artifactModifiedProperties))
                {
                    Logger.Log($"modified properties not found for {artifactId}", message, tenant, LogLevel.Debug);
                    continue;
                }
                
                Logger.Log($"{artifactModifiedProperties?.Count ?? 0} modified properties found", message, tenant,
                    LogLevel.Debug);
                if (artifactModifiedProperties == null || !artifactModifiedProperties.Any())
                {
                    continue;
                }

                SqlWorkFlowStateInformation currentStateInfo;
                if (!workflowStates.TryGetValue(artifactId, out currentStateInfo))
                {
                    continue;
                }

                var modifiedSystemPropertiesSet = artifactModifiedProperties.Where(a => a.PredefinedType == (int)PropertyTypePredefined.Name ||
                a.PredefinedType == (int)PropertyTypePredefined.Description)
                    .Select(a => (PropertyTypePredefined)a.PredefinedType)
                    .ToHashSet();

                await ProcessSystemPropertyChange(tenant, message, repository, notifications, modifiedSystemPropertiesSet, currentStateInfo, updatedArtifacts, artifactId, workflowStates, projects, notificationMessages);

                var modifiedCustomPropertiesSet = artifactModifiedProperties.Where(a => a.PredefinedType == (int)PropertyTypePredefined.CustomGroup)
                    .Select(a => a.TypeId)
                    .ToHashSet();

                //Process custom properties
                Logger.Log(
                    $"{modifiedCustomPropertiesSet.Count} instance property type IDs being located: {string.Join(", ", modifiedCustomPropertiesSet)}",
                    message, tenant, LogLevel.Debug);

                await ProcessCustomPropertyChange(tenant, message, repository, notifications, modifiedCustomPropertiesSet, currentStateInfo, updatedArtifacts, artifactId, workflowStates, projects, notificationMessages);
            }

            if (notificationMessages.Count == 0)
            {
                return false;
            }

            foreach (var notificationMessage in notificationMessages)
            {
                await WorkflowEventsMessagesHelper.ProcessMessages(LogSource,
                    tenant.TenantId,
                    serviceLogRepository,
                     notificationMessage.Value,
                    $"Error on new artifact creation with Id: {notificationMessage.Key}",
                    messageProcessor);
            }

            return true;
        }

        private static async Task ProcessSystemPropertyChange(TenantInformation tenant, 
            ArtifactsPublishedMessage message, 
            IArtifactsPublishedRepository repository, 
            List<EmailNotificationAction> notifications, 
            HashSet<PropertyTypePredefined> modifiedSystemPropertiesSet, 
            SqlWorkFlowStateInformation currentStateInfo, 
            List<PublishedArtifactInformation> updatedArtifacts, 
            int artifactId, 
            Dictionary<int, SqlWorkFlowStateInformation> workflowStates, 
            List<SqlProject> projects, 
            Dictionary<int, List<IWorkflowMessage>> notificationMessages)
        {
            if (modifiedSystemPropertiesSet.Count == 0)
            {
                return;
            }

            foreach (var notificationAction in notifications)
            {
                Logger.Log("Processing notification action", message, tenant, LogLevel.Debug);

                if (!notificationAction.EventPropertyTypeId.HasValue)
                {
                    continue;
                }

                int eventPropertyTypeId = notificationAction.EventPropertyTypeId.Value;

                //If system property provided is neither name or description
                if (eventPropertyTypeId != WorkflowConstants.PropertyTypeFakeIdName &&
                    eventPropertyTypeId != WorkflowConstants.PropertyTypeFakeIdDescription)
                {
                    Logger.Log(
                        $"The system property type ID {notificationAction.EventPropertyTypeId} is not supported. Only Name and Description are supported.",
                        message, tenant, LogLevel.Debug);
                    continue;
                }

                //if modified properties does not conatin event property type Id
                if (!modifiedSystemPropertiesSet.Contains(GetPropertyTypePredefined(notificationAction.EventPropertyTypeId.Value)))
                {
                    continue;
                }

                if (notificationAction.ConditionalStateId.HasValue &&
                    (currentStateInfo == null ||
                     currentStateInfo.WorkflowStateId != notificationAction.ConditionalStateId.Value))
                {
                    //the conditional state id is present, but either the current state info is not present or the current state is not same as conditional state
                    var currentStateId = currentStateInfo?.WorkflowStateId.ToString() ?? "none";
                    Logger.Log(
                        $"Conditional state ID {notificationAction.ConditionalStateId.Value} does not match current state ID: {currentStateId}",
                        message, tenant, LogLevel.Debug);
                    continue;
                }

                var artifact = updatedArtifacts.First(a => a.Id == artifactId);

                string messageHeader =
                    I18NHelper.FormatInvariant("You are being notified because artifact with Id: {0} has been updated.",
                        artifactId);
                var artifactPartUrl = artifact.Url ?? ServerUriHelper.GetArtifactUrl(artifactId, true);
                var blueprintUrl = artifact.BaseUrl ?? ServerUriHelper.GetBaseHostUri()?.ToString();
                var emails = await WorkflowEventsMessagesHelper.GetEmailValues(message.RevisionId, artifactId,
                    notificationAction, repository.UsersRepository);

                var notificationMessage = new NotificationMessage
                {
                    ArtifactName = workflowStates[artifactId].Name,
                    ProjectName = projects.First(p => p.ItemId == artifact.ProjectId).Name,
                    Subject = notificationAction.Subject,
                    From = notificationAction.FromDisplayName,
                    To = emails,
                    Header = messageHeader,
                    Message = notificationAction.Message,
                    RevisionId = message.RevisionId,
                    UserId = message.UserId,
                    ArtifactTypeId = currentStateInfo.ItemTypeId,
                    ArtifactId = artifactId,
                    ArtifactUrl = artifactPartUrl,
                    ArtifactTypePredefined = artifact.Predefined,
                    ProjectId = artifact.ProjectId,
                    BlueprintUrl = blueprintUrl
                };

                if (notificationMessages.ContainsKey(artifactId))
                {
                    notificationMessages[artifactId].Add(notificationMessage);
                }
                else
                {
                    notificationMessages.Add(artifactId,
                        new List<IWorkflowMessage>
                        {
                            notificationMessage
                        });
                }
            }
        }

        private static PropertyTypePredefined GetPropertyTypePredefined(int propertyTypeId)
        {
            if (propertyTypeId == WorkflowConstants.PropertyTypeFakeIdName)
            {
                return PropertyTypePredefined.Name;
            }
            if (propertyTypeId == WorkflowConstants.PropertyTypeFakeIdDescription)
            {
                return PropertyTypePredefined.Description;
            }
            return PropertyTypePredefined.None;
        }

        private static async Task ProcessCustomPropertyChange(TenantInformation tenant, 
            ArtifactsPublishedMessage message,
            IArtifactsPublishedRepository repository, 
            List<EmailNotificationAction> notifications,
            HashSet<int> modifiedCustomPropertiesSet,
            SqlWorkFlowStateInformation currentStateInfo, 
            List<PublishedArtifactInformation> updatedArtifacts, 
            int artifactId, 
            Dictionary<int, SqlWorkFlowStateInformation> workflowStates,
            List<SqlProject> projects, 
            Dictionary<int, List<IWorkflowMessage>> notificationMessages)
        {
            if (modifiedCustomPropertiesSet.Count == 0)
            {
                return;
            }
            //Dictionary<int, List<int>> instancePropertyTypeIds
            var instancePropertyTypeIds = await repository.GetInstancePropertyTypeIdsMap(modifiedCustomPropertiesSet);
            Logger.Log(
                $"{instancePropertyTypeIds.Count} instance property type IDs found: {string.Join(", ", instancePropertyTypeIds.Select(k => k.Key))}",
                message, tenant, LogLevel.Debug);

            foreach (var notificationAction in notifications)
            {
                Logger.Log("Processing notification action", message, tenant, LogLevel.Debug);

                if (!notificationAction.EventPropertyTypeId.HasValue)
                {
                    continue;
                }

                int eventPropertyTypeId = notificationAction.EventPropertyTypeId.Value;
                if (!instancePropertyTypeIds.ContainsKey(eventPropertyTypeId))
                {
                    Logger.Log(
                        $"The property type ID {notificationAction.EventPropertyTypeId} was not found in the dictionary of instance property type IDs.",
                        message, tenant, LogLevel.Debug);
                    continue;
                }

                List<int> propertyTypeIds;
                if (!instancePropertyTypeIds.TryGetValue(eventPropertyTypeId, out propertyTypeIds) || propertyTypeIds.IsEmpty())
                {
                    Logger.Log(
                        $"The property type ID {notificationAction.EventPropertyTypeId} was not found in the dictionary of instance property type IDs.",
                        message, tenant, LogLevel.Debug);
                    continue;
                }

                if (notificationAction.ConditionalStateId.HasValue &&
                    (currentStateInfo == null ||
                     currentStateInfo.WorkflowStateId != notificationAction.ConditionalStateId.Value))
                {
                    //the conditional state id is present, but either the current state info is not present or the current state is not same as conditional state
                    var currentStateId = currentStateInfo?.WorkflowStateId.ToString() ?? "none";
                    Logger.Log(
                        $"Conditional state ID {notificationAction.ConditionalStateId.Value} does not match current state ID: {currentStateId}",
                        message, tenant, LogLevel.Debug);
                    continue;
                }

                var artifact = updatedArtifacts.First(a => a.Id == artifactId);

                string messageHeader =
                    I18NHelper.FormatInvariant("You are being notified because artifact with Id: {0} has been updated.",
                        artifactId);
                var artifactPartUrl = artifact.Url ?? ServerUriHelper.GetArtifactUrl(artifactId, true);
                var blueprintUrl = artifact.BaseUrl ?? ServerUriHelper.GetBaseHostUri()?.ToString();
                var emails = await WorkflowEventsMessagesHelper.GetEmailValues(message.RevisionId, artifactId,
                    notificationAction, repository.UsersRepository);

                var notificationMessage = new NotificationMessage
                {
                    ArtifactName = workflowStates[artifactId].Name,
                    ProjectName = projects.First(p => p.ItemId == artifact.ProjectId).Name,
                    Subject = notificationAction.Subject,
                    From = notificationAction.FromDisplayName,
                    To = emails,
                    Header = messageHeader,
                    Message = notificationAction.Message,
                    RevisionId = message.RevisionId,
                    UserId = message.UserId,
                    ArtifactTypeId = currentStateInfo.ItemTypeId,
                    ArtifactId = artifactId,
                    ArtifactUrl = artifactPartUrl,
                    ArtifactTypePredefined = artifact.Predefined,
                    ProjectId = artifact.ProjectId,
                    BlueprintUrl = blueprintUrl
                };

                if (notificationMessages.ContainsKey(artifactId))
                {
                    notificationMessages[artifactId].Add(notificationMessage);
                }
                else
                {
                    notificationMessages.Add(artifactId,
                        new List<IWorkflowMessage>
                        {
                            notificationMessage
                        });
                }
            }
        }
    }
}
