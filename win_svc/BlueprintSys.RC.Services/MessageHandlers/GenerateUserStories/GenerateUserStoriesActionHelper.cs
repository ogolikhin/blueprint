using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BlueprintSys.RC.Services.Models;
using BlueprintSys.RC.Services.Repositories;
using BluePrintSys.Messaging.CrossCutting.Logging;
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
        protected override async Task<bool> HandleActionInternal(TenantInformation tenant, ActionMessage actionMessage, IActionHandlerServiceRepository actionHandlerServiceRepository)
        {
            var generateUserStoriesMessage = actionMessage as GenerateUserStoriesMessage;
            if (generateUserStoriesMessage == null)
            {
                Log.Debug("Invalid GenerateTestsMessage received");
                return false;
            }

            Logger.Log($"Handling of type: {generateUserStoriesMessage.ActionType} started for user ID {generateUserStoriesMessage.UserId}, revision ID {generateUserStoriesMessage.RevisionId} with message {generateUserStoriesMessage.ToJSON()}", generateUserStoriesMessage, tenant, LogLevel.Debug);

            var payload = new GenerateUserStoryInfo { ProcessId = generateUserStoriesMessage.ArtifactId, TaskId = null };
            var parameters = SerializationHelper.ToXml(payload);
            var generationUserStoriesActionRepo = (IGenerateUserStoriesRepository) actionHandlerServiceRepository;
            var jobsRepository = generationUserStoriesActionRepo.JobsRepository;
            var user = await GetUserInfo(generateUserStoriesMessage, actionHandlerServiceRepository);

            var jobId = await jobsRepository.AddJobMessage(JobType.GenerateUserStories,
                false, 
                parameters, 
                null, 
                generateUserStoriesMessage.ProjectId, 
                generateUserStoriesMessage.ProjectName, 
                generateUserStoriesMessage.UserId, 
                user?.Login, 
                generateUserStoriesMessage.BaseHostUri);

            if (jobId.HasValue)
            {
                Log.Debug($"Job scheduled for {generateUserStoriesMessage.ActionType} with id: {jobId.Value}");
            }

            return jobId.HasValue && jobId > 0;

        }
    }
}
