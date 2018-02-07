using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.Models.Actions;

namespace BlueprintSys.RC.Services.MessageHandlers.ProjectsChanged
{
    public class ProjectsChangedMessageHandler : BaseMessageHandler<ProjectsChangedMessage>
    {
        public ProjectsChangedMessageHandler() : this(new ProjectsChangedActionHelper(), new TenantInfoRetriever(), new ConfigHelper(), new TransactionValidator())
        {
        }

        public ProjectsChangedMessageHandler(IActionHelper actionHelper, ITenantInfoRetriever tenantInfoRetriever, IConfigHelper configHelper, ITransactionValidator transactionValidator) : base(actionHelper, tenantInfoRetriever, configHelper, transactionValidator)
        {
        }
    }
}
