using System.Threading.Tasks;
using ServiceLibrary.Models.Files;
using ServiceLibrary.Models.Jobs;
using ServiceLibrary.Repositories.Files;

namespace ServiceLibrary.Repositories.Jobs
{
    public interface IJobsRepository
    {
        Task<JobResult> GetVisibleJobs
        (
            int userId,
            int? offset = null,
            int? limit = null,
            JobType? jobType = JobType.None);

        Task<JobInfo> GetJob(int jobId, int userId);

        Task<File> GetJobResultFile(int jobId, int userId, IFileRepository fileRepository);

        Task<int?> AddJobMessage(JobType type, bool hidden, string parameters, string receiverJobServiceId,
            int? projectId, string projectLabel, int userId, string userName, string hostUri);
    }
}
