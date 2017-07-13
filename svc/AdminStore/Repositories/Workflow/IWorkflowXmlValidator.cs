using AdminStore.Models.Workflow;

namespace AdminStore.Repositories.Workflow
{
    public interface IWorkflowXmlValidator
    {
        WorkflowXmlValidationResult Validate(IeWorkflow workflow);
    }
}