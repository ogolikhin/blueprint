using ServiceLibrary.Models.Enums;

namespace BluePrintSys.Messaging.Models.Actions
{
    public class GenerateUserStoriesMessage : ActionMessage
    {
        public override MessageActionType ActionType { get; } = MessageActionType.GenerateUserStories;

        public int ArtifactId { get; set; }

        public int RevisionId { get; set; }
    }
}
