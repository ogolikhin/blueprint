using System.Collections.Generic;

namespace Model.JobModel.Impl
{
    public class JobResult
    {
        public IEnumerable<JobInfo> JobInfos { get; set; }

        public int? TotalJobCount { get; set; }
    }
}