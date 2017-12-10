using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AdminStore.Models.Workflow;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Services.Workflow.Validation.Data
{
    public class PropertyValueValidator : IPropertyValueValidator
    {
        #region Interface Implementation

        public void Validate(
            IePropertyChangeAction action,
            PropertyType propertyType,
            IList<SqlUser> users,
            IList<SqlGroup> groups,
            bool ignoreIds,
            WorkflowDataValidationResult result)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (propertyType == null)
            {
                throw new ArgumentNullException(nameof(propertyType));
            }

            switch (propertyType.PrimitiveType)
            {
                case PropertyPrimitiveType.Text:
                    ValidateTextPropertyValue(action, propertyType, result);
                    break;

                case PropertyPrimitiveType.Number:
                    ValidateNumberPropertyValue(action, propertyType, result);
                    break;

                case PropertyPrimitiveType.Date:
                    ValidateDatePropertyValue(action, propertyType, result);
                    break;

                case PropertyPrimitiveType.User:
                    ValidateUserPropertyValue(action, propertyType, users, groups, ignoreIds, result);
                    break;

                case PropertyPrimitiveType.Choice:
                    ValidateChoicePropertyValue(action, propertyType, ignoreIds, result);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(propertyType.PrimitiveType));
            }
        }

        #endregion

        #region Private Methods

        private static void ValidateTextPropertyValue(IePropertyChangeAction action, PropertyType propertyType, WorkflowDataValidationResult result)
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

            if (!ValidateIsPropertyRequired(propertyType.IsRequired.GetValueOrDefault(), action.PropertyValue, true, true))
            {
                result.Errors.Add(new WorkflowDataValidationError
                {
                    Element = action.PropertyName,
                    ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredPropertyValueEmpty
                });
            }
        }

        private static void ValidateNumberPropertyValue(IePropertyChangeAction action, PropertyType propertyType, WorkflowDataValidationResult result)
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

            if (!ValidateIsPropertyRequired(propertyType.IsRequired.GetValueOrDefault(), action.PropertyValue, true, true))
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

        private static void ValidateDatePropertyValue(IePropertyChangeAction action, PropertyType propertyType, WorkflowDataValidationResult result)
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

            if (!ValidateIsPropertyRequired(propertyType.IsRequired.GetValueOrDefault(), action.PropertyValue, true, true))
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

        private static void ValidateChoicePropertyValue(IePropertyChangeAction action, PropertyType propertyType, bool ignoreIds, WorkflowDataValidationResult result)
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

            if (!ValidateIsPropertyRequired(propertyType.IsRequired.GetValueOrDefault(), action.PropertyValue, action.ValidValues.IsEmpty(), true))
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

            foreach (var validValue in action.ValidValues)
            {
                // Update Name where Id is present (to null if Id is not found)
                if (!ignoreIds && validValue.Id.HasValue)
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
            }
        }

        private static void ValidateUserPropertyValue(IePropertyChangeAction action, PropertyType propertyType, IEnumerable<SqlUser> users, IEnumerable<SqlGroup> groups, bool ignoreIds, WorkflowDataValidationResult result)
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

            if (action.PropertyValue != null)
            {
                result.Errors.Add(new WorkflowDataValidationError
                {
                    Element = action.PropertyName,
                    ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredUserPropertyPropertyValueNotApplicable
                });
                return;
            }

            var usersGroups = action.UsersGroups?.UsersGroups;
            var isUsersGroupsEmpty = action.UsersGroups == null || usersGroups.IsEmpty() && !action.UsersGroups.IncludeCurrentUser.GetValueOrDefault();

            if (!ValidateIsPropertyRequired(propertyType.IsRequired.GetValueOrDefault(), null, true, isUsersGroupsEmpty))
            {
                result.Errors.Add(new WorkflowDataValidationError
                {
                    Element = action.PropertyName,
                    ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredPropertyValueEmpty
                });
                return;
            }

            if (usersGroups.IsEmpty())
            {
                return;
            }

            var usersMap = users.ToDictionary(u => u.UserId, u => u.Login);
            var groupsMap = groups.ToDictionary(g => g.GroupId, g => Tuple.Create(g.Name, g.ProjectId));
            var userNames = usersMap.Values.ToHashSet();
            var groupNames = groupsMap.Values.ToHashSet();

            if (usersGroups == null || !usersGroups.Any())
            {
                return;
            }

            foreach (var userGroup in usersGroups)
            {
                if (userGroup.IsGroup.GetValueOrDefault())
                {
                    // Update Name where Id is present (to null if Id is not found)
                    if (!ignoreIds && userGroup.Id.HasValue)
                    {
                        Tuple<string, int?> nameProject;
                        if (!groupsMap.TryGetValue(userGroup.Id.Value, out nameProject))
                        {
                            result.Errors.Add(new WorkflowDataValidationError
                            {
                                Element = action.PropertyName,
                                ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionGroupNotFoundById
                            });
                            return;
                        }

                        userGroup.Name = nameProject.Item1;
                    }

                    if (!groupNames.Contains(Tuple.Create(userGroup.Name, userGroup.GroupProjectId)))
                    {
                        result.Errors.Add(new WorkflowDataValidationError
                        {
                            Element = action.PropertyName,
                            ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionGroupNotFoundByName
                        });
                        return;
                    }
                }
                else
                {
                    // Update Name where Id is present (to null if Id is not found)
                    if (!ignoreIds && userGroup.Id.HasValue)
                    {
                        string name;
                        if (!usersMap.TryGetValue(userGroup.Id.Value, out name))
                        {
                            result.Errors.Add(new WorkflowDataValidationError
                            {
                                Element = action.PropertyName,
                                ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionUserNotFoundById
                            });
                            return;
                        }

                        userGroup.Name = name;
                    }

                    if (!userNames.Contains(userGroup.Name))
                    {
                        result.Errors.Add(new WorkflowDataValidationError
                        {
                            Element = action.PropertyName,
                            ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionUserNotFoundByName
                        });
                        return;
                    }
                }
            }
        }

        private static bool ValidateIsPropertyRequired(bool isRequired, string value, bool isChoicesEmpty, bool isUsersGroupsEmpty)
        {
            return !(isRequired && string.IsNullOrWhiteSpace(value) && isChoicesEmpty && isUsersGroupsEmpty);
        }

        #endregion
    }
}
