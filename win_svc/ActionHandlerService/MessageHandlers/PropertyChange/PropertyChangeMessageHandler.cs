using BluePrintSys.Messaging.Models.Actions;

namespace ActionHandlerService.MessageHandlers.PropertyChange
{
    public class PropertyChangeMessageHandler : BaseMessageHandler<GenerateUserStoriesMessage>
    {
        public PropertyChangeMessageHandler() : this(new PropertyChangeMessageHelper())
        {
        }

        public PropertyChangeMessageHandler(IActionHelper actionHelper) : base(actionHelper)
        {
        }

        protected override MessageActionType ActionType { get; } = MessageActionType.Property;
    }
}
