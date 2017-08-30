using System.Threading.Tasks;
using ActionHandlerService.Helpers;
using ActionHandlerService.Models;
using ActionHandlerService.Repositories;
using BluePrintSys.Messaging.CrossCutting.Logging;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Jobs;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.Jobs;

namespace ActionHandlerService.MessageHandlers.GenerateUserStories
{
    public class GenerateUserStoryInfo
    {
        public int ProcessId { get; set; }

        public int? TaskId { get; set; }
    }

    public class GenerateUserStoriesActionHelper : IActionHelper
    {
        public async Task<bool> HandleAction(TenantInformation tenant, ActionMessage actionMessage, IActionHandlerServiceRepository actionHandlerServiceRepository)
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
            var jobsRepository = new JobsRepository(new SqlConnectionWrapper(tenant.BlueprintConnectionString));
            var jobId = await jobsRepository.AddJobMessage(JobType.GenerateUserStories,
                false, parameters, null, generateUserStoriesMessage.ProjectId, 
                generateUserStoriesMessage.ProjectName, generateUserStoriesMessage.UserId, 
                generateUserStoriesMessage.UserName, generateUserStoriesMessage.BaseHostUri);

            if (jobId.HasValue)
            {
                Log.Debug($"Job scheduled for {generateUserStoriesMessage.ActionType} with id: {jobId.Value}");
            }

            return jobId.HasValue && jobId > 0;

        }
    }
}
