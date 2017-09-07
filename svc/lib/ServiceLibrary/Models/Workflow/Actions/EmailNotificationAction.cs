using System.Collections.Generic;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.PropertyType;

namespace ServiceLibrary.Models.Workflow.Actions
{
    public class EmailNotificationAction : WorkflowEventAction, IWorkflowEventASynchronousAction
    {
        public IList<string> Emails { get; } = new List<string>();

        public int? ConditionalStateId { get; set; }

        public int? EventPropertyTypeId { get; set; }

        public int? PropertyTypeId { get; set; }

        public string Header { get; set; } = "You are being notified because of an update to the following artifact:";

        public string Message { get; set; }

        public string FromDisplayName { get; set; } = string.Empty;

        public string Subject { get; set; } = "Artifact has been updated.";

        public override MessageActionType ActionType { get; } = MessageActionType.Notification;

        public override PropertySetResult ValidateAction(IExecutionParameters executionParameters)
        {
            return null;
        }
    }

}
