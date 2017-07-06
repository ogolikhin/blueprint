using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminStore.Models.Workflow
{
    public class WorkflowArtifactTypeDto : IEquatable<WorkflowArtifactTypeDto>
    {
        public string Name { get; set; }
        public bool Equals(WorkflowArtifactTypeDto other)
        {
            if (ReferenceEquals(other, null)) return false;

            if (ReferenceEquals(this, other)) return true;

            return Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            int hashWorkflowArtifactTypeDtoName = Name == null ? 0 : Name.GetHashCode();

            return hashWorkflowArtifactTypeDtoName;
        }
    }
}