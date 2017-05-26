using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ServiceLibrary.Models.Workflow
{
    public class WorkflowTransition
    {
        public int WorkflowId { get; set; }

        [Required]
        public int TransitionId { get; set; }

        [Required]
        public string TransitionName { get; set; }

        [Required]
        public int WorkflowFromStateId { get; set; }

        [Required]
        public int WorkflowToStateId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [MinLength(WorkflowConstants.MinNameLength)]
        [MaxLength(WorkflowConstants.MaxNameLength)]
        public string WorkflowFromStateName { get; set; }

        [Required]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [MinLength(WorkflowConstants.MinNameLength)]
        [MaxLength(WorkflowConstants.MaxNameLength)]
        public string WorkflowToStateName { get; set; }
    }
}