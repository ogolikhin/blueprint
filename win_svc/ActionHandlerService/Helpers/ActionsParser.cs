using System.Collections.Generic;
using ActionHandlerService.MessageHandlers.ArtifactPublished;
using ActionHandlerService.Models;

namespace ActionHandlerService.Helpers
{
    public interface IActionsParser
    {
        IEnumerable<NotificationAction> GetNotificationActions(IEnumerable<SqlArtifactTriggers> sqlArtifactTriggers);
    }

    public class ActionsParser : IActionsParser
    {
        public IEnumerable<NotificationAction> GetNotificationActions(IEnumerable<SqlArtifactTriggers> sqlArtifactTriggers)
        {
            //Should be replaced with some pattern here
            foreach (var workflowEvent in sqlArtifactTriggers)
            {
                var triggers = GetTriggers(workflowEvent.Triggers);
                foreach (var trigger in triggers)
                {
                    yield return new NotificationAction
                    {
                        PropertyTypeId = workflowEvent.EventPropertyTypeId ?? 0,
                        ConditionalStateId = workflowEvent.RequiredPreviousStateId,
                        FromEmail = trigger.FromEmail,
                        ToEmail = trigger.ToEmail,
                        Subject = trigger.Subject,
                        MessageTemplate = trigger.MessageTemplate
                    };
                }
            }
        }

        private List<Trigger> GetTriggers(string triggersXml)
        {
            var triggers = new List<Trigger>();
            if (!string.IsNullOrWhiteSpace(triggersXml))
            {
                //TODO parse the XML
                var testTrigger = new Trigger
                {
                    FromEmail = "munish.saini@blueprintsys.com",
                    ToEmail = "munish.saini@blueprintsys.com",
                    Subject = "Artifact has been published.",
                    MessageTemplate = "Artifact has been published."
                };
                triggers.Add(testTrigger);
            }
            return triggers;
        }
    }

    public class Trigger
    {
        public string FromEmail { get; set; }
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string MessageTemplate { get; set; }
    }
}
