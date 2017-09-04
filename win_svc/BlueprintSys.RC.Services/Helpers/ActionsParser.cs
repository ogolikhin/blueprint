using System;
using System.Collections.Generic;
using System.Linq;
using BluePrintSys.Messaging.CrossCutting.Logging;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Models.Workflow.Actions;

namespace BlueprintSys.RC.Services.Helpers
{
    public interface IActionsParser
    {
        List<EmailNotificationAction> GetNotificationActions(IEnumerable<SqlWorkflowEvent> sqlArtifactTriggers);
    }

    public class ActionsParser : IActionsParser
    {
        public List<EmailNotificationAction> GetNotificationActions(IEnumerable<SqlWorkflowEvent> sqlArtifactTriggers)
        {
            var notifications = new List<EmailNotificationAction>();
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
                    var emailNotification = new EmailNotificationAction
                    {
                        ConditionalStateId = condition?.StateId,
                        EventPropertyTypeId = workflowEvent.EventPropertyTypeId ?? 0,
                        PropertyTypeId = action.PropertyTypeId,
                        Message = action.Message
                    };
                    emailNotification.Emails.AddRange(action.Emails);
                    notifications.Add(emailNotification);
                }
            }
            return notifications;
        }
    }
}
