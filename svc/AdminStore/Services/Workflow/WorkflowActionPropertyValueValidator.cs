using System;
using System.Collections.Generic;
using AdminStore.Models.Workflow;
using ArtifactStore.Helpers;
using ServiceLibrary.Models.ProjectMeta;

namespace AdminStore.Services.Workflow
{
    public class WorkflowActionPropertyValueValidator : IWorkflowActionPropertyValueValidator
    {
        #region Interface Implementation

        public bool ValidatePropertyValue(IePropertyChangeAction action, PropertyType propertyType,
            ISet<string> validUsers, ISet<string> validGroups, out WorkflowDataValidationErrorCodes? errorCode)
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
                    result = ValidateUserPropertyValue(action, propertyType, validUsers, validGroups, out errorCode);
                    break;
                case PropertyPrimitiveType.Choice:
                    result = ValidateChoicePropertyValue(action, propertyType, out errorCode);
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
            errorCode = null;
            if (!ValidateIsPropertyRequired(propertyType.IsRequired.GetValueOrDefault(),
                action.PropertyValue, true, true))
            {
                errorCode = WorkflowDataValidationErrorCodes.PropertyValueEmpty;
                return false;
            }

            return true;
        }

        private static bool ValidateNumberPropertyValue(IePropertyChangeAction action, PropertyType propertyType,
            out WorkflowDataValidationErrorCodes? errorCode)
        {
            errorCode = null;
            if (!ValidateIsPropertyRequired(propertyType.IsRequired.GetValueOrDefault(),
                action.PropertyValue, true, true))
            {
                errorCode = WorkflowDataValidationErrorCodes.PropertyValueEmpty;
                return false;
            }

            // TODO:

            return true;
        }

        private static bool ValidateDatePropertyValue(IePropertyChangeAction action, PropertyType propertyType,
            out WorkflowDataValidationErrorCodes? errorCode)
        {
            errorCode = null;
            if (!ValidateIsPropertyRequired(propertyType.IsRequired.GetValueOrDefault(),
                action.PropertyValue, true, true))
            {
                errorCode = WorkflowDataValidationErrorCodes.PropertyValueEmpty;
                return false;
            }

            // TODO:

            return true;
        }

        private static bool ValidateChoicePropertyValue(IePropertyChangeAction action, PropertyType propertyType,
            out WorkflowDataValidationErrorCodes? errorCode)
        {
            errorCode = null;
            if (!ValidateIsPropertyRequired(propertyType.IsRequired.GetValueOrDefault(),
                action.PropertyValue, action.ValidValues.IsEmpty(), true))
            {
                errorCode = WorkflowDataValidationErrorCodes.PropertyValueEmpty;
                return false;
            }

            // TODO:

            return true;
        }

        private static bool ValidateUserPropertyValue(IePropertyChangeAction action, PropertyType propertyType,
            ISet<string> validUsers, ISet<string> validGroups, out WorkflowDataValidationErrorCodes? errorCode)
        {
            errorCode = null;
            if (!ValidateIsPropertyRequired(propertyType.IsRequired.GetValueOrDefault(),
                action.PropertyValue, true, action.UsersGroups.IsEmpty()))
            {
                errorCode = WorkflowDataValidationErrorCodes.PropertyValueEmpty;
                return false;
            }

            // TODO:

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