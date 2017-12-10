using System.Collections.Generic;
using AdminStore.Models.Workflow;
using ServiceLibrary.Models;
using ServiceLibrary.Models.ProjectMeta;

namespace AdminStore.Services.Workflow.Validation.Data
{
    public interface IPropertyValueValidator
    {
        void Validate(IePropertyChangeAction action, PropertyType propertyType, IList<SqlUser> users, IList<SqlGroup> groups, bool ignoreIds,  WorkflowDataValidationResult result);
    }
}
