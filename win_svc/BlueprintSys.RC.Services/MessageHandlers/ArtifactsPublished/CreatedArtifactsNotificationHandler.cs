using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.CrossCutting.Helpers;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Repositories.ConfigControl;

namespace BlueprintSys.RC.Services.MessageHandlers.ArtifactsPublished
{
    internal class CreatedArtifactsNotificationHandler
    {
        private const string LogSource = "ArtifactsPublishedActionHelper.CreatedArtifacts";
        internal static async Task<bool> ProcessCreatedArtifacts(TenantInformation tenant,
            ArtifactsPublishedMessage message,
            IArtifactsPublishedRepository repository,
            IServiceLogRepository serviceLogRepository,
            IWorkflowMessagingProcessor messageProcessor,
            int transactionCommitWaitTimeInMilliSeconds = 60000)
        {
            var createdArtifacts = message?.Artifacts?.Where(p => p.IsFirstTimePublished && repository.WorkflowRepository.IsWorkflowSupported((ItemTypePredefined)p.Predefined)).ToList();
            if (createdArtifacts == null || createdArtifacts.Count <= 0)
            {
                Logger.Log("No created artifacts found", message, tenant);
                return false;
            }
            Logger.Log($"{createdArtifacts.Count} created artifacts found", message, tenant);

            var artifactIds = createdArtifacts.Select(a => a.Id).ToHashSet();
            var artifactInfos = (await repository.WorkflowRepository.GetWorkflowMessageArtifactInfoAsync(message.UserId, artifactIds, message.RevisionId)).ToDictionary(k => k.Id);
            Logger.Log($"{artifactInfos.Count} artifact infos found", message, tenant);
            var notificationMessages = new Dictionary<int, List<IWorkflowMessage>>();

            foreach (var createdArtifact in createdArtifacts)
            {
                WorkflowMessageArtifactInfo artifactInfo;
                if (!artifactInfos.TryGetValue(createdArtifact.Id, out artifactInfo))
                {
                    await serviceLogRepository.LogInformation(LogSource, $"Could not recover information for artifact Id: {createdArtifact.Id} and Name: {createdArtifact.Name} and Project Id: {createdArtifact.ProjectId}");
                    Logger.Log($"Could not recover information for artifact Id: {createdArtifact.Id} and Name: {createdArtifact.Name} and Project Id: {createdArtifact.ProjectId}", message, tenant);
                    continue;
                }

                var eventTriggers = await repository.WorkflowRepository.GetWorkflowEventTriggersForNewArtifactEvent(message.UserId,
                    new[] { createdArtifact.Id },
                    message.RevisionId, true);

                if (eventTriggers?.AsynchronousTriggers == null 
                    || eventTriggers.AsynchronousTriggers.Count == 0)
                {
                    Logger.Log($"Found no async triggers for artifact with ID {createdArtifact.Id}", message, tenant);
                    continue;
                }
                Logger.Log($"Found {eventTriggers.AsynchronousTriggers.Count} async triggers for artifact with ID {createdArtifact.Id}", message, tenant);

                IEnumerable<ArtifactPropertyInfo> artifactPropertyInfo = null;
                if (eventTriggers.AsynchronousTriggers.Any(tr => tr.ActionType == MessageActionType.Webhooks))
                {
                    artifactPropertyInfo = await repository.WorkflowRepository.GetArtifactsWithPropertyValuesAsync(message.UserId, new List<int>(createdArtifact.Id));
                }

                int artifactId = createdArtifact.Id;

                var actionMessages = await WorkflowEventsMessagesHelper.GenerateMessages(message.UserId,
                    message.RevisionId,
                    message.UserName,
                    message.TransactionId,
                    eventTriggers.AsynchronousTriggers,
                    artifactInfo,
                    artifactInfo.ProjectName,
                    new Dictionary<int, IList<Property>>(),
                    createdArtifact.Url,
                    createdArtifact.BaseUrl,
                    createdArtifact.AncestorArtifactTypeIds,
                    repository.UsersRepository,
                    serviceLogRepository,
                    repository.WebhooksRepository,
                    artifactPropertyInfo);

                if (actionMessages == null || actionMessages.Count == 0)
                {
                    continue;
                }

                if (!notificationMessages.ContainsKey(artifactId))
                {
                    notificationMessages.Add(artifactId, new List<IWorkflowMessage>());
                }

                notificationMessages[artifactId].AddRange(actionMessages);
            }

            if (notificationMessages.Count == 0)
            {
                Logger.Log("None of the created artifacts have async triggers", message, tenant);
                return false;
            }
            Logger.Log($"Sending async trigger messages for artifacts: {string.Join(", ", notificationMessages.Select(kvp => kvp.Key))}", message, tenant);

            foreach (var notificationMessage in notificationMessages.Where(m => m.Value != null))
            {
                await WorkflowEventsMessagesHelper.ProcessMessages(LogSource,
                    tenant,
                    serviceLogRepository,
                    notificationMessage.Value,
                    $"Error on new artifact creation with Id: {notificationMessage.Key}",
                    messageProcessor);
            }

            return true;
        }
    }
}
