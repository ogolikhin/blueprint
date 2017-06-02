using ServiceLibrary.Models.Enums;

namespace ServiceLibrary.Models.Workflow
{
    public class WorkflowTransitionResult : QueryResult<WorkflowTransition>
    {
        public QueryResultCode ResultCode { get; set; }

        public int Count { get; set; }
    }
}