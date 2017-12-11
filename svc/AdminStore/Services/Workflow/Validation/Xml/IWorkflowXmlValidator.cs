using AdminStore.Models.Workflow;

namespace AdminStore.Services.Workflow.Validation.Xml
{
    public interface IWorkflowXmlValidator
    {
        WorkflowXmlValidationResult ValidateXml(IeWorkflow workflow);

        WorkflowXmlValidationResult ValidateUpdateXml(IeWorkflow workflow);
    }
}