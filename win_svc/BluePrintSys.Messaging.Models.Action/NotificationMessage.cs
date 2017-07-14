using NServiceBus;

namespace BluePrintSys.Messaging.Models.Actions
{
    [Express]
    public class NotificationMessage : ActionMessage
    {
        public NotificationMessage()
        {
        }

        public NotificationMessage(int tenantId) : base(tenantId)
        {
        }

        public override MessageActionType ActionType { get; } = MessageActionType.Notification;
        public string ToEmail { get; set; }
        public string MessageTemplate { get; set; }
        public int ArtifactId { get; set; }
        public string ArtifactUrl { get; set; }
        public int RevisionId { get; set; }
        public int ArtifactTypeId { get; set; }
        public int PredefinedArtifactTypeId { get; set; }
        public int UserId { get; set; }
        public ChangedProperty[] ChangedProperties { get; set; }
    }

    public class ChangedProperty
    {
        public int PropertyId { get; set; }
        public string PropertyName { get; set; }
        public int PredefinedTypeId { get; set; }
    }
}
