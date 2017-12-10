using System.Collections.Generic;
using System.Linq;

namespace AdminStore.Services.Workflow.Validation.Xml
{
    public class WorkflowXmlValidationResult
    {
        public bool HasErrors => Errors.Any();

        private List<WorkflowXmlValidationError> _errors;
        public List<WorkflowXmlValidationError> Errors => _errors ?? (_errors = new List<WorkflowXmlValidationError>());
    }
}
