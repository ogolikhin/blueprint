﻿using System.Collections.Generic;
using System.Threading.Tasks;
using AdminStore.Models.DiagramWorkflow;
using AdminStore.Models.DTO;
using AdminStore.Models.Enums;
using AdminStore.Models.Workflow;
using ServiceLibrary.Models;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Repositories.Files;

namespace AdminStore.Services.Workflow
{
    public interface IWorkflowService
    {
        Task<ImportWorkflowResult> ImportWorkflowAsync(IeWorkflow workflow, string fileName, int userId, string xmlSerError);

        Task<ImportWorkflowResult> UpdateWorkflowViaImport(int userId, int workflowId, IeWorkflow workflow, string fileName = null, string xmlSerError = null, WorkflowMode workflowMode = WorkflowMode.Xml);

        IFileRepository FileRepository { get; set; }

        Task<string> GetImportWorkflowErrorsAsync(string guid, int userId);

        Task<WorkflowDetailsDto> GetWorkflowDetailsAsync(int workflowId);

        Task<int> UpdateWorkflowStatusAsync(StatusUpdate statusUpdate, int workflowId, int userId);

        Task UpdateWorkflowAsync(UpdateWorkflowDto workflowDto, int workflowId, int userId);

        Task<int> DeleteWorkflows(OperationScope body, string search, int sessionUserId);

        Task<IeWorkflow> GetWorkflowExportAsync(int workflowId, WorkflowMode mode);

        Task<int> CreateWorkflow(string name, string description, int userId);

        Task<DWorkflow> GetWorkflowDiagramAsync(int workflowId);

        Task<IEnumerable<PropertyType>> GetWorkflowArtifactTypesProperties(ISet<int> standardArtifactTypeIds);
    }
}
