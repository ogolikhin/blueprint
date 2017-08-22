using System.Collections.Generic;

namespace AdminStore.Services.Workflow
{
    public interface IWorkflowValidationErrorBuilder
    {
        string BuildTextXmlErrors(IEnumerable<WorkflowXmlValidationError> errors, string fileName, bool isEditFileMessage = true);

        string BuildTextDataErrors(IEnumerable<WorkflowDataValidationError> errors, string fileName, bool isEditFileMessage = true);
    }
}