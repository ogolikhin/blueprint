using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace ServiceLibrary.Models.Workflow
{
    public class WorkflowState
    {
        public int WorkflowId { get; set; }

        public int StateId { get; set; }

        public string StateName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string StateDescription { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsDefault { get; set; }
    }

    public class WorkFlowStateDb
    {
        public int WorkflowId { get; set; }
        public string WorkflowStateName { get; set; }
        public int WorkflowStateId { get; set; }
    }
}