﻿using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceLibrary.Models.Jobs;

namespace AdminStore.Repositories.Jobs
{
    public interface IJobsRepository
    {
        Task<IEnumerable<JobInfo>> GetVisibleJobs(
            int? userId,
            int? offset = null,
            int? limit = null,
            JobType? jobType = JobType.None);

        Task<JobInfo> GetJob(int jobId, int? userId);
    }
}