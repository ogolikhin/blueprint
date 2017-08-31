using ServiceLibrary.Models.Enums;

namespace BluePrintSys.Messaging.Models.Actions
{
    public class GenerateUserStoriesMessage : ProjectContainerActionMessage
    {
        public override MessageActionType ActionType { get; } = MessageActionType.GenerateUserStories;
        public int ArtifactId { get; set; }

        public int RevisionId { get; set; }

        public string ProjectName { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; }

        public string BaseHostUri { get; set; }
    }
}
