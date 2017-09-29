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
            if (AllProjects && !AllArtifacts && (ArtifactIds == null || !ArtifactIds.Any()))
                return true;
            else if (AllArtifacts && !AllProjects && (ProjectIds == null || !ProjectIds.Any()))
                return true;
            else if (!AllArtifacts && !AllProjects
                && ((ArtifactIds == null || !ArtifactIds.Any())
                || (ProjectIds == null || !ProjectIds.Any())))
                return true;
            else return false;
        }
    }
}