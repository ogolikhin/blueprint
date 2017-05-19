using System.ComponentModel.DataAnnotations;

namespace ServiceLibrary.Models.Workflow
{
    public class WorkflowTransition : BaseInformation
    {
        public int WorkflowId { get; set; }

        public int WorkflowFromStateId { get; set; }

        public int WorkflowToStateId { get; set; }

        [MinLength(1)]
        [MaxLength(128)]
        public string WorkflowFromStateName { get; set; }

        [MinLength(1)]
        [MaxLength(128)]
        public string WorkflowToStateName { get; set; }
    }
}