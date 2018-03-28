using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.CrossCutting.Helpers;
using BluePrintSys.Messaging.Models.Actions;

namespace BlueprintSys.RC.Services.MessageHandlers.ArtifactsChanged
{
    public static class ArtifactsChangedMessageSender
    {
        public const int MaximumArtifactBatchSize = 1000;

        public static async Task Send(IEnumerable<int> artifactIds, TenantInformation tenant, ActionMessage sourceMessage, IWorkflowMessagingProcessor workflowMessagingProcessor)
        {
            var artifacts = artifactIds?.ToList();
            if (artifacts == null || !artifacts.Any())
            {
                Logger.Log("No artifact IDs found; no need to send Artifact Changed Message", sourceMessage, tenant);
                return;
            }
            Logger.Log($"Found {artifacts.Count} artifact IDs", sourceMessage, tenant);

            var artifactsChangedMessage = new ArtifactsChangedMessage
            {
                TransactionId = sourceMessage.TransactionId,
                RevisionId = sourceMessage.RevisionId,
                UserId = sourceMessage.UserId,
                ChangeType = ArtifactChangedType.Indirect
            };

            while (artifacts.Any())
            {
                var batch = artifacts.Take(MaximumArtifactBatchSize).ToList();
                artifactsChangedMessage.ArtifactIds = batch;

                Logger.Log($"Sending Artifacts Changed Message for {batch.Count} artifact IDs: {string.Join(",", batch)}", sourceMessage, tenant);
                await ActionMessageSender.Send(artifactsChangedMessage, tenant, workflowMessagingProcessor);
                Logger.Log("Finished sending Artifacts Changed Message", sourceMessage, tenant);

                artifacts = artifacts.Skip(MaximumArtifactBatchSize).ToList();
            }
        }
    }
}
