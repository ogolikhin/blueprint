using System.Threading.Tasks;

namespace ServiceLibrary.Models.Workflow
{
    public class PostopWorkflowEventTriggers : WorkflowEventTriggers
    {
        protected override Task InternalBatchExecute(IExecutionParameters executionParameters)
        {
            return Task.Run(() => { });
        }
    }
}