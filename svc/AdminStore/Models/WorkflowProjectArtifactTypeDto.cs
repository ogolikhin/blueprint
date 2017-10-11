using System.Collections;
using System.Collections.Generic;

namespace AdminStore.Models
{
    public class WorkflowProjectArtifactTypeDto
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public IEnumerable<WorkflowArtifact> Artifacts { get; set; }
    }


}