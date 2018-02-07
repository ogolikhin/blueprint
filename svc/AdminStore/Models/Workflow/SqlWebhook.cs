using System;

namespace AdminStore.Models.Workflow
{
    public class SqlWebhook
    {
        public int WebhookId { get; set; }

        public string Url { get; set; }

        public string SecurityInfo { get; set; }

        public bool State { get; set; }

        public string Scope { get; set; }

        public DWebhookEventType EventType { get; set; }

        public int WorkflowId { get; set; }
    }

    public enum DWebhookScope
    {
        None = 0,
        Instance = 1,
        Project = 2,
        Workflow = 3
    }

    [Flags]
    public enum DWebhookEventType
    {
        None = 0,

        ArtifactCreated = 1,
        ArtifactDeleted = 2,
        ArtifactUpdated = 4,

        ProjectCreated = 8,
        ProjectDeleted = 16,
        ProjectUpdated = 32,

        WorkflowTransition = 64
    }
}