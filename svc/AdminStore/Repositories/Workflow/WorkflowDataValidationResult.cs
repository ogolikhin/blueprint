using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminStore.Models;

namespace AdminStore.Repositories.Workflow
{
    public class WorkflowDataValidationResult
    {
        public bool HasErrors => Errors.Any();

        private List<WorkflowDataValidationError> _errors;
        public List<WorkflowDataValidationError> Errors => _errors ?? (_errors = new List<WorkflowDataValidationError>());

        public HashSet<int> ValidProjectIds { get; set; }
        public HashSet<SqlGroup> ValidGroups { get; set; }
    }

    public class WorkflowDataValidationError
    {
        public string Info { get; set; }
        public WorkflowValidationErrorCodes ErrorCode { get; set; }

    }

}