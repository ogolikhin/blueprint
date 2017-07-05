using BluePrintSys.ActionMessaging.Models;

namespace ActionHandlerService.MessageHandlers
{
    public class GenerateTestsMessageHandler : BaseMessageHandler<GenerateTestsMessage>
    {
        protected override MessageActionType ActionType { get; } = MessageActionType.GenerateTests;

        protected override IActionHelper ActionHelper { get; } = new GenerateTestsActionHelper();
    }
}
