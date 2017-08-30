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
                throw new ConflictException(
                    I18NHelper.FormatInvariant("Property type id {0} is not associated with specified artifact type", InstancePropertyTypeId));
            }

            PopulatePropertyLite(dPropertyType);

            return executionParameters.Validators.Select(v => v.Validate(PropertyLiteValue, executionParameters.CustomPropertyTypes, executionParameters.ValidationContext)).FirstOrDefault(r => r != null);
        }

        protected virtual void PopulatePropertyLite(WorkflowPropertyType propertyType)
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
                case PropertyPrimitiveType.Date:
                    PropertyLiteValue = new PropertyLite
                    {
                        PropertyTypeId = InstancePropertyTypeId,
                        DateValue = ParseDateValue(PropertyValue, new TimeProvider())
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
                default:
                    PropertyLiteValue = new PropertyLite()
                    {
                        PropertyTypeId = InstancePropertyTypeId
                    };
                    break;
            }
        }

        public static DateTime ParseDateValue(string dateValue, ITimeProvider timeProvider)
        {
            //specific date
            const string dateFormat = WorkflowConstants.Iso8601DateFormat;
            DateTime date;
            if (DateTime.TryParseExact(dateValue, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return date;
            }

            //today + days
            int days;
            if (int.TryParse(dateValue, out days))
            {
                return timeProvider.Today.AddDays(days);
            }

            throw new FormatException($"Invalid date value: {dateValue}. The date format must be {dateFormat}, or an integer.");
        }
    }
}
