using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BluePrintSys.Messaging.CrossCutting.Models;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Helpers.Validators;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.Workflow;

namespace ArtifactStore.Models.Workflow.Actions
{

    public class PropertyChangeAction : WorkflowEventSynchronousWorkflowEventAction
    {
        public int InstancePropertyTypeId { get; set; }

        public string PropertyValue { get; set; }

        public PropertyLite PropertyLiteValue { get; private set; }

        public override MessageActionType ActionType { get; } = MessageActionType.PropertyChange;

        public override async Task<bool> Execute(IExecutionParameters executionParameters)
        {
            await base.Execute(executionParameters);

            return await Task.FromResult(true);
        }

        public override bool ValidateActionToBeProcessed(IExecutionParameters executionParameters)
        {
            ValidateReuseSettings(executionParameters);
            var result = ValidateProperty(executionParameters);
            if (result != null)
            {
                return false;
            }
            return true;
        }

        private PropertySetResult ValidateProperty(IExecutionParameters executionParameters)
        {

            if (InstancePropertyTypeId == WorkflowConstants.PropertyTypeFakeIdDescription ||
                InstancePropertyTypeId == WorkflowConstants.PropertyTypeFakeIdName)
            {
                // todo: validate in later stories
                return null;
            }
            var dPropertyType = executionParameters.CustomPropertyTypes.FirstOrDefault(item => item.InstancePropertyTypeId == InstancePropertyTypeId);
            if (dPropertyType == null)
            {
                throw new ConflictException(
                    I18NHelper.FormatInvariant("Property type id {0} is not associated with specified artifact type", InstancePropertyTypeId));
            }

            PopulatePropertyLite(dPropertyType);

            return executionParameters.Validators.Select(v => v.Validate(PropertyLiteValue, executionParameters.CustomPropertyTypes)).FirstOrDefault(r => r != null);
        }

        private void ValidateReuseSettings(IExecutionParameters executionParameters)
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

        private void PopulatePropertyLite(DPropertyType propertyType)
        {
            switch (propertyType?.PrimitiveType)
            {
                case PropertyPrimitiveType.Number:
                    decimal value;
                    if (!Decimal.TryParse(PropertyValue, NumberStyles.AllowDecimalPoint, new NumberFormatInfo(), out value))
                    {
                        throw new FormatException("Property change action provided incorrect value type");
                    }
                    PropertyLiteValue = new PropertyLite()
                    {
                        PropertyTypeId = InstancePropertyTypeId,
                        NumberValue = value
                    };
                    break;
               
                default:
                    PropertyLiteValue = new PropertyLite()
                    {
                        PropertyTypeId = InstancePropertyTypeId
                    };
                    break;
            }
        }
    }

    public class PropertyChangeUserGroupsAction : PropertyChangeAction
    {
        // Used for User properties and indicates that PropertyValue contains the group name.
        public List<ActionUserGroups> UserGroups { get; } = new List<ActionUserGroups>();
    }

}
