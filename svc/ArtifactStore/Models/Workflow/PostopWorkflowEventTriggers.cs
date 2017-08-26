using System.Threading.Tasks;
using ServiceLibrary.Models.Workflow;

namespace ArtifactStore.Models.Workflow
{
    public class PostopWorkflowEventTriggers : WorkflowEventTriggers
    {
        protected override Task InternalBatchExecute(IExecutionParameters executionParameters)
        {
            return Task.Run(() => { });
        }
    }
}