using System;
using System.Collections.Generic;
using System.Linq;
using ActionHandlerService.Models;
using BluePrintSys.Messaging.CrossCutting.Logging;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Workflow;

namespace ActionHandlerService.Helpers
{
    public interface IActionsParser
    {
        List<NotificationAction> GetNotificationActions(IEnumerable<SqlWorkflowEvent> sqlArtifactTriggers);
    }

    public class ActionsParser : IActionsParser
    {
        public List<NotificationAction> GetNotificationActions(IEnumerable<SqlWorkflowEvent> sqlArtifactTriggers)
        {
            var notifications = new List<NotificationAction>();
            foreach (var workflowEvent in sqlArtifactTriggers)
            {
                var triggersXml = workflowEvent.Triggers;
                var xmlWorkflowEventTriggers = new XmlWorkflowEventTriggers();
                if (!string.IsNullOrWhiteSpace(triggersXml))
                {
                    try
                    {
                        Log.Debug($"Deserializing triggers: {triggersXml}");
                        var triggersFromXml = SerializationHelper.FromXml<XmlWorkflowEventTriggers>(triggersXml);
                        if (triggersFromXml != null)
                        {
                            xmlWorkflowEventTriggers = triggersFromXml;
                        }
                        else
                        {
                            Log.Debug($"Invalid triggers XML: {triggersXml}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Deserialization failed for triggers: {triggersXml}", ex);
                    }
                }
                var triggersWithEmailActions = xmlWorkflowEventTriggers.Triggers.Where(trigger => trigger.Action.ActionType == ActionTypes.EmailNotification);
                foreach (var trigger in triggersWithEmailActions)
                {
                    var action = (XmlEmailNotificationAction) trigger.Action;
                    XmlStateCondition condition = null;
                    if (trigger.Condition?.ConditionType == ConditionTypes.State)
                    {
                        condition = (XmlStateCondition) trigger.Condition;
                    }
                    notifications.Add(
                        new NotificationAction
                        {
                            ToEmails = action.Emails,
                            ConditionalStateId = condition?.StateId,
                            PropertyTypeId = workflowEvent.EventPropertyTypeId ?? 0
                        });
                }
            }
            return notifications;
        }
    }

    public class NotificationAction
    {
        public IEnumerable<string> ToEmails { get; set; }
        public int? ConditionalStateId { get; set; }
        public int PropertyTypeId { get; set; }
        public string FromDisplayName => string.Empty;
        public string Subject => "Artifact has been updated.";
        public string MessageTemplate => "You are being notified because of an update to the following artifact:";
    }
}
