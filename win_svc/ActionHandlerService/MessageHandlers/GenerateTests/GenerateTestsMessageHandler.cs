using BluePrintSys.Messaging.Models.Actions;

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
    }
}
