using System.Collections.Generic;
using AdminStore.Services.Workflow.Validation.Data;
using AdminStore.Services.Workflow.Validation.Xml;

namespace AdminStore.Services.Workflow.Validation
{
    public interface IWorkflowValidationErrorBuilder
    {
        string BuildTextXmlErrors(IEnumerable<WorkflowXmlValidationError> errors, string fileName, bool isEditFileMessage = true);

        string BuildTextDataErrors(IEnumerable<WorkflowDataValidationError> errors, string fileName, bool isEditFileMessage = true);

        string BuildTextDataErrors(IEnumerable<WorkflowDataValidationError> errors);

        string BuildTextDiagramErrors(IEnumerable<WorkflowXmlValidationError> errors);
    }
}