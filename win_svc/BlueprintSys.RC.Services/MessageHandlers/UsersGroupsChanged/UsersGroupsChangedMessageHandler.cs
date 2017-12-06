using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.Models.Actions;

namespace BlueprintSys.RC.Services.MessageHandlers.UsersGroupsChanged
{
    public class UsersGroupsChangedMessageHandler : BaseMessageHandler<UsersGroupsChangedMessage>
    {
        public UsersGroupsChangedMessageHandler() : this(new UsersGroupsChangedActionHelper(), new TenantInfoRetriever(), new ConfigHelper())
        {
        }
        public UsersGroupsChangedMessageHandler(
            IActionHelper actionHelper,
            ITenantInfoRetriever tenantInfoRetriever,
            IConfigHelper configHelper)
            : base(actionHelper, tenantInfoRetriever, configHelper)
        {
        }
    }
}
