using System.Collections.Generic;
using AdminStore.Models.Workflow;
using ServiceLibrary.Models.ProjectMeta;

namespace AdminStore.Services.Workflow
{
    public interface IWorkflowActionPropertyValueValidator
    {
        bool ValidatePropertyValue(IePropertyChangeAction action, PropertyType propertyType,
            ISet<string> validUsers, ISet<string> validGroups, out WorkflowDataValidationErrorCodes? errorCode);
    }
}