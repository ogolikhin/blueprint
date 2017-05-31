using Newtonsoft.Json;

namespace ServiceLibrary.Models.Workflow
{
    public class WorkflowTransition
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
        public int TriggerId { get; set; }
        public int StateId { get; set; }
        public string StateName { get; set; }
        public int CurrentStateId { get; set; }
        public string TriggerName { get; set; }
    }
}