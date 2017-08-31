using ServiceLibrary.Models.Enums;

namespace BluePrintSys.Messaging.Models.Actions
{
    public class GenerateDescendantsMessage : ProjectContainerActionMessage
    {
        public override MessageActionType ActionType { get; } = MessageActionType.GenerateChildren;

        public int ChildCount { get; set; } = 10;

        public int? DesiredArtifactTypeId { get; set; }

        public int RevisionId { get; set; }

        public int ArtifactId { get; set; }

        public int TypePredefined { get; set; }

        public int ProjectId { get; set; }

        public string ProjectName { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; }

        public string BaseHostUri { get; set; }
    }
}
