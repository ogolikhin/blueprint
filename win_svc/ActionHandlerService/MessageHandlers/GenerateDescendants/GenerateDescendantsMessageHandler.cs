using BluePrintSys.Messaging.Models.Action;

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
        
        protected override MessageActionType ActionType { get; } = MessageActionType.GenerateDescendants;
    }
}
