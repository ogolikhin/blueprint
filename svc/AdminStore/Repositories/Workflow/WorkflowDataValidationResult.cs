using System.Collections.Generic;
using System.Linq;
using AdminStore.Models;

namespace AdminStore.Repositories.Workflow
{
    public class WorkflowDataValidationResult
    {
        public bool HasErrors => Errors.Any();

        private List<WorkflowDataValidationError> _errors;

        public List<WorkflowDataValidationError> Errors
            => _errors ?? (_errors = new List<WorkflowDataValidationError>());

        public HashSet<int> ValidProjectIds { get; set; }
        public HashSet<SqlGroup> ValidGroups { get; set; }
    }

    public class WorkflowDataValidationError
    {
        public string Info { get; set; }
        public WorkflowDataValidationErrorCodes ErrorCode { get; set; }

    }

    public enum WorkflowDataValidationErrorCodes
    {
        ProjectNotFound,
        GroupsNotFound
    }
}