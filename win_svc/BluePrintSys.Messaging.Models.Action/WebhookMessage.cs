using ServiceLibrary.Models.Enums;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BluePrintSys.Messaging.Models.Actions
{
    public class WebhookMessage : ActionMessage
    {
        public override MessageActionType ActionType { get; } = MessageActionType.Webhook;

        // Authentication Information
        public string Url { get; set; }

        public bool IgnoreInvalidSSLCertificate { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Webhook Authentication.")]
        public List<string> HttpHeaders { get; set; }

        public string BasicAuthUsername { get; set; }

        public string BasicAuthPassword { get; set; }

        public string SignatureSecretToken { get; set; }

        public string SignatureAlgorithm { get; set; }

        // Payload Information
        public int ArtifactId { get; set; }

        public string ArtifactName { get; set; }
    }
}
