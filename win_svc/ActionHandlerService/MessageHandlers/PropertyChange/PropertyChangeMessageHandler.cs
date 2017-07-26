using BluePrintSys.Messaging.Models.Actions;

namespace ActionHandlerService.MessageHandlers.PropertyChange
{
    public class PropertyChangeMessageHandler : BaseMessageHandler<GenerateUserStoriesMessage>
    {
        public PropertyChangeMessageHandler() : this(new PropertyChangeActionHelper())
        {
        }

        public PropertyChangeMessageHandler(IActionHelper actionHelper) : base(actionHelper)
        {
        }
    }
}
