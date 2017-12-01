﻿using System.Threading.Tasks;
using AdminStore.Models.Workflow;
using ServiceLibrary.Models.ProjectMeta;

namespace AdminStore.Services.Workflow
{
    public interface IWorkflowDataValidator
    {
        Task<WorkflowDataValidationResult> ValidateDataAsync(IeWorkflow workflow);

        Task<WorkflowDataValidationResult> ValidateUpdateDataAsync(IeWorkflow workflow, ProjectTypes standardTypes);
    }
}
