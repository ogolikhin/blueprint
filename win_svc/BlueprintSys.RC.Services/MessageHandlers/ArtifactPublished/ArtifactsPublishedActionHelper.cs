using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BlueprintSys.RC.Services.Models;
using BlueprintSys.RC.Services.Repositories;
using BluePrintSys.Messaging.CrossCutting.Host;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Helpers;
using ServiceLibrary.LocalLog;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ApplicationSettings;
using ServiceLibrary.Repositories.ConfigControl;

namespace BlueprintSys.RC.Services.MessageHandlers.ArtifactPublished
{
    public class ArtifactsPublishedActionHelper : IActionHelper
    {
        private readonly IActionsParser _actionsParser;
        private readonly INServiceBusServer _nServiceBusServer;
        private const string LogSource = "ArtifactsPublishedActionHelper.CreatedArtifacts";

        public ArtifactsPublishedActionHelper(IActionsParser actionsParser = null, INServiceBusServer nServiceBusServer = null)
        {
            _actionsParser = actionsParser ?? new ActionsParser();
            _nServiceBusServer = nServiceBusServer ?? WorkflowServiceBusServer.Instance;
        }

        public async Task<bool> HandleAction(TenantInformation tenant, ActionMessage actionMessage, IActionHandlerServiceRepository actionHandlerServiceRepository)
        {
            var message = (ArtifactsPublishedMessage)actionMessage;
            Logger.Log($"Handling started for user ID {message.UserId}, revision ID {message.RevisionId} with message {message.ToJSON()}", message, tenant, LogLevel.Debug);
            var repository = (IArtifactsPublishedRepository)actionHandlerServiceRepository;
            var allPublishedArtifacts = message.Artifacts ?? new PublishedArtifactInformation[] { };
            var updatedArtifacts = allPublishedArtifacts.Where(p => !p.IsFirstTimePublished).ToList();

            //Get modified properties for all artifacts and create a dictionary with key as artifact ids
            //TODO: Modify this to take in a list of artifact ids to get property modifications for impacted artifacts
            if (!await ProcessUpdatedArtifacts(tenant, updatedArtifacts, message, repository))
            {
                return await Task.FromResult(false);
            }

            var createdArtifacts = allPublishedArtifacts.Where(p => p.IsFirstTimePublished).ToList();
            if (!await ProcessCreatedArtifacts(tenant, createdArtifacts, message, repository))
            {
                return await Task.FromResult(false);
            }

            Logger.Log("Finished processing message", message, tenant, LogLevel.Debug);
            return await Task.FromResult(true);
        }

        private async Task<bool> ProcessCreatedArtifacts(TenantInformation tenant, 
            List<PublishedArtifactInformation> createdArtifacts, 
            ArtifactsPublishedMessage message, 
            IArtifactsPublishedRepository repository)
        {
            if (createdArtifacts == null || createdArtifacts.Count <= 0)
            {
                return true;
            }
            var applicationSettingsRepository = new ApplicationSettingsRepository(
                new SqlConnectionWrapper(tenant.BlueprintConnectionString));

            var serviceLogRepository = new ServiceLogRepository(new HttpClientProvider(), 
                new LocalFileLog(), 
                tenant.AdminStoreLog);

            var artifactIds = createdArtifacts.Select(a => a.Id).ToHashSet();

            var artifactInfos = (await repository.GetWorkflowMessageArtifactInfoAsync(message.UserId,
                artifactIds,
                message.RevisionId)).ToDictionary(k => k.Id);

            var notFoundArtifactIds = artifactIds.Except(artifactInfos.Keys).ToHashSet();
            if (notFoundArtifactIds.Count > 0)
            {
                var notFoundArtifactIdString = string.Join(", ", notFoundArtifactIds);
                await serviceLogRepository.LogInformation(LogSource,
                    $"Could not recover information for following artifacts {notFoundArtifactIdString}");
                Logger.Log($"Could not recover information for following artifacts {notFoundArtifactIdString}", message, tenant, LogLevel.Debug);
            }

            foreach (var createdArtifact in createdArtifacts)
            {
                WorkflowMessageArtifactInfo artifactInfo;
                if (!artifactInfos.TryGetValue(createdArtifact.Id, out artifactInfo))
                {
                    await serviceLogRepository.LogInformation(LogSource,
                    $"Could not recover information for artifact Id: {createdArtifact.Id} and Name: {createdArtifact.Name} and Project Id: {createdArtifact.ProjectId}");
                    Logger.Log($"Could not recover information for artifact Id: {createdArtifact.Id} and Name: {createdArtifact.Name} and Project Id: {createdArtifact.ProjectId}",
                        message, tenant, LogLevel.Debug);
                    continue;
                }

                var eventTriggers = await repository.GetWorkflowEventTriggersForNewArtifactEvent(message.UserId,
                    new[] { createdArtifact.Id },
                    message.RevisionId);
                var actionMessages = WorkflowEventsMessagesHelper.GenerateMessages(message.UserId,
                    message.RevisionId,
                    message.UserName,
                    eventTriggers.AsynchronousTriggers,
                    artifactInfo,
                    artifactInfo.ProjectName,
                    new Dictionary<int, IList<Property>>(),
                    false
                    );

                await WorkflowEventsMessagesHelper.ProcessMessages(LogSource,
                    applicationSettingsRepository,
                    serviceLogRepository,
                    actionMessages,
                    $"Error on new artifact creation with Id: {createdArtifact.Id}");
            }
            return await Task.FromResult(true);
        }

        private async Task<bool> ProcessUpdatedArtifacts(TenantInformation tenant,
            ICollection<PublishedArtifactInformation> updatedArtifacts,
            ArtifactsPublishedMessage message,
            IArtifactsPublishedRepository repository)
        {
            var allArtifactsModifiedProperties = updatedArtifacts.ToDictionary(k => k.Id,
                v => v.ModifiedProperties ?? new List<PublishedPropertyInformation>());
            Logger.Log(
                $"{allArtifactsModifiedProperties.Count} artifacts found: {string.Join(", ", allArtifactsModifiedProperties.Select(k => k.Key))}",
                message, tenant, LogLevel.Debug);
            if (allArtifactsModifiedProperties.Count == 0)
            {
                return await Task.FromResult(true);
            }

            //Get property transitions for published artifact ids.
            //TODO: check whether item type id can be relied upon or not for performance reasons
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
                return await Task.FromResult(true);
            }

            //convert all property transitions to a dictionary with artifact id as key
            var activePropertyTransitions = new Dictionary<int, IList<SqlWorkflowEvent>>();
            var publishedArtifactEvents = artifactPropertyEvents.Where(ape => publishedArtifactIds.Contains(ape.VersionItemId));
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
                return await Task.FromResult(true);
            }

            var projectIds = updatedArtifacts.Select(a => a.ProjectId).ToList();
            var projects = await repository.GetProjectNameByIdsAsync(projectIds);
            Logger.Log($"{projects.Count} project names found for project IDs: {string.Join(", ", projectIds)}", message, tenant,
                LogLevel.Debug);

            //for artifacts in active property transitions
            foreach (var artifactId in activePropertyTransitions.Keys)
            {
                Logger.Log($"Processing artifact with ID: {artifactId}", message, tenant, LogLevel.Debug);

                var artifactTransitionInfo = activePropertyTransitions[artifactId];
                var notifications = _actionsParser.GetNotificationActions(artifactTransitionInfo).ToList();
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

                    if (!notificationAction.PropertyTypeId.HasValue)
                    {
                        continue;
                    }

                    if (!instancePropertyTypeIds.ContainsKey(notificationAction.PropertyTypeId.Value)
                        || instancePropertyTypeIds[notificationAction.PropertyTypeId.Value].IsEmpty())
                    {
                        Logger.Log(
                            $"The property type ID {notificationAction.PropertyTypeId} was not found in the dictionary of instance property type IDs.",
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
                    var notificationMessage = new NotificationMessage
                    {
                        ArtifactName = workflowStates[artifactId].Name,
                        ProjectName = projects.First(p => p.ItemId == artifact.ProjectId).Name,
                        Subject = notificationAction.Subject,
                        From = notificationAction.FromDisplayName,
                        To = notificationAction.Emails,
                        Header = notificationAction.Header,
                        MessageTemplate = notificationAction.Message,
                        RevisionId = message.RevisionId,
                        UserId = message.UserId,
                        ArtifactTypeId = currentStateInfo.ItemTypeId,
                        ArtifactId = artifactId,
                        ArtifactUrl = artifact.Url,
                        ArtifactTypePredefined = artifact.Predefined,
                        ProjectId = artifact.ProjectId
                    };
                    await _nServiceBusServer.Send(tenant.TenantId, notificationMessage);
                }
            }
            return false;
        }
    }
}
