using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Helpers.Validators;
using ArtifactStore.Models.PropertyTypes;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
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

        public override WorkflowActionType ActionType { get; } = WorkflowActionType.PropertyChange;

        public override async Task<bool> Execute(ExecutionParameters executionParameters)
        {
            await base.Execute(executionParameters);

            return await Task.FromResult(true);
        }

        protected override bool ValidateActionToBeProcessed(ExecutionParameters executionParameters)
        {
            executionParameters.ReuseValidator.ValidateReuseSettings(InstancePropertyTypeId, executionParameters.ReuseItemTemplate);
            var result = ValidateProperty(executionParameters);
            if (result != null)
            {
                return false;
            }
            return true;
        }

        private PropertySetResult ValidateProperty(ExecutionParameters executionParameters)
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

}
