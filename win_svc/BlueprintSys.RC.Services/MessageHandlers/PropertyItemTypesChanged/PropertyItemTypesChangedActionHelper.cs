using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BlueprintSys.RC.Services.MessageHandlers.ArtifactsChanged;
using BluePrintSys.Messaging.CrossCutting.Helpers;
using BluePrintSys.Messaging.Models.Actions;

namespace BlueprintSys.RC.Services.MessageHandlers.PropertyItemTypesChanged
{
    public class PropertyItemTypesChangedActionHelper : MessageActionHandler
    {
        protected override async Task<bool> HandleActionInternal(TenantInformation tenant, ActionMessage actionMessage, IBaseRepository baseRepository)
        {
            return await HandlePropertyItemTypesChangedAction(tenant, actionMessage, baseRepository, WorkflowMessagingProcessor.Instance);
        }

        public async Task<bool> HandlePropertyItemTypesChangedAction(TenantInformation tenant, ActionMessage actionMessage, IBaseRepository baseRepository, IWorkflowMessagingProcessor workflowMessagingProcessor)
        {
            var message = (PropertyItemTypesChangedMessage)actionMessage;
            var repository = (IPropertyItemTypesChangedRepository)baseRepository;

            var revisionId = message.RevisionId;
            var isInstance = message.IsStandard;
            var itemTypes = message.ItemTypeIds?.ToList();
            var propertyTypes = message.PropertyTypeIds?.ToList();

            var artifactIds = new List<int>();

            if (itemTypes != null && itemTypes.Any())
            {
                Logger.Log("Getting affected artifact IDs for item types", message, tenant);
                var affectedArtifacts = await repository.GetAffectedArtifactIdsForItemTypes(itemTypes, isInstance, revisionId);
                Logger.Log($"Received {affectedArtifacts.Count} affected artifact IDs for item types", message, tenant);
                artifactIds.AddRange(affectedArtifacts);
            }

            if (propertyTypes != null && propertyTypes.Any())
            {
                Logger.Log("Getting affected artifact IDs for property types", message, tenant);
                var affectedArtifacts = await repository.GetAffectedArtifactIdsForPropertyTypes(propertyTypes, isInstance, revisionId);
                Logger.Log($"Received {affectedArtifacts.Count} affected artifact IDs for property types", message, tenant);
                artifactIds.AddRange(affectedArtifacts);
            }

            await ArtifactsChangedMessageSender.Send(artifactIds, tenant, actionMessage, workflowMessagingProcessor, repository);
            return true;
        }
    }
}
