using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;

namespace AdminStore.Models
{
    public class WorkflowProjectArtifactsDto
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", 
        Justification = "For Xml serialization, the property sometimes needs to be null")]
        public List<WorkflowArtifact> Artifacts { get; set; }
    }


}