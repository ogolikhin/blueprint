using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.Models.Actions;

namespace BlueprintSys.RC.Services.MessageHandlers.GenerateUserStories
{
    public class GenerateUserStoriesMessageHandler : BaseMessageHandler<GenerateUserStoriesMessage>
    {
        public GenerateUserStoriesMessageHandler() : this(new GenerateUserStoriesActionHelper(), new TenantInfoRetriever(), new ConfigHelper())
        {
        }

        public GenerateUserStoriesMessageHandler(IActionHelper actionHelper, ITenantInfoRetriever tenantInfoRetriever, IConfigHelper configHelper) : base(actionHelper, tenantInfoRetriever, configHelper)
        {
        }
    }
}
