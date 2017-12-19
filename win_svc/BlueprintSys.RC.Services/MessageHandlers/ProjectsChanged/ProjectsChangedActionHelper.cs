using System.Linq;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.Models.Actions;

namespace BlueprintSys.RC.Services.MessageHandlers.ProjectsChanged
{
    public class ProjectsChangedActionHelper : IActionHelper
    {
        public async Task<bool> HandleAction(TenantInformation tenant, ActionMessage actionMessage, IBaseRepository baseRepository)
        {
            var message = (ProjectsChangedMessage) actionMessage;
            var repository = (ProjectsChangedRepository) baseRepository;

            Logger.Log("Getting affected artifact IDs", message, tenant);
            var artifactIds = await repository.GetAffectedArtifactIds();
            if (!artifactIds.Any())
            {
                Logger.Log("No artifact IDs found", message, tenant);
                return false;
            }
            Logger.Log($"Found artifact IDs {string.Join(",", artifactIds)}", message, tenant);

            return true;
        }
    }
}
