using Newtonsoft.Json;

namespace ServiceLibrary.Models.Workflow
{
    public class WorkflowTransition
    {
        public int WorkflowId { get; set; }

        public int TransitionId { get; set; }

        public string TransitionName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public WorkflowState FromState { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public WorkflowState ToState { get; set; }
    }

    public class Transition
    {
        public int TriggerId { get; set; }
        public int StateId { get; set; }
        public string StateName { get; set; }
        public int CurrentStateId { get; set; }
        public string TriggerName { get; set; }
    }
}