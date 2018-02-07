using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.Models.Actions;

namespace BlueprintSys.RC.Services.MessageHandlers.ArtifactsChanged
{
    public class ArtifactsChangedMessageHandler : BaseMessageHandler<ArtifactsChangedMessage>
    {
        public ArtifactsChangedMessageHandler() : this(new ArtifactsChangedActionHelper(), new TenantInfoRetriever(), new ConfigHelper(), new TransactionValidator())
        {
        }

        public ArtifactsChangedMessageHandler(IActionHelper actionHelper, ITenantInfoRetriever tenantInfoRetriever, IConfigHelper configHelper, ITransactionValidator transactionValidator) : base(actionHelper, tenantInfoRetriever, configHelper, transactionValidator)
        {
        }
    }
}
