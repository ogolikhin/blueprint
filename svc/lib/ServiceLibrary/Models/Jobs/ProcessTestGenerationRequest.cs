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
        public int ProjectId { get; set; }

        public int ProcessId { get; set; }

        public bool GenerateForIncludedProcesses { get; set; }
    }
}
