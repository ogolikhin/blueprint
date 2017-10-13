using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminStore.Models
{
    public class AssignProjectsResult
    {
        public static AssignProjectsResult Empty => new AssignProjectsResult { TotalAssigned = 0, AllProjectsAssignedToWorkflow = false };
        public int TotalAssigned { get; set; }
        public bool AllProjectsAssignedToWorkflow { get; set; }
    }
}