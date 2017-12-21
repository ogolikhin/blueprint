using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.Models.Actions;

namespace BlueprintSys.RC.Services.MessageHandlers
{
    public abstract class BoundaryReachedActionHandler : MessageActionHandler
    {
        protected override async Task<bool> PreActionValidation(TenantInformation tenant, ActionMessage actionMessage, IBaseRepository baseRepository)
        {
            var projectContainerActionMessage = actionMessage as ProjectContainerActionMessage;
            if (projectContainerActionMessage == null)
            {
                Logger.Log("The message is not a projectContainerActionMessage", actionMessage, tenant, LogLevel.Error);
                return false;
            }
            var isProjectMaxArtifactBoundaryReached = await baseRepository.IsProjectMaxArtifactBoundaryReached(projectContainerActionMessage.ProjectId);
            if (isProjectMaxArtifactBoundaryReached)
            {
                Logger.Log($"Max artifact boundary for project {projectContainerActionMessage.ProjectId} has been reached", projectContainerActionMessage, tenant, LogLevel.Error);
            }
            return !isProjectMaxArtifactBoundaryReached;
        }
    }
}
