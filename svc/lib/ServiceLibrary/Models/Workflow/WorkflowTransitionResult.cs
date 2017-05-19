using System.Collections.Generic;

namespace ServiceLibrary.Models.Workflow
{
    public class WorkflowTransitionResult : IPaginatedResult<WorkflowTransition>
    {
        public int Total { get; set; }

        public int Count { get; set; }

        public IEnumerable<WorkflowTransition> Items { get; set; } 
    }
}