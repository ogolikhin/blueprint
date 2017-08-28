using ActionHandlerService.Helpers;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.Models.Actions;

namespace ActionHandlerService.MessageHandlers.GenerateUserStories
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
