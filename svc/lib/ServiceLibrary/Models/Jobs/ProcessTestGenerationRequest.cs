using System.Collections.Generic;

namespace ServiceLibrary.Models.Jobs
{
    public class ProcessTestGenerationRequest
    {
        public int ProjectId { get; set; }

        public string ProjectName { get; set; }

        public IEnumerable<ProcessTestGenInfo> Processes { get; set; } 
    }

    public class ProcessTestGenInfo
    {
        public int ProcessId { get; set; }
    }
}
