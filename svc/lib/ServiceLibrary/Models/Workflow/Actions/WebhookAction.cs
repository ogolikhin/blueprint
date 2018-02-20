using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.PropertyType;

namespace ServiceLibrary.Models.Workflow.Actions
{
    public class WebhookAction : WorkflowEventAction, IWorkflowEventASynchronousAction
    {
        public int WebhookId { get; set; }

        public override MessageActionType ActionType { get; } = MessageActionType.Webhooks;

        public override PropertySetResult ValidateAction(IExecutionParameters executionParameters)
        {
            return null;
        }
    }
}
