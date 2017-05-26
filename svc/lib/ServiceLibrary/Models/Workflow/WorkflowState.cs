using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace ServiceLibrary.Models.Workflow
{
    public class WorkflowState
    {
        [Required]
        public int WorkflowId { get; set; }

        [Required]
        public int StateId { get; set; }

        [Required]
        [MinLength(WorkflowConstants.MinNameLength)]
        [MaxLength(WorkflowConstants.MaxNameLength)]
        public string StateName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [MinLength(WorkflowConstants.MinDescriptionLength)]
        [MaxLength(WorkflowConstants.MaxDescriptionLength)]
        public string StateDescription { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsDefault { get; set; }
    }
}