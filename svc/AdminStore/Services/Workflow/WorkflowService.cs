﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdminStore.Models.Workflow;
using AdminStore.Repositories;
using AdminStore.Repositories.Workflow;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories.Files;
using File = ServiceLibrary.Models.Files.File;

namespace AdminStore.Services.Workflow
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IWorkflowValidator _workflowValidator;
        private readonly IUserRepository _userRepository;

        private const string WorkflowImportErrorsFile = "$workflow_import_errors$.txt";

        public WorkflowService()
            : this(new WorkflowRepository(), new WorkflowValidator(), new SqlUserRepository())
        {
        }

        public WorkflowService(IWorkflowRepository workflowRepository, IWorkflowValidator workflowValidator, IUserRepository userRepository)
        {
            _workflowRepository = workflowRepository;
            _workflowValidator = workflowValidator;
            _userRepository = userRepository;
        }

        public IFileRepository FileRepository
        {
            get { return _workflowRepository.FileRepository; }
            set { _workflowRepository.FileRepository = value; }
        }

        public async Task<string> GetImportWorkflowErrorsAsync(string guid, int userId)
        {
            await VerifyUserRole(userId);
            VerifyWorkflowFeature();

            File errorsFile = null;
            try
            {
                errorsFile = await FileRepository.GetFileAsync(Guid.Parse(guid));
            }
            catch (FormatException ex)
            {
                throw new BadRequestException(ex.Message, ErrorCodes.BadRequest);
            }
            catch (ResourceNotFoundException)
            {
            }
            // Use the name convention for the workflow import error file for security reasons
            // in order not to provided access to other files in the file store.
            if (errorsFile == null || !WorkflowImportErrorsFile.Equals(errorsFile.Info.Name))
            {
                throw new ResourceNotFoundException(I18NHelper.FormatInvariant(
                    "The workflow import errors for GUID={0} are not found.", guid),
                    ErrorCodes.ResourceNotFound);
            }
            using (var reader = new StreamReader(errorsFile.ContentStream))
            {
                return reader.ReadToEnd();
            }
        }

        public async Task<ImportWorkflowResult> ImportWorkflowAsync(IeWorkflow workflow, string fileName, int userId)
        {
            if (workflow == null)
            {
                throw new NullReferenceException(nameof(workflow));
            }

            await VerifyUserRole(userId);
            VerifyWorkflowFeature();

            var importResult = new ImportWorkflowResult();

            var validationResult = _workflowValidator.Validate(workflow);
            if (validationResult.HasErrors)
            {
                // TODO: Create a text file and save it to the file store.
                // TODO: The name convention for the error file "$workflow_import_errors$.txt".
                // TODO: The name convention should be checked when the errors are requested by the client. 
                // TODO: Return guid of the errors file.

                var textErrors = GetValidationErrorsText(validationResult.Errors);
                var guid = await UploadErrorsToFileStore(textErrors);

                importResult.ErrorsGuid = guid;
                importResult.ResultCode = ImportWorkflowResultCodes.InvalidModel;
                return importResult;
            }

            SqlWorkflow newWorkflow = null;

            Func<IDbTransaction, Task> action = async transaction =>
            {
                var publishRevision = await _workflowRepository.CreateRevisionInTransactionAsync(transaction, userId, "Workflow import.");
                var duplicateNames = await _workflowRepository.CheckLiveWorkflowsForNameUniqueness(transaction, new[] { workflow.Name });
                if (duplicateNames.Any())
                {
                    // TODO: Create a text file and save it to the file store, see TODO above.
                    importResult.ErrorsGuid = "temp_guid";
                    importResult.ResultCode = ImportWorkflowResultCodes.Conflict;
                    return;
                }

                var importParams = new SqlWorkflow
                {
                    Name = workflow.Name,
                    Description = workflow.Description,
                    Active = false // imported workflows are inactive. Users need explicitly activate workflows via UI.
                };
                newWorkflow = (await _workflowRepository.CreateWorkflowsAsync(new[] { importParams }, publishRevision, transaction)).FirstOrDefault();

                if (newWorkflow != null)
                {
                    await ImportWorkflowComponentsAsync(workflow, newWorkflow, publishRevision, transaction);

                    importResult.ResultCode = ImportWorkflowResultCodes.Ok;
                }

            };

            await _workflowRepository.RunInTransactionAsync(action);
            importResult.WorkflowId = newWorkflow?.WorkflowId;

            return importResult;

        }

        public async Task<WorkflowDto> GetWorkflowDetailsAsync(int workflowId)
        {
            var workflowDetails = await _workflowRepository.GetWorkflowDetailsAsync(workflowId);
            if (workflowDetails == null)
            {
                throw new ResourceNotFoundException(ErrorMessages.WorkflowNotExist, ErrorCodes.ResourceNotFound);
            }

            var workflowDto = new WorkflowDto {Name = workflowDetails.Name, Description = workflowDetails.Description, Status = workflowDetails.Active, WorkflowId = workflowDetails.WorkflowId,
                VersionId = workflowDetails.VersionId}; 

            var workflowProjectsAndArtifactTypes = (await _workflowRepository.GetWorkflowArtifactTypesAndProjectsAsync(workflowId)).ToList();

            workflowDto.Projects = workflowProjectsAndArtifactTypes.Select(e => new WorkflowProjectDto {Id = e.ProjectId, Name = e.ProjectName}).Distinct().ToList();
            workflowDto.ArtifactTypes = workflowProjectsAndArtifactTypes.Select(e => new WorkflowArtifactTypeDto {Name = e.ArtifactName}).Distinct().ToList();

            return workflowDto;
        }


        public async Task UpdateWorkflowStatusAsync(WorkflowDto workflowDto, int workflowId, int userId)
        {
            var existingWorkflow = await _workflowRepository.GetWorkflowDetailsAsync(workflowId);
            if (existingWorkflow == null)
            {
                throw new ResourceNotFoundException(ErrorMessages.WorkflowNotExist, ErrorCodes.ResourceNotFound);
            }

            if (existingWorkflow.VersionId != workflowDto.VersionId)
            {
                throw new ConflictException(ErrorMessages.WorkflowVersionsNotEqual, ErrorCodes.Conflict);
            }
            var workflows = new List<SqlWorkflow> {new SqlWorkflow {Name = existingWorkflow.Name, Description = existingWorkflow.Description, Active = workflowDto.Status, WorkflowId = workflowId} };

            Func<IDbTransaction, Task> action = async transaction =>
            {
                var publishRevision = await _workflowRepository.CreateRevisionInTransactionAsync(transaction, userId, $"Updating the workflow with id {workflowId}.");
                if (publishRevision < 1)
                {
                    throw new ArgumentException(I18NHelper.FormatInvariant("{0} is less than 1.", nameof(publishRevision)));
                }
               
                var updatedWorkflows = await _workflowRepository.UpdateWorkflows(workflows, publishRevision, transaction);

                var updatedWorkflowsCount = updatedWorkflows.Count();
                var neededCountWorkflows = 1;

                if (updatedWorkflowsCount != neededCountWorkflows)
                {
                    throw new BadRequestException(ErrorMessages.WorkflowWasNotUpdated, ErrorCodes.BadRequest);
                }
            };
            await _workflowRepository.RunInTransactionAsync(action);
        }

        private async Task ImportWorkflowComponentsAsync(IeWorkflow workflow, SqlWorkflow newWorkflow, int publishRevision, IDbTransaction transaction)
        {
            var importStateParams = new List<SqlState>();

            float orderIndex = 0;
            workflow.States.ForEach(state =>
            {

                importStateParams.Add(new SqlState
                {
                    Name = state.Name,
                    Description = state.Description,
                    WorkflowId = newWorkflow.WorkflowId,
                    Default = state.IsInitial.HasValue && state.IsInitial.Value,
                    OrderIndex = orderIndex
                });
                orderIndex += 10;
            });
            var newStates = await _workflowRepository.CreateWorkflowStatesAsync(importStateParams, publishRevision, transaction);

            if (newStates != null)
            {
                await ImportWorkflowTransitions(workflow, newWorkflow, publishRevision, transaction, newStates);
            }

            Dictionary<int, string> projectPaths = new Dictionary<int, string>();
            HashSet<string> projectPathsToLookup = new HashSet<string>();
            workflow.Projects.ForEach(project =>
            {
                if (project.Id.HasValue)
                {
                    projectPaths[project.Id.Value] = project.Path;
                }
                else
                {
                    if (!string.IsNullOrEmpty(project.Path))
                    {
                        projectPathsToLookup.Add(project.Path);
                    }
                }
            });

            if (projectPathsToLookup.Any())
            {
                //look up ID of projects that have no ID provided
                foreach (var sqlProjectPathPair in await _workflowRepository.GetProjectIdsByProjectPaths(projectPathsToLookup))
                {
                    projectPaths[sqlProjectPathPair.ProjectId] = sqlProjectPathPair.ProjectPath;
                }
            }

            if (projectPaths.Count != workflow.Projects.Count)
            {
                //generate a list of all projects in the workflow who are either missing from id list or were not looked up by path
                var listOfBadProjects = string.Join(",", workflow.Projects
                    .Where(proj => projectPaths.All(
                        path => proj.Id.HasValue
                            ? path.Key != proj.Id.Value
                            : !path.Value.Equals(proj.Path))
                    ).Select(proj => proj.Id?.ToString() ?? proj.Path));
                throw new ConflictException($"The following projects could not be found: {listOfBadProjects}");
            }

            await _workflowRepository.CreateWorkflowArtifactAssociationsAsync(workflow.ArtifactTypes.Select(at => at.Name),
                projectPaths.Select(p => p.Key), newWorkflow.WorkflowId, publishRevision, transaction);
        }

        private async Task ImportWorkflowTransitions(IeWorkflow workflow, SqlWorkflow newWorkflow, int publishRevision,
            IDbTransaction transaction, IEnumerable<SqlState> newStates)
        {
            var newStatesArray = newStates.ToArray();
            var importTriggersParams = new List<SqlTrigger>();
            HashSet<string> listOfAllGroups = new HashSet<string>();
            workflow.Transitions.ForEach(transition =>
            {
                transition.PermissionGroups.ForEach(group =>
                {
                    if (!listOfAllGroups.Contains(group.Name))
                    {
                        listOfAllGroups.Add(group.Name);
                    }
                });
            });
            var existingGroupNames = (await _userRepository.GetExistingInstanceGroupsByNames(listOfAllGroups)).ToArray();
            if (existingGroupNames.Length != listOfAllGroups.Count)
            {
                var listOfBadGroups = string.Join(",", listOfAllGroups.Where(
                        li => existingGroupNames.All(g => g.Name != li)
                    ));
                throw new ConflictException($"The following groups were not found: {listOfBadGroups}");
            }

            workflow.Transitions.ForEach(transition =>
            {
                importTriggersParams.Add(new SqlTrigger
                {
                    Name = transition.Name,
                    Description = transition.Description,
                    WorkflowId = newWorkflow.WorkflowId,
                    Type = DTriggerType.Transition,
                    Permissions = SerializationHelper.ToXml(new XmlTriggerPermissions
                    {
                        Skip = "0",
                        GroupIds = transition.PermissionGroups.Select(pg => existingGroupNames.First(p => p.Name == pg.Name).GroupId).ToList()
                    }),
                    Validations = null,
                    Actions = null,
                    WorkflowState1Id = newStatesArray.FirstOrDefault(s => s.Name.Equals(transition.FromState))?.WorkflowStateId,
                    WorkflowState2Id = newStatesArray.FirstOrDefault(s => s.Name.Equals(transition.ToState))?.WorkflowStateId,
                    PropertyTypeId = null
                });
            });
            await _workflowRepository.CreateWorkflowTriggersAsync(importTriggersParams, publishRevision, transaction);
        }

        private async Task VerifyUserRole(int userId)
        {
            var user = await _userRepository.GetLoginUserByIdAsync(userId);
            // At least for now, all instance administrators can import workflows.
            if (user.InstanceAdminRoleId == null)
            {
                throw new AuthorizationException(
                    "The user is not an instance administrator and therefore does not have permissions to import workflows.",
                    ErrorCodes.UnauthorizedAccess);
            }
        }

        private void VerifyWorkflowFeature()
        {
            if (!IsWorkflowFeatureEnabled())
            {
                throw new AuthorizationException("The Workflow feature is disabled.", ErrorCodes.WorkflowDisabled);
            }
        }


        private bool IsWorkflowFeatureEnabled()
        {
            // TODO: after NW made information about Workflow feature available for the services.
            return true;
        }

        private static string GetValidationErrorsText(List<WorkflowValidationError> validationErrors)
        {
            // TODO: create a validation errors builder
            var sb = new StringBuilder();
            sb.AppendLine(I18NHelper.FormatInvariant("Uploaded workflow contains {0} error(s):", validationErrors.Count));
            foreach (var error in validationErrors)
            {
                sb.AppendLine(I18NHelper.FormatInvariant("    - {0}", error.ErrorCode));
            }

            return sb.ToString();
        }

        private async Task<string> UploadErrorsToFileStore(string errors)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(errors ?? string.Empty)))
            {
                return await FileRepository.UploadFileAsync(WorkflowImportErrorsFile, null, stream, DateTime.UtcNow + TimeSpan.FromDays(1));
            }
        }

    }
}