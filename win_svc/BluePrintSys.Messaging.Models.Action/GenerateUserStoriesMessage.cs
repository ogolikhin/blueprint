namespace BluePrintSys.Messaging.Models.Actions
{
    public class GenerateUserStoriesMessage : ActionMessage
    {
        public override MessageActionType ActionType { get; } = MessageActionType.GenerateUserStories;
    }
}
