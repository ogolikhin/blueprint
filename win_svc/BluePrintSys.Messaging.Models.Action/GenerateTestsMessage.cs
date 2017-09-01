using ServiceLibrary.Models.Enums;

namespace BluePrintSys.Messaging.Models.Actions
{
    public class GenerateTestsMessage : ActionMessage
    {
        public override MessageActionType ActionType { get; } = MessageActionType.GenerateTests;

        public int ArtifactId { get; set; }

        public int RevisionId { get; set; }

        public int ProjectId { get; set; }

        public string ProjectName { get; set; }

        public string UserName { get; set; }

        public string BaseHostUri { get; set; }
    }
}
