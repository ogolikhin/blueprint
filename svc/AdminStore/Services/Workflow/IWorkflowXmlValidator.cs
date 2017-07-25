using AdminStore.Models.Workflow;

namespace AdminStore.Services.Workflow
{
    public interface IWorkflowXmlValidator
    {
        WorkflowXmlValidationResult ValidateXml(IeWorkflow workflow);
    }
}