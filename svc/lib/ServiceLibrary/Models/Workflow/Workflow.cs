using System.Collections.Generic;
using Newtonsoft.Json;

namespace ServiceLibrary.Models.Workflow
{
    public class Workflow
    {
        public int WorkflowId { get; set; }

        public string WorkflowName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<WorkflowState> States { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<WorkflowTransition> Transitions { get; set; }
    }
}