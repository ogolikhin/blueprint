using System.ComponentModel.DataAnnotations;
using ServiceLibrary.Attributes;

namespace ServiceLibrary.Models.Workflow
{
    public class WorkflowStateChangeParameter
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int ArtifactId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int CurrentVersionId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int WorkflowId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int FromStateId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int ToStateId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int TransitionId { get; set; }
    }
}
