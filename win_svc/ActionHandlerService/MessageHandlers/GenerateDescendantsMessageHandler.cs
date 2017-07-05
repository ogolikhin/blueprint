using BluePrintSys.ActionMessaging.Models;

namespace ActionHandlerService.MessageHandlers
{
    public class GenerateDescendantsMessageHandler : BaseMessageHandler<GenerateDescendantsMessage>
    {
        protected override MessageActionType ActionType { get; } = MessageActionType.GenerateDescendants;

        protected override IActionHelper ActionHelper { get; } = new GenerateDescendantsActionHelper();
    }
}
