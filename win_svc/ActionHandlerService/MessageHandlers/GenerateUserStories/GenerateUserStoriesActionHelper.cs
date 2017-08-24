using System.Threading.Tasks;
using ActionHandlerService.Models;
using ActionHandlerService.Repositories;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Jobs;
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
                return false;
            }

            var payload = new GenerateUserStoryInfo { ProcessId = generateUserStoriesMessage.ArtifactId, TaskId = null };
            var parameters = SerializationHelper.ToXml(payload);
            var jobsRepository = new JobsRepository();
            var jobId = await jobsRepository.AddJobMessage(JobType.GenerateUserStories,
                false, parameters, null, generateUserStoriesMessage.ProjectId, 
                generateUserStoriesMessage.ProjectName, generateUserStoriesMessage.UserId, 
                generateUserStoriesMessage.UserName, generateUserStoriesMessage.BaseHostUri);

            return jobId > 0;

        }
    }
}
