using System;
using System.Globalization;
using AdminStore.Models.Workflow;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.ProjectMeta;

namespace AdminStore.Services.Workflow.Validation.Data.PropertyValue
{
    public class NumberPropertyValueValidator : PropertyValueValidator
    {
        public override void Validate(IePropertyChangeAction action, PropertyType propertyType, WorkflowDataValidationResult result)
        {
            if (!action.ValidValues.IsEmpty())
            {
                result.Errors.Add(new WorkflowDataValidationError
                {
                    Element = action.PropertyName,
                    ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionNotChoicePropertyValidValuesNotApplicable
                });
                return;
            }

            if (action.UsersGroups != null)
            {
                result.Errors.Add(new WorkflowDataValidationError
                {
                    Element = action.PropertyName,
                    ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionNotUserPropertyUsersGroupsNotApplicable
                });
                return;
            }

            if (!IsPropertyRequired(propertyType.IsRequired.GetValueOrDefault(), action.PropertyValue, true, true))
            {
                result.Errors.Add(new WorkflowDataValidationError
                {
                    Element = action.PropertyName,
                    ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredPropertyValueEmpty
                });
                return;
            }

            decimal numberValue;
            if (!decimal.TryParse(action.PropertyValue, NumberStyles.Number, CultureInfo.InvariantCulture, out numberValue))
            {
                if (!string.IsNullOrWhiteSpace(action.PropertyValue))
                {
                    result.Errors.Add(new WorkflowDataValidationError
                    {
                        Element = action.PropertyName,
                        ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionInvalidNumberFormat
                    });
                    return;
                }
            }

            if (!propertyType.IsValidated.GetValueOrDefault()
                || string.IsNullOrWhiteSpace(action.PropertyValue))
            {
                return;
            }

            var index = action.PropertyValue.IndexOf(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator, StringComparison.Ordinal);
            var decimalPlaces = index == -1 ? 0 : action.PropertyValue.Length - index - 1;
            if (decimalPlaces > propertyType.DecimalPlaces.GetValueOrDefault())
            {
                result.Errors.Add(new WorkflowDataValidationError
                {
                    Element = action.PropertyName,
                    ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionInvalidNumberDecimalPlaces
                });
                return;
            }

            if (numberValue < propertyType.MinNumber.Value || numberValue > propertyType.MaxNumber.Value)
            {
                result.Errors.Add(new WorkflowDataValidationError
                {
                    Element = action.PropertyName,
                    ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionNumberOutOfRange
                });
            }
        }
    }
}
