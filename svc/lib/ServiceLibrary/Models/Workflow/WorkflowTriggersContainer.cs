namespace ServiceLibrary.Models.Workflow
{
    public class WorkflowTriggersContainer
    {
        public WorkflowEventTriggers SynchronousTriggers { get; set; }

        public WorkflowEventTriggers AsynchronousTriggers { get; set; }
    }
}
