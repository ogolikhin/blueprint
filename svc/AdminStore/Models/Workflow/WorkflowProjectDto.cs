using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminStore.Models.Workflow
{
    public class WorkflowProjectDto : IEquatable<WorkflowProjectDto>
    {
        public int Id { get; set; }
        public string Name{ get; set; }
        public bool Equals(WorkflowProjectDto other)
        {
            if (ReferenceEquals(other, null)) return false;

            if (ReferenceEquals(this, other)) return true;

            return Id.Equals(other.Id) && Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            int hashWorkflowProjectDtoName = Name == null ? 0 : Name.GetHashCode();

            int hashWorkflowProjectDtoId = Id.GetHashCode();

            return hashWorkflowProjectDtoName ^ hashWorkflowProjectDtoId;
        }
    }
}