using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminStore.Models.Workflow
{
    public class SqlArtifactTypesWorkflowDetails
    {
        public string Name { get; set; }
        public int VersionProjectId { get; set; }
        public int? WorkflowId { get; set; }
    }
}