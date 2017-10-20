using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ServiceLibrary.Models.Workflow
{
    [JsonObject]
    public class WorkflowStateChangeParameter
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int CurrentVersionId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int FromStateId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int ToStateId { get; set; }
    }

    public class WorkflowStateChangeParameterEx : WorkflowStateChangeParameter
    {
        public WorkflowStateChangeParameterEx()
        {

        }

        public WorkflowStateChangeParameterEx(WorkflowStateChangeParameter workflowStateChangeParameter)
        {
            CurrentVersionId = workflowStateChangeParameter.CurrentVersionId;
            FromStateId = workflowStateChangeParameter.FromStateId;
            ToStateId = workflowStateChangeParameter.ToStateId;
        }

        [Required]
        [Range(1, int.MaxValue)]
        public int ArtifactId { get; set; }

        public int RevisionId { get; set; }

        public string Triggers { get; set; }

        public string UserName { get; set; }
    }
}
