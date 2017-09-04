namespace ServiceLibrary.Models.Workflow
{
    public class WorkflowTriggersContainer
    {
        public WorkflowEventTriggers SynchronousTriggers { get; } = new PostopWorkflowEventTriggers();

        public WorkflowEventTriggers AsynchronousTriggers { get; } = new PostopWorkflowEventTriggers();
    }
}
