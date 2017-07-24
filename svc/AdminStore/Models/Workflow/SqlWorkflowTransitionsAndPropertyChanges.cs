using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminStore.Models.Workflow
{
    public class SqlWorkflowTransitionsAndPropertyChanges
    {
        public int WorkflowId { get; set; }
        public string Name { get; set; }
        public string Permissions { get; set; }
        public int Type { get; set; }
        public string Actions { get; set; }
        public string FromState { get; set; }
        public string ToState { get; set; }
    }
}