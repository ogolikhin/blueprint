using NServiceBus;

namespace BluePrintSys.Messaging.Models.Actions
{
    [Express]
    public class GenerateUserStoriesMessage : ActionMessage
    {
        public override MessageActionType ActionType { get; } = MessageActionType.GenerateUserStories;
    }
}
