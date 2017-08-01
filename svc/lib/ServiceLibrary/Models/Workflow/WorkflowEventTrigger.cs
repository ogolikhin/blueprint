namespace ServiceLibrary.Models.Workflow
{
    public class WorkflowEventTrigger
    {
        public string Name { get; set; }
        
        public EventAction Action { get; set; }

        public WorkflowEventCondition Condition { get; set; }
    }
}
