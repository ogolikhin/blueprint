using System.Collections.Generic;
using Newtonsoft.Json;

namespace ServiceLibrary.Models.Workflow
{
    public class Workflow
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<WorkflowState> States { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<WorkflowTransition> Transitions { get; set; }
    }
}