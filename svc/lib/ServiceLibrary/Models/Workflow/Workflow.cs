using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ServiceLibrary.Models.Workflow
{
    public class Workflow
    {
        [Required]
        public int WorkflowId { get; set; }

        [Required]
        [MinLength(WorkflowConstants.MinNameLength)]
        [MaxLength(WorkflowConstants.MaxNameLength)]
        public string WorkflowName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<WorkflowState> States { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<WorkflowTransition> Transitions { get; set; }
    }
}