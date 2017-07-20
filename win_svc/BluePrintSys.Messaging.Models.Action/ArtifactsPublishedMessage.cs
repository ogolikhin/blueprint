using NServiceBus;

namespace BluePrintSys.Messaging.Models.Actions
{
    [Express]
    public class ArtifactsPublishedMessage : ActionMessage
    {
        public ArtifactsPublishedMessage()
        {
        }

        public ArtifactsPublishedMessage(int tenantId) : base(tenantId)
        {
        }

        public override MessageActionType ActionType { get; } = MessageActionType.ArtifactsPublished;

        public PublishedArtifact[] PublishedArtifacts { get; set; }
    }

    public class PublishedArtifact
    {
        public int UserId { get; set; }
        public int RevisionId { get; set; }
        public int ArtifactId { get; set; }
        public int ArtifactTypeId { get; set; }
        public int PredefinedArtifactTypeId { get; set; }
    }
}
