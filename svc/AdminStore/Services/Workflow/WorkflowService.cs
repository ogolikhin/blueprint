using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdminStore.Models;
using AdminStore.Models.Workflow;
using AdminStore.Repositories;
using AdminStore.Repositories.Workflow;
using ArtifactStore.Helpers;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Repositories.Files;
using File = ServiceLibrary.Models.Files.File;

namespace AdminStore.Services.Workflow
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IWorkflowXmlValidator _workflowValidator;
        private readonly IUserRepository _userRepository;
        private readonly IWorkflowDataValidator _workflowDataValidator;

        private const string WorkflowImportErrorsFile = "$workflow_import_errors$.txt";

        public WorkflowService()
            : this(new WorkflowRepository(), new WorkflowXmlValidator(), new SqlUserRepository())
        {
            _workflowDataValidator = new WorkflowDataValidator(_workflowRepository, _userRepository);
        }

        public WorkflowService(IWorkflowRepository workflowRepository, IWorkflowXmlValidator workflowValidator, IUserRepository userRepository)
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
                var dataValidationResult = await _workflowDataValidator.ValidateData(workflow);
                if (dataValidationResult.HasErrors)
                {
                    // TODO: Create a text file and save it to the file store.
                    // TODO: The name convention for the error file "$workflow_import_errors$.txt".
                    // TODO: The name convention should be checked when the errors are requested by the client. 
                    // TODO: Return guid of the errors file.

                    var textErrors = GetValidationErrorsText(dataValidationResult.Errors);
                    var guid = await UploadErrorsToFileStore(textErrors);

                    importResult.ErrorsGuid = guid;
                    importResult.ResultCode = ImportWorkflowResultCodes.InvalidModel;
                    return;
                }

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
                    await ImportWorkflowComponentsAsync(workflow, newWorkflow, publishRevision, transaction, dataValidationResult);

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
               
                await _workflowRepository.UpdateWorkflows(workflows, publishRevision, transaction);
            };
            await _workflowRepository.RunInTransactionAsync(action);
        }

        public async Task<int> DeleteWorkflows(OperationScope body, string search, int sessionUserId)
        {
            var totalDeleted = 0;
            Func<IDbTransaction, Task> action = async transaction =>
            {
                var publishRevision =
                    await
                        _workflowRepository.CreateRevisionInTransactionAsync(transaction, sessionUserId,
                            $"DeleteWorkflows. Session user id is {sessionUserId}.");
                totalDeleted = await _workflowRepository.DeleteWorkflows(body, search, publishRevision);
            };
            await _workflowRepository.RunInTransactionAsync(action);

            return totalDeleted;
        }

        private async Task ImportWorkflowComponentsAsync(IeWorkflow workflow, SqlWorkflow newWorkflow, int publishRevision, IDbTransaction transaction, WorkflowDataValidationResult dataValidationResult)
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
                await ImportWorkflowTransitions(workflow, newWorkflow, publishRevision, transaction, newStates, dataValidationResult.ValidGroups);
            }

            if (workflow.ArtifactTypes.Any() && workflow.Projects.Any())
            {
                await _workflowRepository.CreateWorkflowArtifactAssociationsAsync(dataValidationResult.ValidArtifactTypeNames,
                        dataValidationResult.ValidProjectIds, newWorkflow.WorkflowId, publishRevision, transaction);
            }
        }

        private async Task ImportWorkflowTransitions(IeWorkflow workflow, SqlWorkflow newWorkflow, int publishRevision,
            IDbTransaction transaction, IEnumerable<SqlState> newStates, HashSet<SqlGroup> validGroups)
        {
            var newStatesArray = newStates.ToArray();
            var importTriggersParams = new List<SqlWorkflowEvent>();

            workflow.TransitionEvents.OfType<IeTransitionEvent>().ForEach(transition =>
            {
                importTriggersParams.Add(new SqlWorkflowEvent
                {
                    Name = transition.Name,
                    Description = transition.Description,
                    WorkflowId = newWorkflow.WorkflowId,
                    Type = DWorkflowEventType.Transition,
                    Permissions = SerializationHelper.ToXml(new XmlTriggerPermissions
                    {
                        Skip = "0",
                        GroupIds = transition.PermissionGroups.Select(pg => validGroups.First(p => p.Name == pg.Name).GroupId).ToList()
                    }),
                    Validations = null,
                    Triggers = SerializationHelper.ToXml(transition.Triggers),
                    WorkflowState1Id = newStatesArray.FirstOrDefault(s => s.Name.Equals(transition.FromState))?.WorkflowStateId,
                    WorkflowState2Id = newStatesArray.FirstOrDefault(s => s.Name.Equals(transition.ToState))?.WorkflowStateId,
                    PropertyTypeId = null
                });
            });
            await _workflowRepository.CreateWorkflowEventsAsync(importTriggersParams, publishRevision, transaction);
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

        private static string GetValidationErrorsText(List<WorkflowXmlValidationError> validationErrors)
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

        private static string GetValidationErrorsText(List<WorkflowDataValidationError> dataValidationErrors)
        {
            // TODO: create a validation errors builder
            var sb = new StringBuilder();
            sb.AppendLine(I18NHelper.FormatInvariant("Uploaded workflow contains {0} error(s):", dataValidationErrors.Count));
            foreach (var error in dataValidationErrors)
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