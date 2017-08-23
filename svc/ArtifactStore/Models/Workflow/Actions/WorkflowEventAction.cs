using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceLibrary.Models.Enums;

namespace ArtifactStore.Models.Workflow.Actions
{
    public abstract class WorkflowEventAction
    {
        public abstract WorkflowActionType ActionType { get; }

        public abstract Task<bool> Execute(ExecutionParameters executionParameters);
    }

    #region Synchronous Actions

    public interface IWorkflowEventSynchronousAction
    {
        Task<bool> Execute(ExecutionParameters executionParameters);
    }

    public abstract class WorkflowEventSynchronousWorkflowEventAction : WorkflowEventAction, IWorkflowEventSynchronousAction
    {
        public override async Task<bool> Execute(ExecutionParameters executionParameters)
        {
            var result = ValidateActionToBeProcessed(executionParameters);
            return await Task.FromResult(result);
        }

        protected abstract bool ValidateActionToBeProcessed(ExecutionParameters executionParameters);

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

    #endregion

    #region Asynchronous actions

    public interface IWorkflowEventASynchronousAction { }

    public class EmailNotificationAction : WorkflowEventAction, IWorkflowEventASynchronousAction
    {
        public IList<string> Emails { get; } = new List<string>();

        public int? PropertyTypeId { get; set; }

        public string Message { get; set; }

        public override WorkflowActionType ActionType { get; } = WorkflowActionType.Notification;

        public override async Task<bool> Execute(ExecutionParameters executionParameters)
        {
            return await Task.FromResult(true);
        }
    }

    public abstract class GenerateAction : WorkflowEventAction, IWorkflowEventASynchronousAction
    {
    }

    public class GenerateChildrenAction : GenerateAction
    {
        public int? ChildCount { get; set; }

        public int ArtifactTypeId { get; set; }

        public override WorkflowActionType ActionType { get; } = WorkflowActionType.GenerateChildren;
        public override async Task<bool> Execute(ExecutionParameters executionParameters)
        {
            return await Task.FromResult(true);
        }
    }

    public class GenerateUserStoriesAction : GenerateAction
    {
        public override WorkflowActionType ActionType { get; } = WorkflowActionType.GenerateUserStories;

        public override async Task<bool> Execute(ExecutionParameters executionParameters)
        {
            return await Task.FromResult(true);
        }
    }

    public class GenerateTestCasesAction : GenerateAction
    {
        public override WorkflowActionType ActionType { get; } = WorkflowActionType.GenerateTests;

        public override async Task<bool> Execute(ExecutionParameters executionParameters)
        {
            return await Task.FromResult(true);
        }
    }

    #endregion
}
