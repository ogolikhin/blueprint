using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.Models.Actions;

namespace BlueprintSys.RC.Services.MessageHandlers.PropertyItemTypesChanged
{
    public class PropertyItemTypesChangedMessageHandler : BaseMessageHandler<PropertyItemTypesChangedMessage>
    {
        public PropertyItemTypesChangedMessageHandler() : this(new PropertyItemTypesChangedActionHelper(), new TenantInfoRetriever(), new ConfigHelper(), new TransactionValidator())
        {
        }

        public PropertyItemTypesChangedMessageHandler(IActionHelper actionHelper, ITenantInfoRetriever tenantInfoRetriever, IConfigHelper configHelper, ITransactionValidator transactionValidator) : base(actionHelper, tenantInfoRetriever, configHelper, transactionValidator)
        {
        }
    }
}
