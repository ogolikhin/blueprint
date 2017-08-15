using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminStore.Models.Workflow
{
    public class SqlWorkflowArtifactTypes
    {
        public int ArtifactTypeId { get; set; }
        public string ArtifactTypeName { get; set; }
        public int ProjectId { get; set; }
        public string ProjectPath { get; set; }
    }
}