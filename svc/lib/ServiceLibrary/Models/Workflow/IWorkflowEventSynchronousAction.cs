using System.Threading.Tasks;

namespace ServiceLibrary.Models.Workflow
{
    public interface IWorkflowEventSynchronousAction
    {
        Task<bool> Execute(IExecutionParameters executionParameters);
    }
}
