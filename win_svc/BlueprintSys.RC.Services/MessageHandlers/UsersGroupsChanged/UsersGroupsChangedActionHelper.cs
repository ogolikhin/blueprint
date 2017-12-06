using System;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Models;
using BlueprintSys.RC.Services.Repositories;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Models.Enums;

namespace BlueprintSys.RC.Services.MessageHandlers.UsersGroupsChanged
{
    public class UsersGroupsChangedActionHelper : MessageActionHandler
    {
        protected override Task<bool> HandleActionInternal(TenantInformation tenant, ActionMessage actionMessage,
            IActionHandlerServiceRepository actionHandlerServiceRepository)
        {
            if (!actionMessage.ActionType.HasFlag(MessageActionType.UsersGroupsChanged))
            {
                throw new NotSupportedException("Artifacts changed handler can only handle Artifacts Changed type");
            }
            // call service repostiory to execute stored procedure.
            return Task.FromResult(true);
        }
    }
}
