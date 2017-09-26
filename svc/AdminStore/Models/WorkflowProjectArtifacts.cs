using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminStore.Models.Workflow;

namespace AdminStore.Models
{
    public class WorkflowProjectArtifacts
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Prefix { get; set; }
        public string ArtifactName { get; set; }
        public int ArtifactId { get; set; }
    }
}