using System.Threading.Tasks;
using BlueprintSys.RC.Services.Models;
using BlueprintSys.RC.Services.Repositories;
using BluePrintSys.Messaging.CrossCutting.Logging;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Exceptions;

namespace BlueprintSys.RC.Services.MessageHandlers
{
    public abstract class BoundaryReachedActionHandler : MessageActionHandler
    {
        protected override async Task<bool> PreActionValidation(TenantInformation tenant, ActionMessage actionMessage,
            IActionHandlerServiceRepository actionHandlerServiceRepository)
        {
            var projectContainerActionMessage = actionMessage as ProjectContainerActionMessage;
            if (projectContainerActionMessage == null)
            {
                return false;
            }
            var isBoundaryReached = await actionHandlerServiceRepository.IsBoundaryReached(projectContainerActionMessage.ProjectId);
            if (isBoundaryReached)
            {
                Log.Error($"Boundary for project {projectContainerActionMessage.ProjectId} bas been reached");
            }
            return !isBoundaryReached;
        }
    }
}
