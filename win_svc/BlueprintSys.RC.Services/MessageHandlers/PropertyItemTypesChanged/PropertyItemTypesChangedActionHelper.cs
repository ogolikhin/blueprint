using System.Linq;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.Models.Actions;

namespace BlueprintSys.RC.Services.MessageHandlers.PropertyItemTypesChanged
{
    public class PropertyItemTypesChangedActionHelper : MessageActionHandler
    {
        protected override async Task<bool> HandleActionInternal(TenantInformation tenant, ActionMessage actionMessage, IBaseRepository baseRepository)
        {
            var message = (PropertyItemTypesChangedMessage) actionMessage;
            var repository = (PropertyItemTypesChangedRepository) baseRepository;

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
