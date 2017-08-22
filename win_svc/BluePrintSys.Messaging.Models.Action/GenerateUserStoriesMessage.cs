namespace BluePrintSys.Messaging.Models.Actions
{
    public class GenerateUserStoriesMessage : ActionMessage
    {
        public override MessageActionType ActionType { get; } = MessageActionType.GenerateUserStories;

        public int ProjectId { get; set; }

        public int ProcessId { get; set; }

        public int? TaskId { get; set; }
    }
}
