using System.Collections;
using System.Collections.Generic;

namespace AdminStore.Models
{
    public class WorkflowProjectArtifactsDto
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public IEnumerable<WorkflowArtifact> Artifacts { get; set; }
    }


}