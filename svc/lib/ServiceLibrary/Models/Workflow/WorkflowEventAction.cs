using System.Threading.Tasks;
using ServiceLibrary.Models.Enums;

namespace ServiceLibrary.Models.Workflow
{
    public abstract class WorkflowEventAction
    {
        public abstract MessageActionType ActionType { get; }

        public abstract Task<bool> Execute(IExecutionParameters executionParameters);
    }
}
