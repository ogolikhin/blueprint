using AdminStore.Models.Workflow;
using ServiceLibrary.Models.ProjectMeta;

namespace AdminStore.Services.Workflow.Validation.Data.PropertyValue
{
    public abstract class PropertyValueValidator : IPropertyValueValidator
    {
        public abstract void Validate(IePropertyChangeAction action, PropertyType propertyType, WorkflowDataValidationResult result);

        protected static bool IsPropertyRequired(bool isRequired, string value, bool isChoicesEmpty, bool isUsersGroupsEmpty)
        {
            return !(isRequired && string.IsNullOrWhiteSpace(value) && isChoicesEmpty && isUsersGroupsEmpty);
        }
    }
}
