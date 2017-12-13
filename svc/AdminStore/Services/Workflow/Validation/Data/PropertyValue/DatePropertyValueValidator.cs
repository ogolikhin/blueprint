﻿using System;
using System.Globalization;
using AdminStore.Models.Workflow;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Services.Workflow.Validation.Data.PropertyValue
{
    public class DatePropertyValueValidator : PropertyValueValidator
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

            // For "Today+/-{the number of days)" date format we do not validate Min and Max constraints.
            int relativeToTodayDays;
            if (int.TryParse(action.PropertyValue, out relativeToTodayDays))
            {
                return;
            }

            DateTime dateValue;
            if (!DateTime.TryParseExact(action.PropertyValue, WorkflowConstants.Iso8601DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateValue))
            {
                if (!string.IsNullOrWhiteSpace(action.PropertyValue))
                {
                    result.Errors.Add(new WorkflowDataValidationError
                    {
                        Element = action.PropertyName,
                        ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionInvalidDateFormat
                    });
                    return;
                }
            }

            if (!propertyType.IsValidated.GetValueOrDefault() || string.IsNullOrWhiteSpace(action.PropertyValue))
            {
                return;
            }

            if (dateValue.Date < propertyType.MinDate.Value.Date || dateValue.Date > propertyType.MaxDate.Value.Date)
            {
                result.Errors.Add(new WorkflowDataValidationError
                {
                    Element = action.PropertyName,
                    ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionDateOutOfRange
                });
            }
        }
    }
}
