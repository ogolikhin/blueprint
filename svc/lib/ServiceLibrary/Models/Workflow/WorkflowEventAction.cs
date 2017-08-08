using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Reuse;

namespace ServiceLibrary.Models.Workflow
{
    public class CustomProperties
    {
        public int InstancePropertyTypeId;

        public int PropertyTypeId;

        public int ProjectId;

        public int VersionId;

        public int Predefined;

        public string Name;

        public int PrimitiveType;

        public int StartRevision;
    }
    public abstract class WorkflowEventAction
    {
        public abstract WorkflowActionType ActionType { get; }

        public abstract Task<bool> Execute(ExecutionParameters executionParameters);
    }

    #region Synchronous Actions

    public interface IWorkflowEventSynchronousAction
    {
        Task<bool> IsActionValidToBeProcessed(ExecutionParameters executionParameters);
    }

    public abstract class WorkflowEventSynchronousWorkflowEventAction : WorkflowEventAction, IWorkflowEventSynchronousAction
    {
        public override async Task<bool> Execute(ExecutionParameters executionParameters)
        {
            if (!await IsActionValidToBeProcessed(executionParameters))
            {
                throw new Exception();
            }
            return await Task.FromResult(true);
        }

        public abstract Task<bool> IsActionValidToBeProcessed(ExecutionParameters executionParameters);

    }

    public class PropertyChangeAction : WorkflowEventSynchronousWorkflowEventAction
    {
        public int? InstancePropertyTypeId { get; set; }

        public string PropertyValue { get; set; }

        public override WorkflowActionType ActionType { get; } = WorkflowActionType.PropertyChange;

        public override async Task<bool> Execute(ExecutionParameters executionParameters)
        {
            await base.Execute(executionParameters);

            return await Task.FromResult(true);
        }

        public override async Task<bool> IsActionValidToBeProcessed(ExecutionParameters executionParameters)
        {
            ValidateReuseSettings(executionParameters);
            return await Task.FromResult(true);
        }

        private void ValidateReuseSettings(ExecutionParameters executionParameters)
        {
            var reuseTemplate = executionParameters.ReuseItemTemplate;
            if (reuseTemplate == null || !InstancePropertyTypeId.HasValue)
            {
                return;
            }

            if (reuseTemplate.ReadOnlySettings.HasFlag(ItemTypeReuseTemplateSetting.Name) &&
                InstancePropertyTypeId == -1)
            {
                throw new Exception("Cannot modify name ");
            }
            if (reuseTemplate.ReadOnlySettings.HasFlag(ItemTypeReuseTemplateSetting.Description) &&
                InstancePropertyTypeId == -2)
            {
                throw new Exception("Cannot modify name ");
            }

            var customProperty = reuseTemplate.PropertyTypeReuseTemplates[InstancePropertyTypeId.Value];

            var propertyReusetemplate = reuseTemplate.PropertyTypeReuseTemplates[customProperty.PropertyTypeId];
            if (propertyReusetemplate.Settings == PropertyTypeReuseTemplateSettings.ReadOnly)
            {
                throw new Exception("Cannot modify property");
            }
        }
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
