using System.Collections.Generic;
using NServiceBus;

namespace BluePrintSys.Messaging.Models.Actions
{
    [Express]
    public class ArtifactsPublishedMessage : ActionMessage
    {
        public override MessageActionType ActionType { get; }

        public int UserId { get; set; }

        public int RevisionId { get; set; }

        public ICollection<PublishedArtifactInformation> Artifacts { get; set; }
    }
}
