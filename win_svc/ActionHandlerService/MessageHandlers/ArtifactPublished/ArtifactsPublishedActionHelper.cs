using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActionHandlerService.Helpers;
using ActionHandlerService.Models;
using ActionHandlerService.Repositories;
using ArtifactStore.Helpers;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Models.Enums;

namespace ActionHandlerService.MessageHandlers.ArtifactPublished
{
    public class ArtifactsPublishedActionHelper : IActionHelper
    {
        private readonly IActionsParser _actionsParser;
        public ArtifactsPublishedActionHelper() : this(new ActionsParser())
        {
        }

        public ArtifactsPublishedActionHelper(IActionsParser actionsParser)
        {
            _actionsParser = actionsParser;
        }

        public async Task<bool> HandleAction(TenantInformation tenantInformation, ActionMessage actionMessage)
        {
            var message = (ArtifactsPublishedMessage)actionMessage;
            var repository = new ActionHandlerServiceRepository(tenantInformation.ConnectionString);
            var publishedArtifacts = message.Artifacts ?? new PublishedArtifactInformation[] { };
            var publishedArtifactIds = publishedArtifacts.Select(a => a.ArtifactId).ToHashSet();
            
            //Get property transitions for published artifact ids.
            //TODO: check whether item type id can be relied upon or not for performance reasons
            var artifactPropertyTriggers = await repository.GetWorkflowPropertyTransitionsForArtifactsAsync(message.UserId, 
                message.RevisionId, 
                (int) TransitionType.Property, 
                publishedArtifactIds);

            //if no property transitions found, then call does not need to proceed 
            if (artifactPropertyTriggers == null || artifactPropertyTriggers.Count == 0)
            {
                return await Task.FromResult(true);
            }

            //convert all property transitions to a dictionary with artifact id as key
            var activePropertyTransitions = new Dictionary<int, IList<SqlArtifactTriggers>>();
            foreach (var artifactPropertyTrigger in artifactPropertyTriggers.Where(artifactPropertyTrigger => publishedArtifactIds.Contains(artifactPropertyTrigger.HolderId)))
            {
                if (activePropertyTransitions.ContainsKey(artifactPropertyTrigger.HolderId))
                {
                    activePropertyTransitions[artifactPropertyTrigger.HolderId].Add(artifactPropertyTrigger);
                }
                else
                {
                    activePropertyTransitions.Add(artifactPropertyTrigger.HolderId, new List<SqlArtifactTriggers>
                    {
                        artifactPropertyTrigger
                    });
                }
            }

            //Get modified properties for all artifacts and create a dictionary with key as artifact ids
            //TODO: Modify this to take in a list of artifact ids to get property modifications for impacted artifacts
            var modifiedProperties = (await repository.GetPropertyModificationsForRevisionIdAsync(message.RevisionId))
                .GroupBy(a => a.ArtifactId)
                .ToDictionary(a => a.Key, v => v.Select(a => a).ToList());
            if (modifiedProperties.Count == 0)
            {
                return await Task.FromResult(true);
            }

            var workflowStates =
                (await
                    repository.GetWorkflowStatesForArtifactsAsync(message.UserId, activePropertyTransitions.Keys,
                        message.RevisionId)).ToDictionary(k => k.ArtifactId);

            if (workflowStates.Count == 0)
            {
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
                    if (notificationActionToProcess.ConditionalStateId.HasValue &&
                        (currentStateInfo == null ||
                         currentStateInfo.WorkflowStateId != notificationActionToProcess.ConditionalStateId.Value))
                    {
                        return await Task.FromResult(true);
                    }
                    NServiceBusServer.Instance.Send(
                        tenantInformation.Id,
                        new NotificationMessage
                        {
                            ArtifactId = artifactId,
                            RevisionId = message.RevisionId,
                            ArtifactTypeId =  currentStateInfo.ItemTypeId,
                            //ArtifactTypePredefined = currentStateInfo. artifact.ArtifactTypePredefined,
                            UserId = message.UserId,
                            ModifiedPropertiesInformation = artifactModifiedProperties.Select(a => new ModifiedPropertyInformation()
                            {
                                PropertyId = a.TypeId,
                                PredefinedTypeId = a.Type,
                                PropertyName = a.PropertyName
                            }).ToArray(),
                            ToEmail = notificationActionToProcess.ToEmail,
                            //ArtifactUrl = artifact.ArtifactUrl,
                            MessageTemplate = notificationActionToProcess.MessageTemplate
                        });
                }
            }
            return true;
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
