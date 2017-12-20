using System.Linq;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.Models.Actions;

namespace BlueprintSys.RC.Services.MessageHandlers.ArtifactsChanged
{
    public class ArtifactsChangedActionHelper : MessageActionHandler
    {
        protected override async Task<bool> HandleActionInternal(TenantInformation tenant, ActionMessage actionMessage, IBaseRepository baseRepository)
        {
            var message = (ArtifactsChangedMessage) actionMessage;
            var repository = (IArtifactsChangedRepository) baseRepository;
            var artifactIds = message.ArtifactIds.ToList();

            Logger.Log($"ArtifactsChanged message received for artifact ids {string.Join(", ", artifactIds)}.", message, tenant);

            if (!artifactIds.Any())
            {
                Logger.Log("The message contains no artifact IDs", message, tenant);
                return false;
            }

            Logger.Log("Started repopulating search items", message, tenant);
            await repository.RepopulateSearchItems(artifactIds);
            Logger.Log("Finished repopulating search items", message, tenant);
            return true;
        }
    }
}
