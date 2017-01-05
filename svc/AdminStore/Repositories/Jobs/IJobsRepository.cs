using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceLibrary.Models.Files;
using ServiceLibrary.Models.Jobs;

namespace AdminStore.Repositories.Jobs
{
    public interface IJobsRepository
    {
        Task<IEnumerable<JobInfo>> GetVisibleJobs
        (
            int userId,
            int? offset = null,
            int? limit = null,
            JobType? jobType = JobType.None
        );

        Task<JobInfo> GetJob(int jobId, int userId);

        Task<File> GetJobResultFile(Uri baseUri, int jobId, int userId, string sessionToken);
    }
}
