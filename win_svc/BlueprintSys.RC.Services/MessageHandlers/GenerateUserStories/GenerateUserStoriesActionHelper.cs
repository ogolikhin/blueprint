using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Jobs;

namespace BlueprintSys.RC.Services.MessageHandlers.GenerateUserStories
{
    public class GenerateUserStoryInfo
    {
        public int ProcessId { get; set; }

        public int? TaskId { get; set; }
    }

    public class GenerateUserStoriesActionHelper : BoundaryReachedActionHandler
    {
        protected override async Task<bool> HandleActionInternal(TenantInformation tenant, ActionMessage actionMessage, IBaseRepository baseRepository)
        {
            var message = (GenerateUserStoriesMessage)actionMessage;
            if (message == null || message.ProjectId <= 0 || tenant == null)
            {
                Logger.Log($"Invalid GenerateTestsMessage received: {message?.ToJSON()}", message, tenant, LogLevel.Error);
                return false;
            }

            Logger.Log($"Handling of type: {message.ActionType} started for user ID {message.UserId}, revision ID {message.RevisionId} with message {message.ToJSON()}", message, tenant, LogLevel.Debug);

            var repository = (IGenerateActionsRepository)baseRepository;
            var user = await repository.GetUser(message.UserId);
            Logger.Log($"Retrieved user: {user?.Login}", message, tenant);

            var payload = new GenerateUserStoryInfo { ProcessId = message.ArtifactId, TaskId = null };
            var parameters = SerializationHelper.ToXml(payload);
            var jobId = await repository.JobsRepository.AddJobMessage(JobType.GenerateUserStories,
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
