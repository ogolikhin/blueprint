using ServiceLibrary.Models.Enums;

namespace BluePrintSys.Messaging.Models.Actions
{
    public class GenerateTestsMessage : ActionMessage
    {
        public override MessageActionType ActionType { get; } = MessageActionType.GenerateTests;

        public int ArtifactId { get; set; }

        public int RevisionId { get; set; }
    }
}
