using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.Models.Actions;

namespace BlueprintSys.RC.Services.MessageHandlers.PropertyChange
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
