using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.Models.Actions;

namespace BlueprintSys.RC.Services.MessageHandlers.WorkflowsChanged
{
    public class WorkflowsChangedMessageHandler : BaseMessageHandler<WorkflowsChangedMessage>
    {
        public WorkflowsChangedMessageHandler() : this(new WorkflowsChangedActionHelper(), new TenantInfoRetriever(), new ConfigHelper())
        {
        }

        public WorkflowsChangedMessageHandler(IActionHelper actionHelper, ITenantInfoRetriever tenantInfoRetriever, IConfigHelper configHelper) : base(actionHelper, tenantInfoRetriever, configHelper)
        {
        }
    }
}
