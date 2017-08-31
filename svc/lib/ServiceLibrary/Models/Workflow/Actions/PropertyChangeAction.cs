using System;
using System.Globalization;
using System.Linq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.PropertyType;
using System.Collections.Generic;

namespace ServiceLibrary.Models.Workflow.Actions
{
    public class PropertyChangeAction : WorkflowEventSynchronousWorkflowEventAction, IPropertyChangeAction
    {
        public int InstancePropertyTypeId { get; set; }

        public string PropertyValue { get; set; }

        public PropertyLite PropertyLiteValue { get; protected set; }
        public List<int> ValidValues { get; } = new List<int>();

        public override MessageActionType ActionType { get; } = MessageActionType.PropertyChange;

        protected override bool ValidateActionToBeProcessed(IExecutionParameters executionParameters)
        {
            executionParameters.ReuseValidator.ValidateReuseSettings(InstancePropertyTypeId, executionParameters.ReuseItemTemplate);
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
                return new PropertySetResult(InstancePropertyTypeId, ErrorCodes.InvalidArtifactProperty,
                     I18NHelper.FormatInvariant("Property type id {0} is not associated with specified artifact type", InstancePropertyTypeId));
            }

            var resultSet = PopulatePropertyLite(dPropertyType);
            if (resultSet != null)
            {
                return resultSet;
            }

            return executionParameters.Validators.Select(v => v.Validate(PropertyLiteValue, executionParameters.CustomPropertyTypes, executionParameters.ValidationContext)).FirstOrDefault(r => r != null);
        }

        protected virtual PropertySetResult PopulatePropertyLite(WorkflowPropertyType propertyType)
        {
            switch (propertyType?.PrimitiveType)
            {
                case PropertyPrimitiveType.Text:
                    PropertyLiteValue = new PropertyLite()
                    {
                        PropertyTypeId = InstancePropertyTypeId,
                        TextOrChoiceValue = PropertyValue
                    };
                    break;
                case PropertyPrimitiveType.Number:
                    return PopulateNumberPropertyLite();
                case PropertyPrimitiveType.Date:
                    PropertyLiteValue = new PropertyLite
                    {
                        PropertyTypeId = InstancePropertyTypeId,
                        DateValue = PropertyHelper.ParseDateValue(PropertyValue, new TimeProvider())
                    };
                    break;
                case PropertyPrimitiveType.Choice:
                    PropertyLiteValue = new PropertyLite
                    {
                        PropertyTypeId = InstancePropertyTypeId,
                    };
                    if (!ValidValues.Any() && !propertyType.Validate.GetValueOrDefault(false))
                    {
                        PropertyLiteValue.TextOrChoiceValue = PropertyValue;
                    }
                    else
                    {
                        PropertyLiteValue.ChoiceIds.AddRange(ValidValues);
                    }
                    break;
                case PropertyPrimitiveType.User:
                    return new PropertySetResult(InstancePropertyTypeId, ErrorCodes.InvalidArtifactProperty, "Property type is now user, but does not contain user and group change actions");
                default:
                    PropertyLiteValue = new PropertyLite()
                    {
                        PropertyTypeId = InstancePropertyTypeId
                    };
                    break;
            }
            return null;
        }

        private PropertySetResult PopulateNumberPropertyLite()
        {
            if (ValidValues != null && ValidValues.Any())
            {
                return new PropertySetResult(InstancePropertyTypeId, ErrorCodes.InvalidArtifactProperty, 
                    "Property type is now number property. Property change action is currently invalid.");
            }
            if (String.IsNullOrEmpty(PropertyValue))
            {
                PropertyLiteValue = new PropertyLite()
                {
                    PropertyTypeId = InstancePropertyTypeId,
                    NumberValue = null
                };
            }
            else
            {
                decimal value;
                if (
                    !Decimal.TryParse(PropertyValue, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, new NumberFormatInfo(),
                        out value))
                {
                    return new PropertySetResult(InstancePropertyTypeId, ErrorCodes.InvalidArtifactProperty, 
                        "Property type is now number property. Property change action is currently invalid.");
                }
                PropertyLiteValue = new PropertyLite()
                {
                    PropertyTypeId = InstancePropertyTypeId,
                    NumberValue = value
                };
            }
            return null;
        }
    }
}
