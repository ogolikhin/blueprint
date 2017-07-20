using BluePrintSys.Messaging.Models.Actions;

namespace ActionHandlerService.MessageHandlers.GenerateUserStories
{
    public class GenerateUserStoriesMessageHandler : BaseMessageHandler<GenerateUserStoriesMessage>
    {
        public GenerateUserStoriesMessageHandler() : this(new GenerateUserStoriesActionHelper())
        {
        }

        public GenerateUserStoriesMessageHandler(IActionHelper actionHelper) : base(actionHelper)
        {
        }
    }
}
