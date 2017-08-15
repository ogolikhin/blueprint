using System;
using System.Collections.Generic;
using AdminStore.Models.Workflow;
using ServiceLibrary.Models.ProjectMeta;

namespace AdminStore.Services.Workflow
{
    public interface IWorkflowActionPropertyValueValidator
    {
        // For Tuple of validGroup, Item1 - Group Name, Item2 - ProjectId
        bool ValidatePropertyValue(IePropertyChangeAction action, PropertyType propertyType,
            ISet<string> validUsers, ISet<Tuple<string, int?>> validGroups, out WorkflowDataValidationErrorCodes? errorCode);
    }
}