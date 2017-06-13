using Newtonsoft.Json;

namespace ServiceLibrary.Models.Workflow
{
    public class WorkflowTransition : ITrigger
    {
        public int WorkflowId { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public WorkflowState FromState { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public WorkflowState ToState { get; set; }
    }

    public class SqlWorkflowTransition
    {
        public int WorkflowId { get; set; }
        public int TriggerId { get; set; }
        public string TriggerName { get; set; }
        public int FromStateId { get; set; }
        public string FromStateName { get; set; }
        public int ToStateId { get; set; }
        public string ToStateName { get; set; }
        
    }
}