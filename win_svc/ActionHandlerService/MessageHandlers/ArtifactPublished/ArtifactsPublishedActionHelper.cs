using System.Linq;
using ActionHandlerService.Models;
using BluePrintSys.Messaging.Models.Actions;

namespace ActionHandlerService.MessageHandlers.ArtifactPublished
{
    public class ArtifactsPublishedActionHelper : IActionHelper
    {
        public bool HandleAction(TenantInformation tenantInformation, ActionMessage actionMessage)
        {
            var message = (ArtifactsPublishedMessage)actionMessage;
            var repository = new ActionHandlerServiceRepository(tenantInformation.ConnectionString);
            var publishedArtifacts = message.Artifacts ?? new PublishedArtifactInformation[] { };
            foreach (var artifact in publishedArtifacts)
            {
                var modifiedProperties = repository.GetPropertyModificationsForRevisionId(message.RevisionId);
                var artifactChangedProperties = modifiedProperties.Select(p => new ChangedProperty { PropertyId = p.TypeId, PredefinedTypeId = p.Type, PropertyName = p.PropertyName }).ToArray();

                repository.GetWorkflowStatesForArtifacts(message.UserId, new[] {artifact.ArtifactId}, message.RevisionId);
                //TODO: get the Actions from the database
                var notifications = new[] {new NotificationAction {PropertyId = artifactChangedProperties.Any() ? artifactChangedProperties.First().PropertyId : 0}};

                //if any of the artifact's changed properties has a Notification action associated with it, then send the NotificationMessage
                var notificationsToSend = notifications.Where(n => artifactChangedProperties.Any(p => p.PropertyId == n.PropertyId));
                foreach (var notification in notificationsToSend)
                {
                    NServiceBusServer.Instance.Send(
                        tenantInformation.Id.ToString(),
                        new NotificationMessage
                        {
                            ArtifactId = artifact.ArtifactId,
                            RevisionId = message.RevisionId,
                            ArtifactTypeId = artifact.ArtifactTypeId,
                            ArtifactTypePredefined = artifact.ArtifactTypePredefined,
                            UserId = message.UserId,
                            ChangedProperties = artifactChangedProperties,
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
