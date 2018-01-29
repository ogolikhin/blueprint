namespace ServiceLibrary.Models
{
    public class Webhook
    {
        public int WebhookId { get; set; }
        public string Url { get; set; }
        public string SecurityInfo { get; set; }
        public bool State { get; set; }
        public WebhookScope Scope { get; set; }
        public WebhookEventType EventType { get; set; }
        public int WorkflowVersionId { get; set; }
    }

    public enum WebhookScope
    {
        None = 0,
        Instance = 1,
        Project = 2,
        Workflow = 3
    }

    public enum WebhookEventType
    {
        None = 0,

        ArtifactCreated = 1,
        ArtifactDeleted = 2,
        ArtifactUpdated = 3,

        ProjectCreated = 10,
        ProjectDeleted = 11,
        ProjectUpdated = 12,

        WorkflowTransistion = 20
    }
}
