using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ServiceLibrary.Models.Workflow
{
    public class WorkflowTransition : IWorkflowEvent
    {
        public int WorkflowId { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public WorkflowState FromState { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public WorkflowState ToState { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public WorkflowEventTriggers Triggers { get; } = new WorkflowEventTriggers();
    }
}
