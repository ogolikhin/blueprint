using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;

namespace ArtifactStore.Models.Workflow
{
    public class WorkflowTransitionResult : QueryResult<WorkflowTransition>
    {
        public QueryResultCode ResultCode { get; set; }

        public int Count { get; set; }
    }
}