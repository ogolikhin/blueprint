using System;
using System.Globalization;
using System.Linq;
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

        protected override PropertySetResult ValidateActionToBeProcessed(IExecutionParameters executionParameters)
        {
            var result = executionParameters.ReuseValidator.ValidateReuseSettings(InstancePropertyTypeId, executionParameters.ReuseItemTemplate);
            if (result != null)
            {
                return result;
            }
            return ValidateProperty(executionParameters);
        }

        private PropertySetResult ValidateProperty(IExecutionParameters executionParameters)
        {
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
                    return PopulateDatePropertyLite();
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

        private PropertySetResult PopulateDatePropertyLite()
        {
            //was Choice property
            if (ValidValues != null && ValidValues.Any())
            {
                return new PropertySetResult(InstancePropertyTypeId, ErrorCodes.InvalidArtifactProperty, "Property type is now date property. Property change action is currently invalid.");
            }

            //is null
            if (string.IsNullOrEmpty(PropertyValue))
            {
                PropertyLiteValue = new PropertyLite
                {
                    PropertyTypeId = InstancePropertyTypeId,
                    DateValue = null
                };
                return null;
            }

            DateTime date;
            try
            {
                date = PropertyHelper.ParseDateValue(PropertyValue, new TimeProvider());
            }
            catch (Exception ex)
            {
                //invalid date format
                return new PropertySetResult(InstancePropertyTypeId, ErrorCodes.InvalidArtifactProperty, $"Property type is now date property. Property change action is currently invalid. {ex.Message}");
            }

            //valid date format
            PropertyLiteValue = new PropertyLite
            {
                PropertyTypeId = InstancePropertyTypeId,
                DateValue = date
            };
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
