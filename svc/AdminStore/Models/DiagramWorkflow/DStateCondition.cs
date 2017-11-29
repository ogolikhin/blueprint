using Newtonsoft.Json;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Models.DiagramWorkflow
{
    public class DStateCondition : DCondition
    {
        public override ConditionTypes ConditionType => ConditionTypes.State;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string State { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? StateId { get; set; }
    }
}