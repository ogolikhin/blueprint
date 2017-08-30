using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.Models.Actions;

namespace BlueprintSys.RC.Services.MessageHandlers.StateTransition
{
    public class StateTransitionMessageHandler : BaseMessageHandler<StateChangeMessage>
    {
        public StateTransitionMessageHandler() : this(new StateTransitionActionHelper(), new TenantInfoRetriever(), new ConfigHelper())
        {
        }

        public StateTransitionMessageHandler(IActionHelper actionHelper, ITenantInfoRetriever tenantInfoRetriever, IConfigHelper configHelper) : base(actionHelper, tenantInfoRetriever, configHelper)
        {
        }
    }
}
