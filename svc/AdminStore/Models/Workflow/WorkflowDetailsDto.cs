using System.Collections.Generic;

namespace AdminStore.Models.Workflow
{
    public class WorkflowDetailsDto
    {
        public int WorkflowId { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public int VersionId { get; set; }
        public string Description { get; set; }
        public IEnumerable<WorkflowProjectDto> Projects { get; set; }
        public IEnumerable<WorkflowArtifactTypeDto> ArtifactTypes { get; set; }
    }
}