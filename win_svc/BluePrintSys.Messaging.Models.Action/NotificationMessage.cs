using System.Collections.Generic;
using NServiceBus;

namespace BluePrintSys.Messaging.Models.Actions
{
    [Express]
    public class NotificationMessage : ActionMessage
    {
        public override MessageActionType ActionType { get; } = MessageActionType.Notification;

        public IEnumerable<string> To { get; set; }

        public string From { get; set; }
        
        public string Subject { get; set; }

        public string MessageTemplate { get; set; }
        public int ArtifactId { get; set; }
        public string ArtifactName { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ArtifactUrl { get; set; }
        public int RevisionId { get; set; }
        public int ArtifactTypeId { get; set; }
        public int ArtifactTypePredefined { get; set; }
        public int UserId { get; set; }
        public ModifiedPropertyInformation[] ModifiedPropertiesInformation { get; set; }
    }

    public class ModifiedPropertyInformation
    {
        public int? PropertyId { get; set; }
        public string PropertyName { get; set; }
        public int PredefinedTypeId { get; set; }
    }
}
