using ActionHandlerService.Helpers;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.Models.Actions;

namespace ActionHandlerService.MessageHandlers.PropertyChange
{
    public class PropertyChangeMessageHandler : BaseMessageHandler<GenerateUserStoriesMessage>
    {
        public PropertyChangeMessageHandler() : this(new PropertyChangeActionHelper(), new TenantInfoRetriever(), new ConfigHelper())
        {
        }

        public PropertyChangeMessageHandler(IActionHelper actionHelper, ITenantInfoRetriever tenantInfoRetriever, IConfigHelper configHelper) : base(actionHelper, tenantInfoRetriever, configHelper)
        {
        }
    }
}
