using System.Collections.Generic;
using AdminStore.Models.Workflow;
using ServiceLibrary.Models;
using ServiceLibrary.Models.ProjectMeta;

namespace AdminStore.Services.Workflow
{
    public interface IWorkflowActionPropertyValueValidator
    {
        // For Tuple of validGroup, Item1 - Group Name, Item2 - ProjectId
        bool ValidatePropertyValue(IePropertyChangeAction action, PropertyType propertyType,
            IList<SqlUser> users, IList<SqlGroup> groups, bool ignoreIds,
            out WorkflowDataValidationErrorCodes? errorCode);
    }
}