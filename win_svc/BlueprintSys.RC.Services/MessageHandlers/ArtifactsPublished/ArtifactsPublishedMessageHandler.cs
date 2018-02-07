using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.Models.Actions;

namespace BlueprintSys.RC.Services.MessageHandlers.ArtifactsPublished
{
    public class ArtifactsPublishedMessageHandler : BaseMessageHandler<ArtifactsPublishedMessage>
    {
        public ArtifactsPublishedMessageHandler() : this(new ArtifactsPublishedActionHelper(), new TenantInfoRetriever(), new ConfigHelper(), new TransactionValidator())
        {
        }

        public ArtifactsPublishedMessageHandler(IActionHelper actionHelper, ITenantInfoRetriever tenantInfoRetriever, IConfigHelper configHelper, ITransactionValidator transactionValidator) : base(actionHelper, tenantInfoRetriever, configHelper, transactionValidator)
        {
        }
    }
}
