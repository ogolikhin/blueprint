using System.Collections;
using System.Collections.Generic;

namespace AdminStore.Models
{
    public class WorkflowProjectArtifactTypesDto
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public IEnumerable<WorkflowArtifactType> Artifacts { get; set; }
    }


}