using System.Threading.Tasks;
using BlueprintSys.RC.Services.Models;
using BlueprintSys.RC.Services.Repositories;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Exceptions;

namespace BlueprintSys.RC.Services.MessageHandlers
{
    public abstract class BoundaryReachedActionHandler : MessageActionHandler
    {
        protected override async void PreActionValidation(TenantInformation tenant, ActionMessage actionMessage,
            IActionHandlerServiceRepository actionHandlerServiceRepository)
        {
            var projectContainerActionMessage = actionMessage as ProjectContainerActionMessage;
            if (projectContainerActionMessage == null)
            {
                return;
            }
            base.PreActionValidation(tenant, actionMessage, actionHandlerServiceRepository);
            await CheckForBoundary(projectContainerActionMessage.ProjectId, actionHandlerServiceRepository);
        }

        protected async Task CheckForBoundary(int projectId, IActionHandlerServiceRepository actionHandlerServiceRepository)
        {
            var isBoundaryReached = await actionHandlerServiceRepository.IsBoundaryReached(projectId);
            if (isBoundaryReached)
            {
                throw new BoundaryReachedException("No more artifacts can be created due to package limitation.");
            }
        }
    }
}
