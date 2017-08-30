using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.Models.Actions;

namespace BlueprintSys.RC.Services.MessageHandlers.GenerateDescendants
{
    public class GenerateDescendantsMessageHandler : BaseMessageHandler<GenerateDescendantsMessage>
    {
        public GenerateDescendantsMessageHandler() : this(new GenerateDescendantsActionHelper(), new TenantInfoRetriever(), new ConfigHelper())
        {
        }

        public GenerateDescendantsMessageHandler(IActionHelper actionHelper, ITenantInfoRetriever tenantInfoRetriever, IConfigHelper configHelper) : base(actionHelper, tenantInfoRetriever, configHelper)
        {
        }
    }
}
