using BluePrintSys.Messaging.Models.Action;

namespace ActionHandlerService.MessageHandlers
{
    public class GenerateUserStoriesMessageHandler : BaseMessageHandler<GenerateUserStoriesMessage>
    {
        public GenerateUserStoriesMessageHandler() : this(new GenerateUserStoriesActionHelper())
        {
        }

        public GenerateUserStoriesMessageHandler(IActionHelper actionHelper) : base(actionHelper)
        {
        }

        protected override MessageActionType ActionType { get; } = MessageActionType.GenerateUserStories;
    }
}
