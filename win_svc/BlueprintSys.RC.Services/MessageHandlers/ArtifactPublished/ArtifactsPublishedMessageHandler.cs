using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.Models.Actions;

namespace BlueprintSys.RC.Services.MessageHandlers.ArtifactPublished
{
    public class ArtifactsPublishedMessageHandler : BaseMessageHandler<ArtifactsPublishedMessage>
    {
        public ArtifactsPublishedMessageHandler() : this(new ArtifactsPublishedActionHelper(), new TenantInfoRetriever(), new ConfigHelper())
        {
        }

        public ArtifactsPublishedMessageHandler(IActionHelper actionHelper, 
            ITenantInfoRetriever tenantInfoRetriever, 
            IConfigHelper configHelper) : base(actionHelper, tenantInfoRetriever, configHelper)
        {
        }
    }
}
