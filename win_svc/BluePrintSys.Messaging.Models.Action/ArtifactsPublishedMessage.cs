using System.Collections.Generic;
using ServiceLibrary.Models.Enums;

namespace BluePrintSys.Messaging.Models.Actions
{
    public class ArtifactsPublishedMessage : ActionMessage
    {
        public override MessageActionType ActionType { get; } = MessageActionType.ArtifactsPublished;

        public int UserId { get; set; }

        public int RevisionId { get; set; }

        public ICollection<PublishedArtifactInformation> Artifacts { get; set; }
        public string UserName { get; set; }
    }
}
