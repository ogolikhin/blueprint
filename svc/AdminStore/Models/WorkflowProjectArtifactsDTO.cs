using System.Collections;

namespace AdminStore.Models
{
    public class WorkflowProjectArtifactsDto
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public IEnumerable Artifacts { get; set; }
    }


}