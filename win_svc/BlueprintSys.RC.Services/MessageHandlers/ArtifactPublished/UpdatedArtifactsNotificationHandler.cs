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
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Repositories.ConfigControl;

namespace BlueprintSys.RC.Services.MessageHandlers.ArtifactPublished
{
    internal class UpdatedArtifactsNotificationHandler
    {
        private const string LogSource = "ArtifactsPublishedActionHelper.UpdatedArtifacts";
        internal static async Task<bool> ProcessUpdatedArtifacts(TenantInformation tenant,
            ICollection<PublishedArtifactInformation> updatedArtifacts,
            ArtifactsPublishedMessage message,
            IArtifactsPublishedRepository repository,
            IServiceLogRepository serviceLogRepository,
            IActionsParser actionsParser)
        {
            //Get artifacts which have modified properties list populated
            var allArtifactsModifiedProperties = updatedArtifacts.ToDictionary(k => k.Id,
                v => v.ModifiedProperties ?? new List<PublishedPropertyInformation>());
            Logger.Log(
                $"{allArtifactsModifiedProperties.Count} artifacts found: {string.Join(", ", allArtifactsModifiedProperties.Select(k => k.Key))}",
                message, tenant, LogLevel.Debug);
            if (allArtifactsModifiedProperties.Count == 0)
            {
                return true;
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
                return true;
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
                return true;
            }

            //Get project names
            var projectIds = updatedArtifacts.Select(a => a.ProjectId).ToList();
            var projects = await repository.GetProjectNameByIdsAsync(projectIds);
            Logger.Log($"{projects.Count} project names found for project IDs: {string.Join(", ", projectIds)}", message, tenant,
                LogLevel.Debug);

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

                var artifactModifiedProperties = allArtifactsModifiedProperties[artifactId];
                Logger.Log($"{artifactModifiedProperties?.Count ?? 0} modified properties found", message, tenant,
                    LogLevel.Debug);
                if (artifactModifiedProperties == null || !artifactModifiedProperties.Any())
                {
                    continue;
                }

                var artifactModifiedPropertiesSet = artifactModifiedProperties.Select(a => a.TypeId).ToHashSet();
                Logger.Log(
                    $"{artifactModifiedPropertiesSet.Count} instance property type IDs being located: {string.Join(", ", artifactModifiedPropertiesSet)}",
                    message, tenant, LogLevel.Debug);
                var instancePropertyTypeIds = await repository.GetInstancePropertyTypeIdsMap(artifactModifiedPropertiesSet);
                Logger.Log(
                    $"{instancePropertyTypeIds.Count} instance property type IDs found: {string.Join(", ", instancePropertyTypeIds.Select(k => k.Key))}",
                    message, tenant, LogLevel.Debug);

                var currentStateInfo = workflowStates[artifactId];

                foreach (var notificationAction in notifications)
                {
                    Logger.Log("Processing notification action", message, tenant, LogLevel.Debug);

                    if (!notificationAction.EventPropertyTypeId.HasValue)
                    {
                        continue;
                    }

                    if (!instancePropertyTypeIds.ContainsKey(notificationAction.EventPropertyTypeId.Value)
                        || instancePropertyTypeIds[notificationAction.EventPropertyTypeId.Value].IsEmpty())
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
                        return await Task.FromResult(true);
                    }

                    var artifact = updatedArtifacts.First(a => a.Id == artifactId);

                    string messageHeader = I18NHelper.FormatInvariant("You are being notified because artifact with Id: {0} has been updated.", artifactId);
                    var artifactPartUrl = artifact.Url ?? ServerUriHelper.GetArtifactUrl(artifactId, true);
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
                        MessageTemplate = notificationAction.Message,
                        RevisionId = message.RevisionId,
                        UserId = message.UserId,
                        ArtifactTypeId = currentStateInfo.ItemTypeId,
                        ArtifactId = artifactId,
                        ArtifactUrl = artifactPartUrl,
                        ArtifactTypePredefined = artifact.Predefined,
                        ProjectId = artifact.ProjectId
                    };

                    await WorkflowEventsMessagesHelper.ProcessMessages(LogSource,
                    tenant.TenantId,
                    serviceLogRepository,
                    new List<IWorkflowMessage> { notificationMessage },
                    $"Error on new artifact creation with Id: {artifactId}",
                    WorkflowMessagingProcessor.Instance);
                }
            }
            return false;
        }
    }
}
