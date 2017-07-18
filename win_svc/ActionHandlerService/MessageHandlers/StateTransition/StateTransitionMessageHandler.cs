using BluePrintSys.Messaging.Models.Actions;

namespace ActionHandlerService.MessageHandlers.StateTransition
{
    public class StateTransitionMessageHandler : BaseMessageHandler<GenerateUserStoriesMessage>
    {
        public StateTransitionMessageHandler() : this(new StateTransitionMessageHelper())
        {
        }

        public StateTransitionMessageHandler(IActionHelper actionHelper) : base(actionHelper)
        {
        }

        protected override MessageActionType ActionType { get; } = MessageActionType.StateChange;
    }
}
