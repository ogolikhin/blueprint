﻿using System.Collections.Generic;
using System.Linq;
using AdminStore.Models.Workflow;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.ProjectMeta;

namespace AdminStore.Services.Workflow.Validation.Data.PropertyValue
{
    public class ChoicePropepertyValueValidator : PropertyValueValidator
    {
        private readonly bool _ignoreIds;

        public ChoicePropepertyValueValidator(bool ignoreIds)
        {
            _ignoreIds = ignoreIds;
        }

        public override void Validate(IePropertyChangeAction action, PropertyType propertyType, WorkflowDataValidationResult result)
        {
            if (action.UsersGroups != null)
            {
                result.Errors.Add(new WorkflowDataValidationError
                {
                    Element = action.PropertyName,
                    ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionNotUserPropertyUsersGroupsNotApplicable
                });
                return;
            }

            if (!IsPropertyRequired(propertyType.IsRequired.GetValueOrDefault(), action.PropertyValue, action.ValidValues.IsEmpty(), true))
            {
                result.Errors.Add(new WorkflowDataValidationError
                {
                    Element = action.PropertyName,
                    ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredPropertyValueEmpty
                });
                return;
            }

            if (!propertyType.IsMultipleAllowed.GetValueOrDefault() && action.ValidValues?.Count > 1)
            {
                result.Errors.Add(new WorkflowDataValidationError
                {
                    Element = action.PropertyName,
                    ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionChoicePropertyMultipleValidValuesNotAllowed
                });
                return;
            }

            if (propertyType.IsValidated.GetValueOrDefault()
                && !string.IsNullOrEmpty(action.PropertyValue)
                && action.ValidValues.IsEmpty())
            {
                result.Errors.Add(new WorkflowDataValidationError
                {
                    Element = action.PropertyName,
                    ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionChoiceValueSpecifiedAsNotValidated
                });
                return;
            }

            if (action.ValidValues == null || !action.ValidValues.Any())
            {
                return;
            }

            var validValuesMap = propertyType.ValidValues.ToDictionary(vv => vv.Id, vv => vv.Value);
            var validValueValues = validValuesMap.Values.ToHashSet();

            var processedValidValueIds = new HashSet<int>();
            var processedValidValueValues = new HashSet<string>();

            foreach (var validValue in action.ValidValues)
            {
                // Update Name where Id is present (to null if Id is not found)
                if (!_ignoreIds && validValue.Id.HasValue)
                {
                    string value;
                    if (!validValuesMap.TryGetValue(validValue.Id.Value, out value))
                    {
                        result.Errors.Add(new WorkflowDataValidationError
                        {
                            Element = action.PropertyName,
                            ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionValidValueNotFoundById
                        });
                        return;
                    }

                    if (processedValidValueIds.Contains(validValue.Id.Value))
                    {
                        result.Errors.Add(new WorkflowDataValidationError
                        {
                            Element = action.PropertyName,
                            ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionDuplicateValidValueFound
                        });
                        return;
                    }

                    processedValidValueIds.Add(validValue.Id.Value);

                    validValue.Value = value;
                }

                if (!validValueValues.Contains(validValue.Value))
                {
                    result.Errors.Add(new WorkflowDataValidationError
                    {
                        Element = action.PropertyName,
                        ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionValidValueNotFoundByValue
                    });
                    return;
                }

                if (_ignoreIds)
                {
                    if (processedValidValueValues.Contains(validValue.Value))
                    {
                        result.Errors.Add(new WorkflowDataValidationError
                        {
                            Element = action.PropertyName,
                            ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionDuplicateValidValueFound
                        });
                        return;
                    }

                    processedValidValueValues.Add(validValue.Value);
                }
            }
        }
    }
}
