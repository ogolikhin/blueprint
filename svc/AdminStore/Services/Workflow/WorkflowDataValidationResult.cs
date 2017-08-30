using System.Collections.Generic;
using System.Linq;
using ServiceLibrary.Models;
using ServiceLibrary.Models.ProjectMeta;

namespace AdminStore.Services.Workflow
{
    public class WorkflowDataValidationResult
    {
        public bool HasErrors => Errors.Any();

        private List<WorkflowDataValidationError> _errors;

        public List<WorkflowDataValidationError> Errors
            => _errors ?? (_errors = new List<WorkflowDataValidationError>());

        public HashSet<int> ValidProjectIds { get; } = new HashSet<int>();

        public ProjectTypes StandardTypes { get; set; }
        public Dictionary<string, ItemType> StandardArtifactTypeMapByName { get; } = new Dictionary<string, ItemType>();
        public Dictionary<string, PropertyType> StandardPropertyTypeMapByName { get; } = new Dictionary<string, PropertyType>();
        public Dictionary<int, ItemType> StandardArtifactTypeMapById { get; } = new Dictionary<int, ItemType>();
        public Dictionary<int, PropertyType> StandardPropertyTypeMapById { get; } = new Dictionary<int, PropertyType>();
        public List<SqlUser> Users { get; } = new List<SqlUser>();
        public List<SqlGroup> Groups { get; } = new List<SqlGroup>();
    }

    public class WorkflowDataValidationError
    {
        public object Element { get; set; }
        public WorkflowDataValidationErrorCodes ErrorCode { get; set; }

    }

    public enum WorkflowDataValidationErrorCodes
    {
        WorkflowNameNotUnique,
        ProjectByPathNotFound,
        ProjectByIdNotFound,
        ProjectDuplicate,
        InstanceGroupNotFoundByName,
        StandardArtifactTypeNotFoundByName,
        ArtifactTypeInProjectAlreadyAssociatedWithWorkflow,
        PropertyNotFoundByName,
        GenerateChildArtifactsActionArtifactTypeNotFoundByName,
        EmailNotificationActionPropertyTypeNotFoundByName,
        EmailNotificationActionUnacceptablePropertyType,
        PropertyChangeActionPropertyTypeNotFoundByName,
        // Property Value Validation error codes
        PropertyChangeActionRequiredPropertyValueEmpty,
        PropertyChangeActionUserNotFoundByName,
        PropertyChangeActionGroupNotFoundByName,
        PropertyChangeActionChoiceValueSpecifiedAsNotValidated,
        PropertyChangeActionValidValueNotFoundByValue,
        PropertyChangeActionInvalidNumberFormat,
        PropertyChangeActionInvalidNumberDecimalPlaces,
        PropertyChangeActionNumberOutOfRange,
        PropertyChangeActionInvalidDateFormat,
        PropertyChangeActionDateOutOfRange,

        // Update specific errors
        WorkflowActive,
        StateNotFoundByIdInCurrent,
        TransitionEventNotFoundByIdInCurrent,
        PropertyChangeEventNotFoundByIdInCurrent,
        NewArtifactEventNotFoundByIdInCurrent,
        ProjectArtifactTypeNotFoundByIdInCurrent,
        WorkflowNothingToUpdate,
        StandardArtifactTypeNotFoundById,
        PropertyNotFoundById,
        InstanceGroupNotFoundById,
        EmailNotificationActionPropertyTypeNotFoundById,
        PropertyChangeActionPropertyTypeNotFoundById,
        GenerateChildArtifactsActionArtifactTypeNotFoundById,
        PropertyChangeActionValidValueNotFoundById,
        PropertyChangeActionGroupNotFoundById,
        PropertyChangeActionUserNotFoundById

    }
}