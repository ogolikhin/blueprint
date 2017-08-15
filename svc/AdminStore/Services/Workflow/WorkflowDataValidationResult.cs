using System.Collections.Generic;
using System.Linq;
using AdminStore.Models;
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
        public Dictionary<string, ItemType> StandardArtifactTypeMap { get; } = new Dictionary<string, ItemType>();
        public Dictionary<string, PropertyType> StandardPropertyTypeMap { get; } = new Dictionary<string, PropertyType>();
        public HashSet<SqlUser> Users { get; } = new HashSet<SqlUser>();
        public HashSet<SqlGroup> Groups { get; } = new HashSet<SqlGroup>();
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
        InstanceGroupNotFound,
        StandardArtifactTypeNotFound,
        ArtifactTypeInProjectAlreadyAssociatedWithWorkflow,
        PropertyNotFound,
        GenerateChildArtifactsActionArtifactTypeNotFound,
        EmailNotificationActionPropertyTypeNotFound,
        PropertyChangeActionPropertyTypeNotFound,
        // Property Value Validation error codes
        PropertyChangeActionRequiredPropertyValueEmpty,
        PropertyChangeActionUserOrGroupNotSpecified,
        PropertyChangeActionUserNotFound,
        PropertyChangeActionGroupNotFound,
        PropertyChangeActionChoiceValueSpecifiedAsNotValidated,
        PropertyChangeActionValidValueNotSpecified,
        PropertyChangeActionValidValueNotFound,
        PropertyChangeActionInvalidNumberFormat,
        PropertyChangeActionInvalidNumberDecimalPlaces,
        PropertyChangeActionNumberOutOfRange,
        PropertyChangeActionInvalidDateFormat,
        PropertyChangeActionDateOutOfRange
    }
}