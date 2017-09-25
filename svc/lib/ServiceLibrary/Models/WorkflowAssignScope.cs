using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminStore.Models
{
    public class WorkflowAssignScope
    {
        public bool AllProjects { get; set; }

        public IEnumerable<int> ProjectIds { get; set; }

        public bool AllArtifacts { get; set; }

        public IEnumerable<int> ArtifactIds { get; set; }

        public bool IsEmpty()
        {
            if (AllProjects && (ArtifactIds == null || !ArtifactIds.Any()))
                return true;
            else if (AllArtifacts && (ProjectIds == null || !ProjectIds.Any()))
                return true;
            else if ((ArtifactIds == null || !ArtifactIds.Any()) && (AllArtifacts && (ProjectIds == null || !ProjectIds.Any())))
                return true;
            else return false;
        }
    }
}