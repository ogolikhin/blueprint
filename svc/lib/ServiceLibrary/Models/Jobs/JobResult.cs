using System;
using System.Collections.Generic;

namespace ServiceLibrary.Models.Jobs
{
    public class JobResult
    {
        public IEnumerable<JobInfo> JobInfos { get; set; }

        public int? TotalJobCount { get; set; }
    }
}
