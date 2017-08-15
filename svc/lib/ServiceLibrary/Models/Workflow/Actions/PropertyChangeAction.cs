using System.Linq;
using System.Threading.Tasks;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Helpers.Validators;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.PropertyType;

namespace ServiceLibrary.Models.Workflow.Actions
{

    public class PropertyChangeAction : WorkflowEventSynchronousWorkflowEventAction
    {
        public int InstancePropertyTypeId { get; set; }

        public string PropertyValue { get; set; }

        public override WorkflowActionType ActionType { get; } = WorkflowActionType.PropertyChange;

        public override async Task<bool> Execute(ExecutionParameters executionParameters)
        {
            await base.Execute(executionParameters);

            return await Task.FromResult(true);
        }

        public override void ValidateActionToBeProcessed(ExecutionParameters executionParameters)
        {
            ValidateReuseSettings(executionParameters);
            var result = ValidateProperty(executionParameters);
            if (result != null)
            {
                throw new ConflictException(result.Message, result.ErrorCode);
            }
        }

        private PropertySetResult ValidateProperty(ExecutionParameters executionParameters)
        {

            if (InstancePropertyTypeId == WorkflowConstants.PropertyTypeFakeIdDescription ||
                InstancePropertyTypeId == WorkflowConstants.PropertyTypeFakeIdName)
            {
                // todo: validate in later stories
                return null;
            }

            if (!executionParameters.InstancePropertyTypes.Exists(item => item.PropertyTypeId == InstancePropertyTypeId))
            {
                throw new ConflictException(
                    I18NHelper.FormatInvariant("Property type id {0} is not associated with specified artifact type", InstancePropertyTypeId));
            }
            var propertyLite = new PropertyLite
            {
                PropertyTypeId = InstancePropertyTypeId,
                Value = PropertyValue
            };

            return executionParameters.Validators.Select(v => v.Validate(propertyLite, executionParameters.InstancePropertyTypes)).FirstOrDefault(r => r != null);
        }
        private void ValidateReuseSettings(ExecutionParameters executionParameters)
        {
            var reuseTemplate = executionParameters.ReuseItemTemplate;
            if (reuseTemplate == null)
            {
                return;
            }

            if (reuseTemplate.ReadOnlySettings.HasFlag(ItemTypeReuseTemplateSetting.Name) &&
                InstancePropertyTypeId == WorkflowConstants.PropertyTypeFakeIdName)
            {
                throw new ConflictException("Cannot modify name from workflow event action. Property is readonly.");
            }

            if (reuseTemplate.ReadOnlySettings.HasFlag(ItemTypeReuseTemplateSetting.Description) &&
                InstancePropertyTypeId == WorkflowConstants.PropertyTypeFakeIdDescription)
            {
                throw new ConflictException("Cannot modify description from workflow event action. Property is readonly.");
            }

            var customProperty = reuseTemplate.PropertyTypeReuseTemplates[InstancePropertyTypeId];

            var propertyReusetemplate = reuseTemplate.PropertyTypeReuseTemplates[customProperty.PropertyTypeId];
            if (propertyReusetemplate.Settings == PropertyTypeReuseTemplateSettings.ReadOnly)
            {
                throw new ConflictException("Cannot modify property from workflow event action. Property is readonly.");
            }
        }
    }

}
