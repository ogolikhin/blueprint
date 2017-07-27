using System;
using System.Collections.Generic;

namespace AdminStore.Models.Workflow
{
    public class WorkflowDto
    {
        public int WorkflowId { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastModified { get; set; }
        public string CreatedBy { get; set; }
        public bool Status { get; set; }
        public int VersionId { get; set; }
        public string Description { get; set; }
        public IEnumerable<WorkflowProjectDto> Projects{ get; set; }
        public IEnumerable<WorkflowArtifactTypeDto> ArtifactTypes { get; set; }
        public int NumberOfAssignedProjects { get; set; }

    }
}