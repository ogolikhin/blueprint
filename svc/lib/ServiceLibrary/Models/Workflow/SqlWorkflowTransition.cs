namespace ServiceLibrary.Models.Workflow
{
    public class SqlWorkflowTransition
    {
        public int WorkflowId { get; set; }
        public int WorkflowEventId { get; set; }
        public string WorkflowEventName { get; set; }
        public int FromStateId { get; set; }
        public string FromStateName { get; set; }
        public int ToStateId { get; set; }
        public string ToStateName { get; set; }

        public string Triggers { get; set; }
    }
}
