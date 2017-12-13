using System;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Models;
using BlueprintSys.RC.Services.Repositories;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Models.Enums;

namespace BlueprintSys.RC.Services.MessageHandlers.PropertyItemTypesChanged
{
    public class PropertyItemTypesChangedActionHelper : MessageActionHandler
    {
        protected override Task<bool> HandleActionInternal(TenantInformation tenant, ActionMessage actionMessage,
            IActionHandlerServiceRepository actionHandlerServiceRepository)
        {
            if (!actionMessage.ActionType.HasFlag(MessageActionType.PropertyItemTypesChanged))
            {
                throw new NotSupportedException("Property/Itemtypes changed handler can only handle PropertyItemTypesChanged types");
            }
            // call service repostiory to execute stored procedure.
            return Task.FromResult(true);
        }
    }
}
