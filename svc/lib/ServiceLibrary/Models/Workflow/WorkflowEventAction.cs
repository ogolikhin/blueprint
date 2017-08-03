using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceLibrary.Models.Enums;

namespace ServiceLibrary.Models.Workflow
{
    public abstract class WorkflowEventAction
    {
        public abstract WorkflowActionType ActionType { get; }

        public virtual async Task<bool> Execute()
        {
            return await Task.FromResult(true);
        } 
    }

    public interface ISynchronousAction { }
    public interface IASynchronousAction { }

    public class EmailNotificationAction : WorkflowEventAction, IASynchronousAction
    {
        public IList<string> Emails { get; } = new List<string>();

        public int? PropertyTypeId { get; set; }
        
        public string Message { get; set; }

        public override WorkflowActionType ActionType { get; } = WorkflowActionType.Notification;
    }

    public class PropertyChangeAction : WorkflowEventAction, ISynchronousAction
    {
        public int? InstancePropertyTypeId { get; set; }

        public string PropertyValue { get; set; }

        public override WorkflowActionType ActionType { get; } = WorkflowActionType.Notification;
    }

    public class PropertyChangeUserGroupsAction : PropertyChangeAction
    {
        // Used for User properties and indicates that PropertyValue contains the group name.
        public List<ActionUserGroups> UserGroups { get; } = new List<ActionUserGroups>();
    }

    public class ActionUserGroups
    {
        public int Id;
        public bool? IsGroup;
    }

    public abstract class GenerateAction : WorkflowEventAction, IASynchronousAction
    {
    }

    public class GenerateChildrenAction : GenerateAction
    {
        public int? ChildCount { get; set; }

        public int ArtifactTypeId { get; set; }

        public override WorkflowActionType ActionType { get; } = WorkflowActionType.GenerateChildren;
    }

    public class GenerateUserStoriesAction : GenerateAction
    {
        public override WorkflowActionType ActionType { get; } = WorkflowActionType.GenerateUserStories;
    }

    public class GenerateTestCasesAction : GenerateAction
    {
        public override WorkflowActionType ActionType { get; } = WorkflowActionType.GenerateTests;
    }
}
