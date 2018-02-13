using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;

namespace BluePrintSys.Messaging.Models.Actions
{
    public class WebhookMessage : ActionMessage
    {
        public override MessageActionType ActionType { get; } = MessageActionType.Webhook;

        // Authentication Information
        public string Url { get; set; }

        public string SecurityInfo { get; set; }

        // Payload Information
        public int ArtifactId { get; set; }

        public string ArtifactName { get; set; }

    }
}
