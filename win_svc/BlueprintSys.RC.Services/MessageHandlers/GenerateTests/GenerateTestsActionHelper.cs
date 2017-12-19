using System.Collections.Generic;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Jobs;

namespace BlueprintSys.RC.Services.MessageHandlers.GenerateTests
{
    public class GenerateTestsActionHelper : IActionHelper
    {
        public async Task<bool> HandleAction(TenantInformation tenant, ActionMessage actionMessage, IBaseRepository baseRepository)
        {
            var message = (GenerateTestsMessage) actionMessage;
            if (message == null || message.ArtifactId <= 0 || message.ProjectId <= 0 || message.RevisionId <= 0 || message.UserId <= 0 || string.IsNullOrWhiteSpace(message.UserName) || tenant == null)
            {
                Logger.Log($"Invalid GenerateTestsMessage received: {message?.ToJSON()}", message, tenant, LogLevel.Error);
                return false;
            }

            Logger.Log($"Handling of type: {message.ActionType} started for user ID {message.UserId}, revision ID {message.RevisionId} with message {message.ToJSON()}", message, tenant, LogLevel.Debug);

            var repository = (IGenerateActionsRepository) baseRepository;

            var generateProcessTestInfos = new List<GenerateProcessTestInfo>
            {
                new GenerateProcessTestInfo
                {
                    ProcessId = message.ArtifactId
                }
            };
            var parameters = SerializationHelper.ToXml(generateProcessTestInfos);

            var user = await repository.GetUser(message.UserId);
            Logger.Log($"Retrieved user: {user?.Login}", message, tenant);

            var jobId = await repository.JobsRepository.AddJobMessage(JobType.GenerateProcessTests,
                false,
                parameters,
                null,
                message.ProjectId,
                message.ProjectName,
                message.UserId,
                user?.Login,
                message.BaseHostUri);

            if (jobId.HasValue)
            {
                Logger.Log($"Job scheduled for {message.ActionType} with id: {jobId.Value}", message, tenant);
            }
            else
            {
                Logger.Log("No jobId received", message, tenant);
            }

            return jobId.HasValue && jobId > 0;
        }
    }
}
