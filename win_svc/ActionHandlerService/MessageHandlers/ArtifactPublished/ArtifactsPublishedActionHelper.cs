using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActionHandlerService.Helpers;
using ActionHandlerService.Models;
using ActionHandlerService.Repositories;
using ArtifactStore.Helpers;
using BluePrintSys.Messaging.CrossCutting.Logging;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Models.Enums;

namespace ActionHandlerService.MessageHandlers.ArtifactPublished
{
    public class ArtifactsPublishedActionHelper : IActionHelper
    {
        private readonly IActionsParser _actionsParser;
        private readonly INServiceBusServer _nServiceBusServer;

        public ArtifactsPublishedActionHelper(IActionsParser actionsParser = null, INServiceBusServer nServiceBusServer = null)
        {
            _actionsParser = actionsParser ?? new ActionsParser();
            _nServiceBusServer = nServiceBusServer ?? NServiceBusServer.Instance;
        }

        private void LogInfo(string text, ArtifactsPublishedMessage message, TenantInformation tenant)
        {
            Log.Info($"{text} Message: {message.ActionType.ToString()}. Tenant ID: {tenant.Id}");
        }

        public async Task<bool> HandleAction(TenantInformation tenantInformation, ActionMessage actionMessage, IActionHandlerServiceRepository repository)
        {
            var message = (ArtifactsPublishedMessage) actionMessage;
            var publishedArtifacts = message.Artifacts ?? new PublishedArtifactInformation[] { };
            var publishedArtifactIds = publishedArtifacts.Select(a => a.Id).ToHashSet();
            LogInfo($"Handling started for user ID {message.UserId}, revision ID {message.RevisionId}, artifact IDs {string.Join(", ", publishedArtifactIds)}", message, tenantInformation);

            //Get property transitions for published artifact ids.
            //TODO: check whether item type id can be relied upon or not for performance reasons
            var artifactPropertyEvents = await GetPropertyEventsInformationForArtifactIds(repository, message, publishedArtifactIds);

            //if no property transitions found, then call does not need to proceed 
            if (artifactPropertyEvents == null || artifactPropertyEvents.Count == 0)
            {
                LogInfo("No property events found.", message, tenantInformation);
                return await Task.FromResult(true);
            }

            //convert all property transitions to a dictionary with artifact id as key
            var activePropertyTransitions = BuildArtifactPropertyTransitions(artifactPropertyEvents, publishedArtifactIds);

            //Get modified properties for all artifacts and create a dictionary with key as artifact ids
            //TODO: Modify this to take in a list of artifact ids to get property modifications for impacted artifacts
            var modifiedProperties = publishedArtifacts.ToDictionary(k => k.Id, v => v.ModifiedProperties ?? new List<PublishedPropertyInformation>());
            if (modifiedProperties.Count == 0)
            {
                LogInfo("No modified properties found.", message, tenantInformation);
                return await Task.FromResult(true);
            }

            var workflowStates = (await repository.GetWorkflowStatesForArtifactsAsync(message.UserId, activePropertyTransitions.Keys, message.RevisionId)).Where(w => w.WorkflowStateId > 0).ToDictionary(k => k.ArtifactId);

            if (workflowStates.Count == 0)
            {
                LogInfo("No workflow states found.", message, tenantInformation);
                return await Task.FromResult(true);
            }

            //for artifacts in active property transitions
            foreach (var artifactId in activePropertyTransitions.Keys)
            {
                var artifactTransitionInfo = activePropertyTransitions[artifactId];

                //TODO: get the Actions from the triggers that were retrieved from the database
                var notifications = _actionsParser.GetNotificationActions(artifactTransitionInfo).ToList();
                if (notifications.Count == 0)
                {
                    continue;
                }

                var artifactModifiedProperties = modifiedProperties[artifactId];
                if (artifactModifiedProperties == null || !artifactModifiedProperties.Any())
                {
                    continue;
                }

                var artifactModifiedPropertiesSet = artifactModifiedProperties.Select(a => a.TypeId).ToHashSet();

                var currentStateInfo = workflowStates[artifactId];

                foreach (var notificationActionToProcess in notifications)
                {
                    if (!artifactModifiedPropertiesSet.Contains(notificationActionToProcess.PropertyTypeId))
                    {
                        continue;
                    }
                    //if conditional state id should be present and either the current state info is not present or the current state is not same as conditional state
                    if (notificationActionToProcess.ConditionalStateId.HasValue && (currentStateInfo == null || currentStateInfo.WorkflowStateId != notificationActionToProcess.ConditionalStateId.Value))
                    {
                        LogInfo("Workflow state ID does not match conditional state ID.", message, tenantInformation);
                        return await Task.FromResult(true);
                    }
                    _nServiceBusServer.Send(tenantInformation.Id, new NotificationMessage
                    {
                        ArtifactId = artifactId,
                        RevisionId = message.RevisionId,
                        ArtifactTypeId = currentStateInfo.ItemTypeId,
                        //ArtifactTypePredefined = currentStateInfo. artifact.ArtifactTypePredefined,
                        UserId = message.UserId,
                        ModifiedPropertiesInformation = artifactModifiedProperties.Select(a => new ModifiedPropertyInformation
                        {
                            PropertyId = a.TypeId,
                            PredefinedTypeId = a.PredefinedType,
                            //PropertyName = a.PropertyName
                        }).ToArray(),
                        ToEmail = notificationActionToProcess.ToEmail,
                        //ArtifactUrl = artifact.ArtifactUrl,
                        MessageTemplate = notificationActionToProcess.MessageTemplate
                    });
                }
            }
            LogInfo("Action handling complete.", message, tenantInformation);
            return true;
        }

        private Dictionary<int, IList<SqlArtifactTriggers>> BuildArtifactPropertyTransitions(IList<SqlArtifactTriggers> artifactPropertyEvents, HashSet<int> publishedArtifactIds)
        {
            var activePropertyTransitions = new Dictionary<int, IList<SqlArtifactTriggers>>();
            foreach (var artifactPropertyEvent in artifactPropertyEvents.Where(ape => publishedArtifactIds.Contains(ape.VersionItemId)))
            {
                if (activePropertyTransitions.ContainsKey(artifactPropertyEvent.VersionItemId))
                {
                    activePropertyTransitions[artifactPropertyEvent.VersionItemId].Add(artifactPropertyEvent);
                }
                else
                {
                    activePropertyTransitions.Add(artifactPropertyEvent.VersionItemId, new List<SqlArtifactTriggers> {artifactPropertyEvent});
                }
            }
            return activePropertyTransitions;
        }

        private async Task<IList<SqlArtifactTriggers>> GetPropertyEventsInformationForArtifactIds(IActionHandlerServiceRepository repository, ArtifactsPublishedMessage message, HashSet<int> publishedArtifactIds)
        {
            return await repository.GetWorkflowPropertyTransitionsForArtifactsAsync(message.UserId, message.RevisionId, (int) TransitionType.Property, publishedArtifactIds);
        }
    }

    //TODO: Use model created for xml import
    public class NotificationAction
    {
        public int PropertyTypeId { get; set; }
        public string ToEmail { get; set; }
        public string MessageTemplate { get; set; }
        public int? ConditionalStateId { get; set; }
    }
}
