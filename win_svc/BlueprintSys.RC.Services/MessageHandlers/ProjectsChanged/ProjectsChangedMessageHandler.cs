using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.Models.Actions;

namespace BlueprintSys.RC.Services.MessageHandlers.ProjectsChanged
{
    public class ProjectsChangedMessageHandler : BaseMessageHandler<ProjectsChangedMessage>
    {
        public ProjectsChangedMessageHandler() : this(new ProjectsChangedActionHelper(), new TenantInfoRetriever(), new ConfigHelper())
        {
        }
        public ProjectsChangedMessageHandler(
            IActionHelper actionHelper,
            ITenantInfoRetriever tenantInfoRetriever,
            IConfigHelper configHelper)
            : base(actionHelper, tenantInfoRetriever, configHelper)
        {
        }
    }
}
