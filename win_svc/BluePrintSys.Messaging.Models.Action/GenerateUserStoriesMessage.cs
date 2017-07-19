using NServiceBus;

namespace BluePrintSys.Messaging.Models.Actions
{
    [Express]
    public class GenerateUserStoriesMessage : ActionMessage
    {
        public GenerateUserStoriesMessage()
        {
        }

        public GenerateUserStoriesMessage(int tenantId) : base(tenantId)
        {
        }

        public override MessageActionType ActionType { get; } = MessageActionType.GenerateUserStories;
    }
}
