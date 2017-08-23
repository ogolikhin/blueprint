using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceLibrary.Helpers.Validators;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.Reuse;

namespace BluePrintSys.Messaging.CrossCutting.Models
{
    public interface IExecutionParameters
    {
        ItemTypeReuseTemplate ReuseItemTemplate { get; }
        List<DPropertyType> CustomPropertyTypes { get; }
        IDbTransaction Transaction { get; }
        IReadOnlyList<IPropertyValidator> Validators { get; }
        int UserId { get; }
    }

    public abstract class WorkflowEventAction
    {
        public abstract MessageActionType ActionType { get; }

        public abstract Task<bool> Execute(IExecutionParameters executionParameters);
    }

    #region Synchronous Actions

    public interface IWorkflowEventSynchronousAction
    {
        bool ValidateActionToBeProcessed(IExecutionParameters executionParameters);
    }

    public abstract class WorkflowEventSynchronousWorkflowEventAction : WorkflowEventAction, IWorkflowEventSynchronousAction
    {
        public override async Task<bool> Execute(IExecutionParameters executionParameters)
        {
            var result = ValidateActionToBeProcessed(executionParameters);
            return await Task.FromResult(result);
        }

        public abstract bool ValidateActionToBeProcessed(IExecutionParameters executionParameters);

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

        public int? ConditionalStateId { get; set; }

        public int? PropertyTypeId { get; set; }

        public string Message { get; set; } = "You are being notified because of an update to the following artifact:";

        public string FromDisplayName { get; set; } = string.Empty;

        public string Subject { get; set; } = "Artifact has been published.";

        public override MessageActionType ActionType { get; } = MessageActionType.Notification;

        public override async Task<bool> Execute(IExecutionParameters executionParameters)
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

        public override MessageActionType ActionType { get; } = MessageActionType.GenerateChildren;
        public override async Task<bool> Execute(IExecutionParameters executionParameters)
        {
            return await Task.FromResult(true);
        }
    }

    public class GenerateUserStoriesAction : GenerateAction
    {
        public override MessageActionType ActionType { get; } = MessageActionType.GenerateUserStories;

        public override async Task<bool> Execute(IExecutionParameters executionParameters)
        {
            return await Task.FromResult(true);
        }
    }

    public class GenerateTestCasesAction : GenerateAction
    {
        public override MessageActionType ActionType { get; } = MessageActionType.GenerateTests;

        public override async Task<bool> Execute(IExecutionParameters executionParameters)
        {
            return await Task.FromResult(true);
        }
    }

    #endregion
}
