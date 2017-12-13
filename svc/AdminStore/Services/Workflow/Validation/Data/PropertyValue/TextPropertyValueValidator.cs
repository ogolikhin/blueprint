using AdminStore.Models.Workflow;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.ProjectMeta;

namespace AdminStore.Services.Workflow.Validation.Data.PropertyValue
{
    public class TextPropertyValueValidator : PropertyValueValidator
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
            }
        }
    }
}