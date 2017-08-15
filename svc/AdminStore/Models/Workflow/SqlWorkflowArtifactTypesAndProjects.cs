using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminStore.Models.Workflow
{
    public class SqlWorkflowArtifactTypesAndProjects
    {
        public string ArtifactName { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
    }

    public class SqlWorkflowArtifactTypes
    {
        public int ArtifactTypeId { get; set; }
        public string ArtifactTypeName { get; set; }
        public int ProjectId { get; set; }
    }
}