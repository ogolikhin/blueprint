using AdminStore.Models.Workflow;

namespace AdminStore.Repositories.Workflow
{
    public interface IWorkflowValidator
    {
        WorkflowValidationResult Validate(IeWorkflow workflow);
    }
}