using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.Models.Actions;

namespace BlueprintSys.RC.Services.MessageHandlers.PropertyItemTypesChanged
{
    public class PropertyItemTypesChangedMessageHandler : BaseMessageHandler<PropertyItemTypeChangedMessage>
    {
        public PropertyItemTypesChangedMessageHandler() : this(new PropertyItemTypesChangedActionHelper(), new TenantInfoRetriever(), new ConfigHelper())
        {
        }
        public PropertyItemTypesChangedMessageHandler(
            IActionHelper actionHelper,
            ITenantInfoRetriever tenantInfoRetriever,
            IConfigHelper configHelper)
            : base(actionHelper, tenantInfoRetriever, configHelper)
        {
        }
    }
}
