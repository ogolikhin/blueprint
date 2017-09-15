using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AdminStore.Models.Workflow;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Services.Workflow
{
    public class WorkflowActionPropertyValueValidator : IWorkflowActionPropertyValueValidator
    {
        #region Interface Implementation

        public bool ValidatePropertyValue(IePropertyChangeAction action, PropertyType propertyType,
            IList<SqlUser> users, IList<SqlGroup> groups, bool ignoreIds,
            out WorkflowDataValidationErrorCodes? errorCode)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (propertyType == null)
            {
                throw new ArgumentNullException(nameof(propertyType));
            }

            bool result;
            switch (propertyType.PrimitiveType)
            {
                case PropertyPrimitiveType.Text:
                    result = ValidateTextPropertyValue(action, propertyType, out errorCode);
                    break;
                case PropertyPrimitiveType.Number:
                    result = ValidateNumberPropertyValue(action, propertyType, out errorCode);
                    break;
                case PropertyPrimitiveType.Date:
                    result = ValidateDatePropertyValue(action, propertyType, out errorCode);
                    break;
                case PropertyPrimitiveType.User:
                    result = ValidateUserPropertyValue(action, propertyType, users, groups, ignoreIds, out errorCode);
                    break;
                case PropertyPrimitiveType.Choice:
                    result = ValidateChoicePropertyValue(action, propertyType, ignoreIds, out errorCode);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(propertyType.PrimitiveType));
            }

            return result;
        }
        
        #endregion

        #region Private Methods

        private static bool ValidateTextPropertyValue(IePropertyChangeAction action, PropertyType propertyType,
            out WorkflowDataValidationErrorCodes? errorCode)
        {
            if (!action.ValidValues.IsEmpty())
            {
                errorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionNotChoicePropertyValidValuesNotApplicable;
                return false;
            }

            if (action.UsersGroups != null)
            {
                errorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionNotUserPropertyUsersGroupsNotApplicable;
                return false;
            }

            errorCode = null;
            if (!ValidateIsPropertyRequired(propertyType.IsRequired.GetValueOrDefault(),
                action.PropertyValue, true, true))
            {
                errorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredPropertyValueEmpty;
                return false;
            }

            return true;
        }

        private static bool ValidateNumberPropertyValue(IePropertyChangeAction action, PropertyType propertyType,
            out WorkflowDataValidationErrorCodes? errorCode)
        {
            if (!action.ValidValues.IsEmpty())
            {
                errorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionNotChoicePropertyValidValuesNotApplicable;
                return false;
            }

            if (action.UsersGroups != null)
            {
                errorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionNotUserPropertyUsersGroupsNotApplicable;
                return false;
            }

            errorCode = null;
            if (!ValidateIsPropertyRequired(propertyType.IsRequired.GetValueOrDefault(),
                action.PropertyValue, true, true))
            {
                errorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredPropertyValueEmpty;
                return false;
            }

            decimal numberValue;
            if (!decimal.TryParse(action.PropertyValue, out numberValue))
            {
                if(!string.IsNullOrWhiteSpace(action.PropertyValue))
                {
                    errorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionInvalidNumberFormat;
                    return false;
                }
            }

            if (!propertyType.IsValidated.GetValueOrDefault()
                || string.IsNullOrWhiteSpace(action.PropertyValue))
            {
                return true;
            }

            var index = action.PropertyValue.IndexOf(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator, StringComparison.Ordinal);
            var decimalPlaces = (index == -1) ? 0 : action.PropertyValue.Length - index - 1;
            if (decimalPlaces > propertyType.DecimalPlaces.GetValueOrDefault())
            {
                errorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionInvalidNumberDecimalPlaces;
                return false;
            }

            if (numberValue < propertyType.MinNumber.Value
                || numberValue > propertyType.MaxNumber.Value)
            {
                errorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionNumberOutOfRange;
                return false;
            }

            return true;
        }

        private static bool ValidateDatePropertyValue(IePropertyChangeAction action, PropertyType propertyType,
            out WorkflowDataValidationErrorCodes? errorCode)
        {
            if (!action.ValidValues.IsEmpty())
            {
                errorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionNotChoicePropertyValidValuesNotApplicable;
                return false;
            }

            if (action.UsersGroups != null)
            {
                errorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionNotUserPropertyUsersGroupsNotApplicable;
                return false;
            }

            errorCode = null;
            if (!ValidateIsPropertyRequired(propertyType.IsRequired.GetValueOrDefault(),
                action.PropertyValue, true, true))
            {
                errorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredPropertyValueEmpty;
                return false;
            }

            // For "Today+/-{the number of days)" date format we do not validate Min and Max constraints.
            int relativeToTodayDays;
            if (int.TryParse(action.PropertyValue, out relativeToTodayDays))
            {
                return true;
            }

            DateTime dateValue;
            if (!DateTime.TryParseExact(action.PropertyValue, WorkflowConstants.Iso8601DateFormat,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out dateValue))
            {
                if (!string.IsNullOrWhiteSpace(action.PropertyValue))
                {
                    errorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionInvalidDateFormat;
                    return false;
                }
            }

            if (!propertyType.IsValidated.GetValueOrDefault()
                || string.IsNullOrWhiteSpace(action.PropertyValue))
            {
                return true;
            }

            if (dateValue.Date < propertyType.MinDate.Value.Date
                || dateValue.Date > propertyType.MaxDate.Value.Date)
            {
                errorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionDateOutOfRange;
                return false;
            }

            return true;
        }

        private static bool ValidateChoicePropertyValue(IePropertyChangeAction action, PropertyType propertyType,
            bool ignoreIds, out WorkflowDataValidationErrorCodes? errorCode)
        {
            if (action.UsersGroups != null)
            {
                errorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionNotUserPropertyUsersGroupsNotApplicable;
                return false;
            }

            errorCode = null;
            if (!ValidateIsPropertyRequired(propertyType.IsRequired.GetValueOrDefault(),
                action.PropertyValue, action.ValidValues.IsEmpty(), true))
            {
                errorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredPropertyValueEmpty;
                return false;
            }

            if (!propertyType.IsMultipleAllowed.GetValueOrDefault() && action.ValidValues?.Count > 1)
            {
                errorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionChoicePropertyMultipleValidValuesNotAllowed;
                return false;
            }

            if (propertyType.IsValidated.GetValueOrDefault()
                && !string.IsNullOrEmpty(action.PropertyValue)
                && action.ValidValues.IsEmpty())
            {
                errorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionChoiceValueSpecifiedAsNotValidated;
                return false;
            }

            if (action.ValidValues.IsEmpty())
            {
                return true;
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
                        errorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionValidValueNotFoundById;
                        return false;
                    }
                    validValue.Value = value;
                }

                if (!validValueValues.Contains(validValue.Value))
                {
                    errorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionValidValueNotFoundByValue;
                    return false;
                }
            }

            return true;
        }

        private static bool ValidateUserPropertyValue(IePropertyChangeAction action, PropertyType propertyType,
            IList<SqlUser> users, IList<SqlGroup> groups, bool ignoreIds, out WorkflowDataValidationErrorCodes? errorCode)
        {
            if (!action.ValidValues.IsEmpty())
            {
                errorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionNotChoicePropertyValidValuesNotApplicable;
                return false;
            }

            if (action.PropertyValue != null)
            {
                errorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredUserPropertyPropertyValueNotApplicable;
                return false;
            }

            var usersGroups = action.UsersGroups?.UsersGroups;
            errorCode = null;
            if (!ValidateIsPropertyRequired(propertyType.IsRequired.GetValueOrDefault(),
                null, true, action.UsersGroups == null
                || (usersGroups.IsEmpty() && !action.UsersGroups.IncludeCurrentUser.GetValueOrDefault())))
            {
                errorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredPropertyValueEmpty;
                return false;
            }

            if (usersGroups.IsEmpty())
            {
                return true;
            }

            var usersMap = users.ToDictionary(u => u.UserId, u => u.Login);
            var groupsMap = groups.ToDictionary(g => g.GroupId, g => Tuple.Create(g.Name, g.ProjectId));
            var userNames = usersMap.Values.ToHashSet();
            var groupNames = groupsMap.Values.ToHashSet();

            if (usersGroups.IsEmpty())
            {
                return true;
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
                            errorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionGroupNotFoundById;
                            return false;
                        }
                        userGroup.Name = nameProject.Item1;
                    }

                    if (!groupNames.Contains(Tuple.Create(userGroup.Name, userGroup.GroupProjectId)))
                    {
                        errorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionGroupNotFoundByName;
                        return false;
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
                            errorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionUserNotFoundById;
                            return false;
                        }
                        userGroup.Name = name;
                    }

                    if (!userNames.Contains(userGroup.Name))
                    {
                        errorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionUserNotFoundByName;
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool ValidateIsPropertyRequired(bool isRequired, string value, bool isChoicesEmpty,
            bool isUsersGroupsEmpty)
        {
            return !(isRequired && string.IsNullOrWhiteSpace(value) && isChoicesEmpty && isUsersGroupsEmpty);
        }

        #endregion



    }
}