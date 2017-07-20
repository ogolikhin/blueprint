using BluePrintSys.Messaging.Models.Actions;

namespace ActionHandlerService.MessageHandlers.GenerateDescendants
{
    public class GenerateDescendantsMessageHandler : BaseMessageHandler<GenerateDescendantsMessage>
    {
        public GenerateDescendantsMessageHandler() : this(new GenerateDescendantsActionHelper())
        {
        }

        public GenerateDescendantsMessageHandler(IActionHelper actionHelper) : base(actionHelper)
        {
        }
    }
}
