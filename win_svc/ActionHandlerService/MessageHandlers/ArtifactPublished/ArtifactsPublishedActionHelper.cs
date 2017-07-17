using System.Linq;
using ActionHandlerService.Helpers;
using ActionHandlerService.Models;
using ActionHandlerService.Repositories;
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

        public bool HandleAction(TenantInformation tenantInformation, ActionMessage actionMessage)
        {
            var message = (ArtifactsPublishedMessage)actionMessage;
            var repository = new ActionHandlerServiceRepository(tenantInformation.ConnectionString);
            var publishedArtifacts = message.Artifacts ?? new PublishedArtifactInformation[] { };
            var publishedArtifactIds = publishedArtifacts.Select(a => a.ArtifactId).ToList();
            repository.GetWorkflowStatesForArtifacts(message.UserId, publishedArtifactIds, message.RevisionId);
            var artifactPropertyTriggers = repository.GetWorkflowTriggersForArtifacts(message.UserId, message.RevisionId, (int) TransitionType.Property, publishedArtifactIds).ToList();
            var artifactsWithTriggers = publishedArtifacts.Where(a => artifactPropertyTriggers.Any(t => t.HolderId == a.ArtifactId)).ToList();
            foreach (var artifact in artifactsWithTriggers)
            {
                var modifiedProperties = repository.GetPropertyModificationsForRevisionId(message.RevisionId);
                var artifactChangedProperties = modifiedProperties.Select(p => new ModifiedPropertyInformation { PropertyId = p.TypeId, PredefinedTypeId = p.Type, PropertyName = p.PropertyName }).ToArray();

                //TODO: get the Actions from the database
                var notifications = _actionsParser.GetNotificationActions(string.Empty, artifactChangedProperties.Any() ? artifactChangedProperties.First().PropertyId : 0);

                //if any of the artifact's changed properties has a Notification action associated with it, then send the NotificationMessage
                var notificationsToSend = notifications.Where(n => artifactChangedProperties.Any(p => p.PropertyId == n.PropertyId));
                foreach (var notification in notificationsToSend)
                {
                    NServiceBusServer.Instance.Send(
                        tenantInformation.Id,
                        new NotificationMessage
                        {
                            ArtifactId = artifact.ArtifactId,
                            RevisionId = message.RevisionId,
                            ArtifactTypeId = artifact.ArtifactTypeId,
                            ArtifactTypePredefined = artifact.ArtifactTypePredefined,
                            UserId = message.UserId,
                            ModifiedPropertiesInformation = artifactChangedProperties,
                            ToEmail = notification.ToEmail,
                            ArtifactUrl = artifact.ArtifactUrl,
                            MessageTemplate = notification.MessageTemplate
                        });
                }
            }
            return true;
        }
    }

    public class NotificationAction
    {
        public int? PropertyId { get; set; }
        public string ToEmail { get; set; }
        public string MessageTemplate { get; set; }
    }
}
