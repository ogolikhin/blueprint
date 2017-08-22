using System.Threading.Tasks;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Jobs;
using ServiceLibrary.Repositories;

namespace ActionHandlerService.Repositories
{
    public class GenerateUserStoryInfo
    {
        public int ProcessId { get; set; }

        public int? TaskId { get; set; }
    }

    public class UserStoryGenerationRepository
    {
        public static async Task GenerateUserStories(int projectId, int processId, int? taskId = null)
        {
            var payload = new GenerateUserStoryInfo { ProcessId = processId, TaskId = taskId };
            var parameters = SerializationHelper.ToXml(payload);
            //var hostUri = ServerUriHelper.BaseHostUri;
            var jobsRepository = new JobsRepository();
            var jobId = await jobsRepository.AddJobMessage(JobType.GenerateUserStories,
                false,
                parameters,
                null,
                projectId,
                string.Empty,
                1, //TODO: get actual user info
                "admin",
                "http://localhost:9801/");
        }
    }
}
