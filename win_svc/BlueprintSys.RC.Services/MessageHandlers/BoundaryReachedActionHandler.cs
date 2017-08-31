using System.Threading.Tasks;
using BlueprintSys.RC.Services.Models;
using BlueprintSys.RC.Services.Repositories;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Exceptions;

namespace BlueprintSys.RC.Services.MessageHandlers
{
    public abstract class BoundaryReachedActionHandler : IActionHelper
    {
        public async Task<bool> HandleAction(TenantInformation tenant, ActionMessage actionMessage,
            IActionHandlerServiceRepository actionHandlerServiceRepository)
        {
            var projectContainerActionMessage = actionMessage as ProjectContainerActionMessage;
            if (projectContainerActionMessage == null)
            {
                return false;
            }
            await CheckForBoundary(projectContainerActionMessage.ProjectId, actionHandlerServiceRepository);
            return await HandleActionInternal(tenant, actionMessage, actionHandlerServiceRepository);
        }

        protected abstract Task<bool> HandleActionInternal(TenantInformation tenant, ActionMessage actionMessage,
            IActionHandlerServiceRepository actionHandlerServiceRepository);

        protected async Task CheckForBoundary(int projectId, IActionHandlerServiceRepository actionHandlerServiceRepository)
        {
            if (await actionHandlerServiceRepository.IsBoundaryReached(projectId))
            {
                throw new BoundaryReachedException("No more artifacts can be created due to package limitation.");
            }
        }
    }
}
