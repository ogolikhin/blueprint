using System.Collections.Generic;
using System.Linq;
using ServiceLibrary.Models.Enums;

namespace BluePrintSys.Messaging.Models.Actions
{
    public enum ArtifactChangedType
    {
        Save,
        Discard,
        Publish,
        Move,
        Indirect
    }

    public class ArtifactsChangedMessage : ActionMessage
    {
        public ArtifactsChangedMessage()
        {
            ArtifactIds = new List<int>();
        }

        public ArtifactsChangedMessage(IEnumerable<int> artifactIds)
        {
            // using a List to allow deserialization
            ArtifactIds = artifactIds.ToList();
        }

        public override MessageActionType ActionType { get; } = MessageActionType.ArtifactsChanged;

        public ArtifactChangedType ChangeType { get; set; }

        public IEnumerable<int> ArtifactIds { get; set; }
    }
}
