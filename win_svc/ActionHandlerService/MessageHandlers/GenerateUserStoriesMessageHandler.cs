using BluePrintSys.ActionMessaging.Models;

namespace ActionHandlerService.MessageHandlers
{
    public class GenerateUserStoriesMessageHandler : BaseMessageHandler<GenerateUserStoriesMessage>
    {
        protected override MessageActionType ActionType { get; } = MessageActionType.GenerateUserStories;

        protected override IActionHelper ActionHelper { get; } = new GenerateUserStoriesActionHelper();
    }
}
