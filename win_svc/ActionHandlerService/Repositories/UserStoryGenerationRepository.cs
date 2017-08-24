using System.Threading.Tasks;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Jobs;
using ServiceLibrary.Repositories.Jobs;

namespace ActionHandlerService.Repositories
{
    public class GenerateUserStoryInfo
    {
        public int ProcessId { get; set; }

        public int? TaskId { get; set; }
    }

    public class UserStoryGenerationRepository
    {
        public static async Task<bool> GenerateUserStories(int projectId, int processId, string projectName, string username, int userId, string uri)
        {
            var payload = new GenerateUserStoryInfo { ProcessId = processId, TaskId = null };
            var parameters = SerializationHelper.ToXml(payload);
            var jobsRepository = new JobsRepository();
            var jobId = await jobsRepository.AddJobMessage(JobType.GenerateUserStories, 
                false, parameters, null, projectId, projectName, userId, username, uri);

            return jobId > 0;
        }
    }
}
