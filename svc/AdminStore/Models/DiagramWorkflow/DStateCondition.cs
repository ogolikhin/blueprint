using ServiceLibrary.Models.Workflow;

namespace AdminStore.Models.DiagramWorkflow
{
    public class DStateCondition : DCondition
    {
        public override ConditionTypes ConditionType => ConditionTypes.State;
        public string State { get; set; }
        public int? StateId { get; set; }
    }
}