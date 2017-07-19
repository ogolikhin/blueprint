using System.Collections.Generic;
using System.Linq;
using AdminStore.Models;

namespace AdminStore.Services.Workflow
{
    public class WorkflowDataValidationResult
    {
        public bool HasErrors => Errors.Any();

        private List<WorkflowDataValidationError> _errors;

        public List<WorkflowDataValidationError> Errors
            => _errors ?? (_errors = new List<WorkflowDataValidationError>());

        public HashSet<int> ValidProjectIds { get; } = new HashSet<int>();
        public HashSet<string> ValidArtifactTypeNames { get; } = new HashSet<string>();
        public HashSet<SqlGroup> ValidGroups { get; } = new HashSet<SqlGroup>();
    }

    public class WorkflowDataValidationError
    {
        public string Info { get; set; }
        public WorkflowDataValidationErrorCodes ErrorCode { get; set; }

    }

    public enum WorkflowDataValidationErrorCodes
    {
        ProjectNotFound,
        GroupsNotFound,
        ArtifactTypeNotFoundInProject,
        ArtifactTypeAlreadyAssociatedWithWorkflow,
        ArtifactTypeNotUsedInProject
    }
}