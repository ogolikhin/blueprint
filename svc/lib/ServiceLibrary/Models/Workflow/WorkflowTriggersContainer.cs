namespace ServiceLibrary.Models.Workflow
{
    public class WorkflowTriggersContainer
    {
        public WorkflowEventTriggers SynchronousTriggers { get; } = new PreopWorkflowEventTriggers();

        public WorkflowEventTriggers AsynchronousTriggers { get; } = new PostopWorkflowEventTriggers();
    }
}
