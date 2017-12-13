using AdminStore.Models.Workflow;
using ServiceLibrary.Models.ProjectMeta;

namespace AdminStore.Services.Workflow.Validation.Data.PropertyValue
{
    public interface IPropertyValueValidator
    {
        void Validate(IePropertyChangeAction action, PropertyType propertyType, WorkflowDataValidationResult result);
    }
}
