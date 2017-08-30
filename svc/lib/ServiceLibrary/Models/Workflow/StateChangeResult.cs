using System.Collections.Generic;

namespace ServiceLibrary.Models.Workflow
{
    public class StateChangeResult
    {
        public QuerySingleResult<WorkflowState> Result { get; set; }

        public IList<IWorkflowMessage> ActionMessages { get; } = new List<IWorkflowMessage>();
    }
}
