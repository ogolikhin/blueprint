using NServiceBus;

namespace BluePrintSys.ActionMessaging.Models
{
    public enum ActionType
    {
        None,
        Property,
        Notification,
        GenerateDescendants,
        GenerateTests,
        GenerateUserStories
    }

    [Express]
    public abstract class ActionMessage : IMessage
    {
        protected ActionMessage(ActionType actionType, int tenantId, int workflowId)
        {
            ActionType = actionType;
            TenantId = tenantId;
            WorkflowId = workflowId;
        }

        public ActionType ActionType { get; set; }
        public int TenantId { get; set; }
        public int WorkflowId { get; set; }
    }

    [Express]
    public class NotificationMessage : ActionMessage
    {
        public NotificationMessage(int tenantId, int workflowId) : base(ActionType.Notification, tenantId, workflowId)
        {
        }
    }

    [Express]
    public class GenerateDescendantsMessage : ActionMessage
    {
        public GenerateDescendantsMessage(int tenantId, int workflowId) : base(ActionType.GenerateDescendants, tenantId, workflowId)
        {
        }
    }

    [Express]
    public class GenerateTestsMessage : ActionMessage
    {
        public GenerateTestsMessage(int tenantId, int workflowId) : base(ActionType.GenerateTests, tenantId, workflowId)
        {
        }
    }

    [Express]
    public class GenerateUserStoriesMessage : ActionMessage
    {
        public GenerateUserStoriesMessage(int tenantId, int workflowId) : base(ActionType.GenerateUserStories, tenantId, workflowId)
        {
        }
    }
}
