using ServiceLibrary.Models.Workflow;

namespace AdminStore.Models.DiagramWorkflow
{
    public abstract class DCondition
    {
        public abstract ConditionTypes ConditionType { get; }
    }
}