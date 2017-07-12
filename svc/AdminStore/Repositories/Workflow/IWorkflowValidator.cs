using System.Collections.Generic;
using System.Threading.Tasks;
using AdminStore.Models;
using AdminStore.Models.Workflow;

namespace AdminStore.Repositories.Workflow
{
    public interface IWorkflowValidator
    {
        WorkflowValidationResult Validate(IeWorkflow workflow);
        Task<WorkflowValidationResult> ValidateData(IeWorkflow workflow, IWorkflowRepository workflowRepository, IUserRepository userRepository);

        HashSet<int> ValidProjectIds { get; }
        HashSet<SqlGroup> ValidGroups { get; }
    }
}