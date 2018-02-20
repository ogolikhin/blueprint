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
            var message = (ArtifactsChangedMessage)actionMessage;
            var repository = (IArtifactsChangedRepository)baseRepository;
            var artifactIds = message.ArtifactIds?.ToList();

            if (artifactIds == null || !artifactIds.Any())
            {
                Logger.Log("The Artifacts Changed Message contains no artifact IDs", message, tenant);
                return false;
            }

            Logger.Log($"Handling Artifacts Changed Message with change type {message.ChangeType} and {artifactIds.Count} artifact IDs: {string.Join(", ", artifactIds)}", message, tenant);
            Logger.Log("Started repopulating search items", message, tenant);
            await repository.RepopulateSearchItems(artifactIds);
            Logger.Log("Finished repopulating search items", message, tenant);
            return true;
        }
    }
}
