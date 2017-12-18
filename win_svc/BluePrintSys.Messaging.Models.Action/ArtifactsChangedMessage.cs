using System.Collections.Generic;
using ServiceLibrary.Models.Enums;

namespace BluePrintSys.Messaging.Models.Actions
{
    public enum ChangedType
    {
        Save,
        Discard,
        Published
    }

    public class ArtifactsChangedMessage : ActionMessage
    {
        public override MessageActionType ActionType { get; } = MessageActionType.ArtifactsChanged;

        public ChangedType ChangeType { get; set; }

        public IEnumerable<int> ArtifactIds { get; set; }
    }
}
