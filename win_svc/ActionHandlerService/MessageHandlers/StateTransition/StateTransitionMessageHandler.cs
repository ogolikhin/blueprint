using BluePrintSys.Messaging.Models.Actions;

namespace ActionHandlerService.MessageHandlers.StateTransition
{
    public class StateTransitionMessageHandler : BaseMessageHandler<StateChangeMessage>
    {
        public StateTransitionMessageHandler() : this(new StateTransitionActionHelper())
        {
        }

        public StateTransitionMessageHandler(IActionHelper actionHelper) : base(actionHelper)
        {
        }
    }
}
