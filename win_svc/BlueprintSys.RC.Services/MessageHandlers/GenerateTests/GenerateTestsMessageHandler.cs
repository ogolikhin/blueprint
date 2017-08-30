using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.Models.Actions;

namespace BlueprintSys.RC.Services.MessageHandlers.GenerateTests
{
    public class GenerateTestsMessageHandler : BaseMessageHandler<GenerateTestsMessage>
    {
        public GenerateTestsMessageHandler() : this(
            new GenerateTestsActionHelper(), 
            new TenantInfoRetriever(), 
            new ConfigHelper())
        {
        }

        public GenerateTestsMessageHandler(IActionHelper actionHelper, ITenantInfoRetriever tenantInfoRetriever, IConfigHelper configHelper) : base(actionHelper, tenantInfoRetriever, configHelper)
        {
        }
    }
}
