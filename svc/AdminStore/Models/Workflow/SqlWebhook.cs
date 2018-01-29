namespace AdminStore.Models.Workflow
{
    public class SqlWebhook
    {
        public int WebhookId { get; set; }

        public string Url { get; set; }

        public string SecurityInfo { get; set; }

        public bool State { get; set; }

        public DWebhookScope Scope { get; set; }

        public DWebhookEventType EventType { get; set; }

        public int WorkflowVersionId { get; set; }
    }

    public enum DWebhookScope
    {
        None = 0,
        Instance = 1,
        Project = 2,
        Workflow = 3
    }

    public enum DWebhookEventType
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