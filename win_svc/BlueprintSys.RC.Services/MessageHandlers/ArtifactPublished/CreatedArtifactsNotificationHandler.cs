using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BlueprintSys.RC.Services.Models;
using BlueprintSys.RC.Services.Repositories;
using BluePrintSys.Messaging.CrossCutting.Models.Exceptions;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Repositories.ConfigControl;

namespace BlueprintSys.RC.Services.MessageHandlers.ArtifactPublished
{
    internal class CreatedArtifactsNotificationHandler
    {
        private const string LogSource = "ArtifactsPublishedActionHelper.CreatedArtifacts";

        internal static async Task<bool> ProcessCreatedArtifacts(TenantInformation tenant,
            List<PublishedArtifactInformation> createdArtifacts,
            ArtifactsPublishedMessage message,
            IArtifactsPublishedRepository repository,
            IServiceLogRepository serviceLogRepository)
        {
            if (createdArtifacts == null || createdArtifacts.Count <= 0)
            {
                return true;
            }


            var artifactIds = createdArtifacts.Select(a => a.Id).ToHashSet();

            var artifactInfos = (await repository.WorkflowRepository.GetWorkflowMessageArtifactInfoAsync(message.UserId,
                artifactIds,
                message.RevisionId)).ToDictionary(k => k.Id);

            var notFoundArtifactIds = artifactIds.Except(artifactInfos.Keys).ToHashSet();
            if (notFoundArtifactIds.Count > 0)
            {
                var notFoundArtifactIdString = string.Join(", ", notFoundArtifactIds);
                await serviceLogRepository.LogInformation(LogSource,
                    $"Could not recover information for following artifacts {notFoundArtifactIdString}");
                Logger.Log($"Could not recover information for following artifacts {notFoundArtifactIdString}", message, tenant, LogLevel.Debug);
                throw new EntityNotFoundException($"Could not recover information for following artifacts {notFoundArtifactIdString}");
            }

            foreach (var createdArtifact in createdArtifacts)
            {
                WorkflowMessageArtifactInfo artifactInfo;
                if (!artifactInfos.TryGetValue(createdArtifact.Id, out artifactInfo))
                {
                    await serviceLogRepository.LogInformation(LogSource,
                    $"Could not recover information for artifact Id: {createdArtifact.Id} and Name: {createdArtifact.Name} and Project Id: {createdArtifact.ProjectId}");
                    Logger.Log($"Could not recover information for artifact Id: {createdArtifact.Id} and Name: {createdArtifact.Name} and Project Id: {createdArtifact.ProjectId}",
                        message, tenant, LogLevel.Debug);
                    continue;
                }

                var eventTriggers = await repository.WorkflowRepository.GetWorkflowEventTriggersForNewArtifactEvent(message.UserId,
                    new[] { createdArtifact.Id },
                    message.RevisionId);
                var actionMessages = await WorkflowEventsMessagesHelper.GenerateMessages(message.UserId,
                    message.RevisionId,
                    message.UserName,
                    eventTriggers.AsynchronousTriggers,
                    artifactInfo,
                    artifactInfo.ProjectName,
                    new Dictionary<int, IList<Property>>(),
                    false,
                    createdArtifact.Url,
                    createdArtifact.BaseUrl,
                    repository.UsersRepository,
                    serviceLogRepository
                    );

                await WorkflowEventsMessagesHelper.ProcessMessages(LogSource,
                    tenant.TenantId,
                    serviceLogRepository,
                    actionMessages,
                    $"Error on new artifact creation with Id: {createdArtifact.Id}");
            }
            return await Task.FromResult(true);
        }
    }
}
