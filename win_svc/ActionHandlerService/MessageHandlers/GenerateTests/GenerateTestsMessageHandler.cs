using BluePrintSys.Messaging.Models.Action;

namespace ActionHandlerService.MessageHandlers.GenerateTests
{
    public class GenerateTestsMessageHandler : BaseMessageHandler<GenerateTestsMessage>
    {
        public GenerateTestsMessageHandler() : this(new GenerateTestsActionHelper())
        {
        }

        public GenerateTestsMessageHandler(IActionHelper actionHelper) : base(actionHelper)
        {
        }

        protected override MessageActionType ActionType { get; } = MessageActionType.GenerateTests;
    }
}
