using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdminStore.Helpers.Workflow;
using AdminStore.Models;
using AdminStore.Models.DiagramWorkflow;
using AdminStore.Models.DTO;
using AdminStore.Models.Enums;
using AdminStore.Models.Workflow;
using AdminStore.Repositories.Workflow;
using AdminStore.Services.Workflow.Validation;
using AdminStore.Services.Workflow.Validation.Data;
using AdminStore.Services.Workflow.Validation.Data.PropertyValue;
using AdminStore.Services.Workflow.Validation.Xml;
using BluePrintSys.Messaging.CrossCutting.Helpers;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ApplicationSettings;
using ServiceLibrary.Repositories.ConfigControl;
using ServiceLibrary.Repositories.Files;
using ServiceLibrary.Repositories.ProjectMeta;
using File = ServiceLibrary.Models.Files.File;
using SqlWorkflowEvent = AdminStore.Models.Workflow.SqlWorkflowEvent;
using ServiceLibrary.Helpers.Security;

namespace AdminStore.Services.Workflow
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IUsersRepository _usersRepository;

        private readonly IWorkflowXmlValidator _workflowXmlValidator;
        private readonly IWorkflowDataValidator _workflowDataValidator;
        private readonly IWorkflowValidationErrorBuilder _workflowValidationErrorBuilder;
        private readonly IProjectMetaRepository _projectMetaRepository;
        private readonly ITriggerConverter _triggerConverter;
        private readonly IPropertyValueValidatorFactory _propertyValueValidatorFactory;
        private readonly IWorkflowDiff _workflowDiff;
        private readonly IArtifactRepository _artifactRepository;
        private readonly IApplicationSettingsRepository _applicationSettingsRepository;
        private readonly IServiceLogRepository _serviceLogRepository;
        private readonly ISendMessageExecutor _sendMessageExecutor;

        private const string WorkflowImportErrorsFile = "$workflow_import_errors$.txt";

        public WorkflowService()
            : this(
                  new WorkflowRepository(),
                  new WorkflowXmlValidator(),
                  new SqlUsersRepository(),
                  new WorkflowValidationErrorBuilder(),
                  new SqlProjectMetaRepository(),
                  new TriggerConverter(),
                  new PropertyValueValidatorFactory(),
                  new WorkflowDiff(),
                  new SqlArtifactRepository(),
                  new ApplicationSettingsRepository(),
                  new ServiceLogRepository(),
                  new SendMessageExecutor())
        {
            _workflowDataValidator = new WorkflowDataValidator(
                _workflowRepository,
                _usersRepository,
                _projectMetaRepository,
                _propertyValueValidatorFactory);

            _artifactRepository = new SqlArtifactRepository();
        }

        public WorkflowService(IWorkflowRepository workflowRepository,
            IWorkflowXmlValidator workflowXmlValidator,
            IUsersRepository usersRepository,
            IWorkflowValidationErrorBuilder workflowValidationErrorBuilder,
            IProjectMetaRepository projectMetaRepository,
            ITriggerConverter triggerConverter,
            IPropertyValueValidatorFactory propertyValueValidatorFactory,
            IWorkflowDiff workflowDiff,
            IArtifactRepository artifactRepository,
            IApplicationSettingsRepository applicationSettingsRepository,
            IServiceLogRepository serviceLogRepository,
            ISendMessageExecutor sendMessageExecutor)
        {
            _workflowRepository = workflowRepository;
            _workflowXmlValidator = workflowXmlValidator;
            _usersRepository = usersRepository;
            _workflowValidationErrorBuilder = workflowValidationErrorBuilder;
            _projectMetaRepository = projectMetaRepository;
            _triggerConverter = triggerConverter;
            _propertyValueValidatorFactory = propertyValueValidatorFactory;
            _workflowDiff = workflowDiff;
            _artifactRepository = artifactRepository;
            _applicationSettingsRepository = applicationSettingsRepository;
            _serviceLogRepository = serviceLogRepository;
            _sendMessageExecutor = sendMessageExecutor;
        }

        public IFileRepository FileRepository
        {
            get { return _workflowRepository.FileRepository; }
            set { _workflowRepository.FileRepository = value; }
        }

        public async Task<string> GetImportWorkflowErrorsAsync(string guid, int userId)
        {
            if (string.IsNullOrWhiteSpace(guid))
            {
                throw new BadRequestException("The error GUID is not provided.", ErrorCodes.BadRequest);
            }

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
                    ErrorMessages.WorkflowImportErrorsNotFound, guid),
                    ErrorCodes.ResourceNotFound);
            }
            using (var reader = new StreamReader(errorsFile.ContentStream))
            {
                return reader.ReadToEnd();
            }
        }

        public async Task<ImportWorkflowResult> ImportWorkflowAsync(IeWorkflow workflow, string fileName, int userId, string xmlSerError)
        {
            var importResult = new ImportWorkflowResult();

            var xmlValidationResult = ValidateWorkflowXmlSerialization(xmlSerError);
            if (!xmlValidationResult.HasErrors)
            {
                ReplaceNewLinesInNames(workflow);
                xmlValidationResult = _workflowXmlValidator.ValidateXml(workflow);
            }

            if (xmlValidationResult.HasErrors)
            {
                var textErrors = _workflowValidationErrorBuilder.BuildTextXmlErrors(xmlValidationResult.Errors, fileName);
                var guid = await UploadErrorsToFileStoreAsync(textErrors);

                importResult.ErrorsGuid = guid;
                importResult.ResultCode = ImportWorkflowResultCodes.InvalidModel;

#if DEBUG
                importResult.ErrorMessage = textErrors;
#endif

                return importResult;
            }

            var dataValidationResult = await _workflowDataValidator.ValidateDataAsync(workflow);
            if (dataValidationResult.HasErrors)
            {
                var textErrors = _workflowValidationErrorBuilder.BuildTextDataErrors(dataValidationResult.Errors,
                    fileName);
                var guid = await UploadErrorsToFileStoreAsync(textErrors);

                importResult.ErrorsGuid = guid;
                importResult.ResultCode = ImportWorkflowResultCodes.Conflict;

#if DEBUG
                importResult.ErrorMessage = textErrors;
#endif

                return importResult;
            }

            SqlWorkflow newWorkflow = null;

            Func<IDbTransaction, Task> action = async transaction =>
            {
                var publishRevision =
                    await _workflowRepository.CreateRevisionInTransactionAsync(transaction, userId, "Workflow import.");

                var importParams = new SqlWorkflow
                {
                    Name = workflow.Name,
                    Description = workflow.Description,
                    Active = false // imported workflows are inactive. Users need explicitly activate workflows via UI.
                };
                newWorkflow =
                    (await _workflowRepository.CreateWorkflowsAsync(new[] { importParams }, publishRevision, transaction))
                        .FirstOrDefault();

                if (newWorkflow != null)
                {
                    await
                        ImportWorkflowComponentsAsync(workflow, newWorkflow.WorkflowId, publishRevision, transaction,
                            dataValidationResult);
                    await
                        _workflowRepository.UpdateWorkflowsChangedWithRevisionsAsync(newWorkflow.WorkflowId, publishRevision,
                            transaction);

                    importResult.ResultCode = ImportWorkflowResultCodes.Ok;
                }
            };

            await _workflowRepository.RunInTransactionAsync(action);
            importResult.WorkflowId = newWorkflow?.WorkflowId;

            return importResult;

        }

        public async Task<ImportWorkflowResult> UpdateWorkflowViaImport(int userId, int workflowId, IeWorkflow workflow,
            string fileName = null, string xmlSerError = null, WorkflowMode workflowMode = WorkflowMode.Xml)
        {
            var importResult = new ImportWorkflowResult();

            var xmlValidationResult = new WorkflowXmlValidationResult();

            if (workflowMode == WorkflowMode.Xml)
            {
                xmlValidationResult = ValidateWorkflowXmlSerialization(xmlSerError);
            }

            if (!xmlValidationResult.HasErrors)
            {
                xmlValidationResult = ValidateWorkflowId(workflow, workflowId);
            }

            if (xmlValidationResult.HasErrors)
            {
                return await FillingXmlErrorsWorkflowResult(fileName, workflowMode, xmlValidationResult, importResult);
            }

            var dataValidationResult = new WorkflowDataValidationResult();

            var standardTypes = await _projectMetaRepository.GetStandardProjectTypesAsync();
            var currentWorkflow = await GetWorkflowExportAsync(workflowId, standardTypes, workflowMode, false);
            if (currentWorkflow.IsActive)
            {
                dataValidationResult.Errors.Add(new WorkflowDataValidationError
                {
                    Element = workflow,
                    ErrorCode = WorkflowDataValidationErrorCodes.WorkflowActive
                });

                return await FillingDataErrorsWorkflowResult(fileName, workflowMode, dataValidationResult, importResult, false);
            }

            ReplaceNewLinesInNames(workflow);
            xmlValidationResult = _workflowXmlValidator.ValidateUpdateXml(workflow);
            if (xmlValidationResult.HasErrors)
            {
                return await FillingXmlErrorsWorkflowResult(fileName, workflowMode, xmlValidationResult, importResult);
            }

            await ReplaceProjectPathsWithIdsAsync(workflow);

            dataValidationResult = await _workflowDataValidator.ValidateUpdateDataAsync(workflow, standardTypes);

            var workflowDiffResult = _workflowDiff.DiffWorkflows(workflow, currentWorkflow);

            // Even if the data validation has errors,
            // anyway we do the validation of not found by Id in current.
            var notFoundErrors = ValidateAndRemoveNotFoundByIdInCurrentWorkflow(workflow, workflowDiffResult);

            dataValidationResult.Errors.AddRange(notFoundErrors);

            if (!dataValidationResult.HasErrors && !workflowDiffResult.HasChanges && workflowMode != WorkflowMode.Canvas)
            {
                dataValidationResult.Errors.Add(new WorkflowDataValidationError
                {
                    Element = workflow,
                    ErrorCode = WorkflowDataValidationErrorCodes.WorkflowNothingToUpdate
                });
            }

            if (dataValidationResult.HasErrors)
            {
                return await FillingDataErrorsWorkflowResult(fileName, workflowMode, dataValidationResult, importResult);
            }

            AssignStateOrderIndexes(workflowDiffResult,
                currentWorkflow.States?.ToDictionary(s => s.Id.Value, s => s.OrderIndex));

            Func<IDbTransaction, Task> action = async transaction =>
            {
                var publishRevision =
                    await
                        _workflowRepository.CreateRevisionInTransactionAsync(transaction, userId,
                            workflowMode == WorkflowMode.Xml ? "Workflow update via import." : "Workflow's diagram update.");

                await UpdateWorkflowEntitiesAsync(workflow, workflowDiffResult, dataValidationResult,
                    publishRevision, transaction, workflowMode);
                await _workflowRepository.UpdateWorkflowsChangedWithRevisionsAsync(workflow.Id.Value,
                    publishRevision, transaction);

                importResult.ResultCode = ImportWorkflowResultCodes.Ok;
            };

            await _workflowRepository.RunInTransactionAsync(action);

            importResult.ResultCode = ImportWorkflowResultCodes.Ok;
            return importResult;
        }

        private async Task<ImportWorkflowResult> FillingDataErrorsWorkflowResult(string fileName, WorkflowMode workflowMode,
            WorkflowDataValidationResult dataValidationResult, ImportWorkflowResult importResult, bool isEditFileMessage = true)
        {
            var textErrors = workflowMode == WorkflowMode.Xml
                ? _workflowValidationErrorBuilder.BuildTextDataErrors(dataValidationResult.Errors, fileName, isEditFileMessage)
                : _workflowValidationErrorBuilder.BuildTextDataErrors(dataValidationResult.Errors);

            var guid = await UploadErrorsToFileStoreAsync(textErrors);

            importResult.ErrorsGuid = guid;
            importResult.ResultCode = ImportWorkflowResultCodes.Conflict;

#if DEBUG
            importResult.ErrorMessage = textErrors;
#endif

            return importResult;
        }

        private async Task<ImportWorkflowResult> FillingXmlErrorsWorkflowResult(string fileName, WorkflowMode workflowMode,
            WorkflowXmlValidationResult xmlValidationResult, ImportWorkflowResult importResult)
        {
            var textErrors = workflowMode == WorkflowMode.Xml
                ? _workflowValidationErrorBuilder.BuildTextXmlErrors(xmlValidationResult.Errors, fileName)
                : _workflowValidationErrorBuilder.BuildTextDiagramErrors(xmlValidationResult.Errors);
            var guid = await UploadErrorsToFileStoreAsync(textErrors);

            importResult.ErrorsGuid = guid;
            importResult.ResultCode = ImportWorkflowResultCodes.InvalidModel;

#if DEBUG
            importResult.ErrorMessage = textErrors;
#endif

            return importResult;
        }

        public async Task<WorkflowDetailsDto> GetWorkflowDetailsAsync(int workflowId)
        {
            var ieWorkflow = await GetWorkflowExportAsync(workflowId, WorkflowMode.Canvas);

            var numberOfStatesAndActions = GetNumberOfStatesAndActions(ieWorkflow);

             var workflowDetailsDto = new WorkflowDetailsDto
            {
                Name = ieWorkflow.Name,
                Description = ieWorkflow.Description,
                Active = ieWorkflow.IsActive,
                WorkflowId = ieWorkflow.Id.GetValueOrDefault(),
                VersionId = ieWorkflow.VersionId,
                LastModified = ieWorkflow.LastModified,
                LastModifiedBy = ieWorkflow.LastModifiedBy,
                NumberOfActions = numberOfStatesAndActions.NumberOfActions,
                NumberOfStates = numberOfStatesAndActions.NumberOfStates,
                Projects = ieWorkflow.Projects?.Select(e => new WorkflowProjectDto { Id = e.Id.GetValueOrDefault(), Name = e.Path ?? string.Empty }).Distinct().ToList(),
                ArtifactTypes = ieWorkflow.Projects?.Where(p => p.ArtifactTypes != null).SelectMany(e => e.ArtifactTypes).Select(t => new WorkflowArtifactTypeDto { Name = t.Name ?? string.Empty }).Distinct().ToList()
            };

            return workflowDetailsDto;
        }

        public async Task<int> UpdateWorkflowStatusAsync(StatusUpdate statusUpdate, int workflowId, int userId)
        {
            var versionId = 0;

            var existingWorkflow = await _workflowRepository.GetWorkflowDetailsAsync(workflowId);
            if (existingWorkflow == null)
            {
                throw new ResourceNotFoundException(ErrorMessages.WorkflowNotExist, ErrorCodes.ResourceNotFound);
            }

            if (existingWorkflow.VersionId != statusUpdate.VersionId)
            {
                throw new ConflictException(ErrorMessages.WorkflowVersionsNotEqual, ErrorCodes.Conflict);
            }

            var workflows = new List<SqlWorkflow>
            {
                new SqlWorkflow
                {
                    Name = existingWorkflow.Name,
                    Description = existingWorkflow.Description,
                    Active = statusUpdate.Active,
                    WorkflowId = workflowId
                }
            };

            Func<IDbTransaction, Task> action = async transaction =>
            {
                var publishRevision =
                    await
                        _workflowRepository.CreateRevisionInTransactionAsync(transaction, userId,
                            $"Updating the workflow with id {workflowId}.");
                if (publishRevision < 1)
                {
                    throw new ArgumentException(I18NHelper.FormatInvariant("{0} is less than 1.",
                        nameof(publishRevision)));
                }

                versionId = await _workflowRepository.UpdateWorkflowsAsync(workflows, publishRevision, transaction);

                if (existingWorkflow.Active != statusUpdate.Active)
                {
                    await PostWorkflowStatusUpdate(workflowId, userId, publishRevision, transaction);
                }
            };
            await _workflowRepository.RunInTransactionAsync(action);
            return versionId;
        }

        private async Task PostWorkflowStatusUpdate(int workflowId, int userId, int revisionId, IDbTransaction transaction = null)
        {
            var message = new WorkflowsChangedMessage { UserId = userId, RevisionId = revisionId, WorkflowId = workflowId };
            await _sendMessageExecutor.Execute(_applicationSettingsRepository, _serviceLogRepository, message, transaction);
        }

        public async Task UpdateWorkflowAsync(UpdateWorkflowDto workflowDto, int workflowId, int userId)
        {
            var previousWorkflowVersion = await _workflowRepository.GetWorkflowDetailsAsync(workflowId);

            if (previousWorkflowVersion == null)
            {
                throw new ResourceNotFoundException(ErrorMessages.WorkflowNotExist, ErrorCodes.ResourceNotFound);
            }

            var workflow = new SqlWorkflow
            {
                Name = workflowDto.Name,
                Description = workflowDto.Description,
                Active = workflowDto.Status,
                WorkflowId = workflowId,
                VersionId = workflowDto.VersionId
            };

            Func<IDbTransaction, Task> action = async transaction =>
            {
                var publishRevision =
                    await
                        _workflowRepository.CreateRevisionInTransactionAsync(transaction, userId,
                            $"Updating the workflow with id {workflowId}.");
                if (publishRevision < 1)
                {
                    throw new ArgumentException(I18NHelper.FormatInvariant("{0} is less than 1.",
                        nameof(publishRevision)));
                }

                await _workflowRepository.UpdateWorkflowAsync(workflow, publishRevision, transaction);
                if (previousWorkflowVersion.Active != workflowDto.Status)
                {
                    await PostWorkflowStatusUpdate(workflowId, userId, publishRevision, transaction);
                }
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
                totalDeleted = await _workflowRepository.DeleteWorkflowsAsync(body, search, publishRevision);
            };
            await _workflowRepository.RunInTransactionAsync(action);

            return totalDeleted;
        }

        private WorkflowXmlValidationResult ValidateWorkflowXmlSerialization(string errorMessage)
        {
            var result = new WorkflowXmlValidationResult();

            if (errorMessage != null)
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    Element = errorMessage,
                    ErrorCode = WorkflowXmlValidationErrorCodes.WorkflowXmlSerializationError
                });
            }

            return result;
        }

        private async Task ImportWorkflowComponentsAsync(IeWorkflow workflow, int newWorkflowId, int publishRevision,
            IDbTransaction transaction, WorkflowDataValidationResult dataValidationResult)
        {
            var i = 1;
            workflow.States.ForEach(s => s.OrderIndex = 10 * i++);
            var newStates = (await _workflowRepository.CreateWorkflowStatesAsync(workflow.States.Select(s =>
                ToSqlState(s, newWorkflowId)), publishRevision, transaction)).ToList();

            var dataMaps = CreateDataMap(dataValidationResult, newStates.ToDictionary(s => s.Name, s => s.WorkflowStateId));

            await CreateWebooksAsync(workflow, newWorkflowId, transaction, dataMaps);

            await CreateWorkflowEventsAsync(workflow, newWorkflowId, publishRevision, transaction, dataMaps);

            var kvPairs = new List<KeyValuePair<int, string>>();
            if (!workflow.Projects.IsEmpty())
            {
                workflow.Projects.ForEach(p => p.ArtifactTypes?.ForEach(at =>
                {
                    // All Project Ids should be assigned.
                    kvPairs.Add(new KeyValuePair<int, string>(p.Id.Value, at.Name));
                }));
            }

            if (!kvPairs.IsEmpty())
            {
                await _workflowRepository.UpdateWorkflowArtifactAssignmentsAsync(kvPairs, new List<KeyValuePair<int, string>>(), newWorkflowId, transaction);
            }
        }

        private static WorkflowDataMaps CreateDataMap(WorkflowDataValidationResult dataValidationResult,
            IDictionary<string, int> stateMap)
        {
            var dataMaps = new WorkflowDataMaps();
            dataMaps.UserMap.AddRange(dataValidationResult.Users.ToDictionary(u => u.Login, u => u.UserId));
            dataMaps.GroupMap.AddRange(dataValidationResult.Groups.ToDictionary(u => Tuple.Create(u.Name, u.ProjectId),
                u => u.GroupId));
            dataMaps.StateMap.AddRange(stateMap);
            dataMaps.ArtifactTypeMap.AddRange(
                dataValidationResult.StandardTypes.ArtifactTypes.ToDictionary(at => at.Name, at => at.Id));
            dataValidationResult.StandardTypes.PropertyTypes.ForEach(pt =>
            {
                dataMaps.PropertyTypeMap.Add(pt.Name, pt.Id);

                if (pt.PrimitiveType == PropertyPrimitiveType.Choice)
                {
                    var validValuesById = new Dictionary<int, string>();
                    var validValuesByValue = new Dictionary<string, int>();

                    pt.ValidValues?.ForEach(vv =>
                    {
                        if (!validValuesByValue.ContainsKey(vv.Value))
                        {
                            validValuesByValue.Add(vv.Value, vv.Id.GetValueOrDefault());
                        }

                        if (vv.Id.HasValue && !validValuesById.ContainsKey(vv.Id.Value))
                        {
                            validValuesById.Add(vv.Id.Value, vv.Value);
                        }
                    });

                    dataMaps.ValidValuesByValue.Add(pt.Id, validValuesByValue);
                    dataMaps.ValidValuesById.Add(pt.Id, validValuesById);
                }
            });

            return dataMaps;
        }

        private static SqlState ToSqlState(IeState ieState, int? workflowId, WorkflowMode workflowMode = WorkflowMode.Xml)
        {
            return ieState == null ? null : new SqlState
            {
                WorkflowStateId = ieState.Id.GetValueOrDefault(),
                Name = ieState.Name,
                WorkflowId = workflowId.GetValueOrDefault(),
                Default = ieState.IsInitial.HasValue && ieState.IsInitial.Value,
                OrderIndex = ieState.OrderIndex,
                CanvasSettings = workflowMode == WorkflowMode.Canvas ? SerializeStateCanvasSettings(ieState.Location) : null
            };
        }

        private async Task CreateWorkflowEventsAsync(IeWorkflow workflow, int workflowId, int publishRevision,
            IDbTransaction transaction, WorkflowDataMaps dataMaps)
        {
            var importTriggersParams = new List<SqlWorkflowEvent>();

            workflow.TransitionEvents.OfType<IeTransitionEvent>().ForEach(e =>
            {
                importTriggersParams.Add(ToSqlWorkflowEvent(e, workflowId, dataMaps));
            });
            workflow.PropertyChangeEvents.OfType<IePropertyChangeEvent>().ForEach(e =>
            {
                importTriggersParams.Add(ToSqlWorkflowEvent(e, workflowId, dataMaps));
            });
            workflow.NewArtifactEvents.OfType<IeNewArtifactEvent>().ForEach(e =>
            {
                importTriggersParams.Add(ToSqlWorkflowEvent(e, workflowId, dataMaps));
            });

            await _workflowRepository.CreateWorkflowEventsAsync(importTriggersParams, publishRevision, transaction);
        }

        private SqlWorkflowEvent ToSqlWorkflowEvent(IeEvent wEvent, int newWorkflowId, WorkflowDataMaps dataMaps, WorkflowMode workflowMode = WorkflowMode.Xml)
        {
            var sqlEvent = new SqlWorkflowEvent
            {
                WorkflowEventId = wEvent.Id.GetValueOrDefault(),
                Name = wEvent.Name,
                WorkflowId = newWorkflowId,
                Validations = null,
                Triggers = wEvent.Triggers == null
                    ? null
                    : SerializationHelper.ToXml(_triggerConverter.ToXmlModel(wEvent.Triggers, dataMaps))
            };

            switch (wEvent.EventType)
            {
                case EventTypes.Transition:
                    sqlEvent.Type = DWorkflowEventType.Transition;
                    var transition = (IeTransitionEvent)wEvent;
                    var skipPermissionGroups = transition.SkipPermissionGroups.GetValueOrDefault();
                    sqlEvent.Permissions = skipPermissionGroups || !transition.PermissionGroups.IsEmpty()
                        ? sqlEvent.Permissions = SerializationHelper.ToXml(new XmlTriggerPermissions
                        {
                            Skip = skipPermissionGroups ? 1 : (int?)null,
                            GroupIds = transition.PermissionGroups.Select(pg =>
                                dataMaps.GroupMap[Tuple.Create(pg.Name, (int?)null)]).ToList()
                        })
                        : null;

                    int state;
                    if (!dataMaps.StateMap.TryGetValue(transition.FromState, out state))
                    {
                        throw new ExceptionWithErrorCode(
                            I18NHelper.FormatInvariant("Id of State '{0}' is not found.",
                                transition.FromState),
                            ErrorCodes.UnexpectedError);
                    }
                    sqlEvent.WorkflowState1Id = state;
                    if (!dataMaps.StateMap.TryGetValue(transition.ToState, out state))
                    {
                        throw new ExceptionWithErrorCode(
                            I18NHelper.FormatInvariant("Id of State '{0}' is not found.",
                                transition.ToState),
                            ErrorCodes.UnexpectedError);
                    }
                    sqlEvent.WorkflowState2Id = state;
                    sqlEvent.CanvasSettings = workflowMode == WorkflowMode.Canvas ? SerializeTransitionCanvasSettings(transition.PortPair) : null;
                    break;
                case EventTypes.PropertyChange:
                    sqlEvent.Type = DWorkflowEventType.PropertyChange;
                    var pcEvent = (IePropertyChangeEvent)wEvent;
                    int propertyTypeId;
                    if (!WorkflowHelper.TryGetNameOrDescriptionPropertyTypeId(pcEvent.PropertyName, out propertyTypeId)
                        && !dataMaps.PropertyTypeMap.TryGetValue(pcEvent.PropertyName, out propertyTypeId))
                    {
                        throw new ExceptionWithErrorCode(
                            I18NHelper.FormatInvariant("Id of Standard Property Type '{0}' is not found.",
                                pcEvent.PropertyName),
                            ErrorCodes.UnexpectedError);
                    }
                    sqlEvent.PropertyTypeId = propertyTypeId;
                    break;
                case EventTypes.NewArtifact:
                    sqlEvent.Type = DWorkflowEventType.NewArtifact;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(wEvent.EventType));
            }

            return sqlEvent;
        }

        public async Task<DWorkflow> GetWorkflowDiagramAsync(int workflowId)
        {
            var ieWorkflow = await GetWorkflowExportAsync(workflowId, WorkflowMode.Canvas);
            var dWorkflow = WorkflowHelper.MapIeWorkflowToDWorkflow(ieWorkflow);

            var numberOfStatesAndActions = GetNumberOfStatesAndActions(ieWorkflow);
            dWorkflow.NumberOfActions = numberOfStatesAndActions.NumberOfActions;
            dWorkflow.NumberOfStates = numberOfStatesAndActions.NumberOfStates;

            return dWorkflow;
        }

        public async Task<IEnumerable<PropertyType>> GetWorkflowArtifactTypesProperties(ISet<int> standardArtifactTypeIds)
        {
            var standardProperties =
                (await _artifactRepository.GetStandardProperties(standardArtifactTypeIds)).ToList();

            PropertyType nameProperty;
            WorkflowHelper.TryGetNameOrDescriptionPropertyType(WorkflowConstants.PropertyTypeFakeIdName, out nameProperty);
            standardProperties.Add(nameProperty);

            PropertyType descriptionProperty;
            WorkflowHelper.TryGetNameOrDescriptionPropertyType(WorkflowConstants.PropertyTypeFakeIdDescription, out descriptionProperty);

            standardProperties.Add(descriptionProperty);

            return standardProperties.OrderBy(x => x.Name); // sort by name ASC as it is done in [dbo].GetStandardProperties
        }

        public async Task<IeWorkflow> GetWorkflowExportAsync(int workflowId, WorkflowMode mode)
        {
            var standardTypes = await _projectMetaRepository.GetStandardProjectTypesAsync();
            return await GetWorkflowExportAsync(workflowId, standardTypes, mode, true);
        }

        public async Task<int> CreateWorkflow(string name, string description, int userId)
        {
            var workflowId = 0;

            Func<IDbTransaction, Task> action = async transaction =>
            {
                var publishRevision =
                    await _workflowRepository.CreateRevisionInTransactionAsync(transaction, userId, "Create workflow.");

                var workflow = new SqlWorkflow
                {
                    Name = name,
                    Description = description,
                    Active = false
                };
                workflowId = await _workflowRepository.CreateWorkflow(workflow, publishRevision, transaction);

                // generate default states
                var states = new List<IeState>()
                {
                    new IeState()
                    {
                        Name = "New",
                        OrderIndex = 10,
                        IsInitial = true
                    },
                    new IeState()
                    {
                        Name = "Done",
                        OrderIndex = 40
                    }
                };

                var newStates = (await _workflowRepository.CreateWorkflowStatesAsync(states.Select(s =>
                    ToSqlState(s, workflowId)), publishRevision, transaction)).ToList();

                var workflowEvents = new List<SqlWorkflowEvent>()
                {
                    new SqlWorkflowEvent()
                    {
                       Name = "Transition 1",
                       WorkflowId = workflowId,
                       WorkflowState1Id = newStates[0].WorkflowStateId,
                       WorkflowState2Id = newStates[1].WorkflowStateId,
                       Triggers = "<TSR><TS /></TSR>",
                       Type = DWorkflowEventType.Transition,
                       EndRevision = 2147483647
                    },
                };

                await _workflowRepository.CreateWorkflowEventsAsync(workflowEvents, publishRevision, transaction);

                await
                        _workflowRepository.UpdateWorkflowsChangedWithRevisionsAsync(workflowId, publishRevision,
                            transaction);

            };
            await _workflowRepository.RunInTransactionAsync(action);
            return workflowId;
        }

        private async Task<IeWorkflow> GetWorkflowExportAsync(int workflowId, ProjectTypes standardTypes, WorkflowMode mode, bool shouldDeleteInValidData)
        {
            var workflowDetails = await _workflowRepository.GetWorkflowDetailsAsync(workflowId);
            if (workflowDetails == null)
            {
                throw new ResourceNotFoundException(ErrorMessages.WorkflowNotExist, ErrorCodes.ResourceNotFound);
            }
            var workflowArtifactTypes = (await _workflowRepository.GetWorkflowArtifactTypesAsync(workflowId)).ToList();
            var workflowStates = (await _workflowRepository.GetWorkflowStatesAsync(workflowId)).ToList();
            var workflowEvents = (await _workflowRepository.GetWorkflowEventsAsync(workflowId)).ToList();

            var dataMaps = LoadDataMaps(standardTypes,
                workflowStates.ToDictionary(s => s.WorkflowStateId, s => s.Name));
            var userIds = new HashSet<int>();
            var groupIds = new HashSet<int>();

            var ieWorkflow = new IeWorkflow
            {
                Id = workflowDetails.WorkflowId,
                Name = workflowDetails.Name,
                Description = workflowDetails.Description,
                IsActive = workflowDetails.Active,
                VersionId = workflowDetails.VersionId,
                LastModified = workflowDetails.LastModified,
                LastModifiedBy = workflowDetails.LastModifiedBy,
                HasProcessArtifactType = workflowArtifactTypes.Any(q => q.PredefinedType == (int)ItemTypePredefined.Process),
                States = workflowStates.Select(
                        e => new IeState
                        {
                            Id = e.WorkflowStateId,
                            IsInitial = e.Default,
                            Name = e.Name,
                            OrderIndex = e.OrderIndex,
                            Location = mode == WorkflowMode.Canvas ? DeserializeStateCanvasSettings(e.CanvasSettings) : null
                        })
                        .Distinct()
                        .ToList(),
                // Do not include Transition if FromState or ToState is not found.
                TransitionEvents = workflowEvents.Where(e => e.Type == (int)DWorkflowEventType.Transition
                    && e.FromStateId.HasValue && dataMaps.StateMap.ContainsKey(e.FromStateId.Value)
                    && e.ToStateId.HasValue && dataMaps.StateMap.ContainsKey(e.ToStateId.Value)).
                    Select(e => new IeTransitionEvent
                    {
                        Id = e.WorkflowEventId,
                        Name = e.Name,
                        FromStateId = e.FromStateId,
                        FromState = e.FromState,
                        ToState = e.ToState,
                        ToStateId = e.ToStateId,
                        PermissionGroups = DeserializePermissionGroups(e.Permissions, groupIds),
                        SkipPermissionGroups = GetSkipPermissionGroup(e.Permissions),
                        Triggers = DeserializeTriggersForWorkflowMode(e.Triggers, dataMaps, userIds, groupIds, mode),
                        PortPair = mode == WorkflowMode.Canvas ? DeserializeTransitionCanvasSettings(e.CanvasSettings) : null
                    }).Distinct().ToList(),
                // Do not include PropertyChangeEvent if PropertyType is not found.
                PropertyChangeEvents = workflowEvents.Where(e => e.Type == (int)DWorkflowEventType.PropertyChange
                    && e.PropertyTypeId.HasValue && (dataMaps.PropertyTypeMap.ContainsKey(e.PropertyTypeId.Value)
                    || WorkflowHelper.IsNameOrDescriptionProperty(e.PropertyTypeId.Value))).
                    Select(e => new IePropertyChangeEvent
                    {
                        Id = e.WorkflowEventId,
                        Name = e.Name,
                        PropertyId = e.PropertyTypeId,
                        PropertyName = GetPropertyChangedName(e.PropertyTypeId, dataMaps),
                        Triggers = DeserializeTriggersForWorkflowMode(e.Triggers, dataMaps, userIds, groupIds, mode)
                    }).Distinct().ToList(),
                NewArtifactEvents = workflowEvents.Where(e => e.Type == (int)DWorkflowEventType.NewArtifact).
                    Select(e => new IeNewArtifactEvent
                    {
                        Id = e.WorkflowEventId,
                        Name = e.Name,
                        Triggers = DeserializeTriggersForWorkflowMode(e.Triggers, dataMaps, userIds, groupIds, mode)
                    }).Distinct().ToList(),
                Projects = GetProjects(workflowArtifactTypes, mode)
            };

            var allWebhookTriggers = ieWorkflow.TransitionEvents.Where(e => e.Triggers != null).SelectMany(e => e.Triggers)
                .Concat(ieWorkflow.NewArtifactEvents.Where(e => e.Triggers != null).SelectMany(e => e.Triggers)).ToList();

            await LookupWebhookActionsFromIds(allWebhookTriggers);

            await UpdateUserAndGroupInfo(ieWorkflow, userIds, groupIds);
            // Remove Property Change and New Artifact events if they do not have any triggers.
            ieWorkflow.PropertyChangeEvents.RemoveAll(e => e.Triggers.IsEmpty());
            ieWorkflow.NewArtifactEvents.RemoveAll(e => e.Triggers.IsEmpty());

            if (shouldDeleteInValidData)
            {
                var dataValidationResult =
                    await _workflowDataValidator.ValidateUpdateDataAsync(ieWorkflow, standardTypes);
                DeleteInValidDataFromExportedWorkflow(ieWorkflow, dataValidationResult);
            }

            return WorkflowHelper.NormalizeWorkflow(ieWorkflow);
        }

        private void DeleteInValidDataFromExportedWorkflow(IeWorkflow workflow,
            WorkflowDataValidationResult validationResult)
        {
            if (workflow != null)
            {
                if (!workflow.HasProcessArtifactType)
                {
                    DeleteUserStoriesAndTestCasesFromWorkflow(workflow);
                }

                foreach (var error in validationResult.Errors)
                {
                    switch (error.ErrorCode)
                    {
                        case WorkflowDataValidationErrorCodes.ProjectByIdNotFound:
                            workflow.Projects?.RemoveAll(q => q.IdSerializable == (int)error.Element);
                            break;
                        case WorkflowDataValidationErrorCodes.ProjectDuplicate:
                            workflow.Projects = workflow.Projects?.GroupBy(q => q.IdSerializable)
                                .Select(q => q.First())
                                .ToList();
                            break;
                        case WorkflowDataValidationErrorCodes.StandardArtifactTypeNotFoundById:
                            workflow.Projects?.ForEach(
                                q => q.ArtifactTypes?.RemoveAll(qu => qu.IdSerializable == (int)error.Element));
                            break;
                        case WorkflowDataValidationErrorCodes.InstanceGroupNotFoundById:
                            workflow.TransitionEvents?.ForEach(
                                q => q.PermissionGroups?.RemoveAll(qu => qu.Id == (int)error.Element));
                            break;
                        // triggers GenerateAction
                        case WorkflowDataValidationErrorCodes.GenerateChildArtifactsActionArtifactTypeNotFoundById:
                            DeleteInvalidArtifactTypeIdFromGenerateTriggersInWorkflow(workflow, (int)error.Element);
                            break;
                        // workflow.PropertyChangeEvents
                        case WorkflowDataValidationErrorCodes.PropertyNotFoundById:
                            DeleteInvalidPropertiesFromWorkflow(workflow, (int)error.Element, null);
                            break;
                        case WorkflowDataValidationErrorCodes.PropertyNotAssociated:
                            DeleteInvalidPropertiesFromWorkflow(workflow, null, (string)error.Element);
                            break;
                        // triggers EmailNotification
                        case WorkflowDataValidationErrorCodes.EmailNotificationActionPropertyTypeNotFoundById:
                            DeleteInvalidPropertiesFromWorkflow(workflow, (int)error.Element, null,
                                ActionTypes.EmailNotification);
                            break;
                        case WorkflowDataValidationErrorCodes.EmailNotificationActionPropertyTypeNotAssociated:
                            DeleteInvalidPropertiesFromWorkflow(workflow, null, (string)error.Element,
                                ActionTypes.EmailNotification);
                            break;
                        case WorkflowDataValidationErrorCodes.EmailNotificationActionUnacceptablePropertyType:
                            DeleteInvalidPropertiesFromWorkflow(workflow, null, (string)error.Element,
                                ActionTypes.EmailNotification);
                            break;
                        // triggers PropertyChange
                        case WorkflowDataValidationErrorCodes.PropertyChangeActionPropertyTypeNotFoundById:
                            DeleteInvalidPropertiesFromWorkflow(workflow, (int)error.Element, null,
                                ActionTypes.PropertyChange);
                            break;
                        case WorkflowDataValidationErrorCodes.PropertyChangeActionPropertyTypeNotAssociated:
                            DeleteInvalidPropertiesFromWorkflow(workflow, null, (string)error.Element,
                                ActionTypes.PropertyChange);
                            break;
                        // cases Validation Property value in trigger PropertyChangeAction
                        case WorkflowDataValidationErrorCodes.PropertyChangeActionNotChoicePropertyValidValuesNotApplicable:
                        case WorkflowDataValidationErrorCodes.PropertyChangeActionNotUserPropertyUsersGroupsNotApplicable:
                        case WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredPropertyValueEmpty:
                        case WorkflowDataValidationErrorCodes.PropertyChangeActionInvalidNumberFormat:
                        case WorkflowDataValidationErrorCodes.PropertyChangeActionInvalidNumberDecimalPlaces:
                        case WorkflowDataValidationErrorCodes.PropertyChangeActionNumberOutOfRange:
                        case WorkflowDataValidationErrorCodes.PropertyChangeActionInvalidDateFormat:
                        case WorkflowDataValidationErrorCodes.PropertyChangeActionDateOutOfRange:
                        case WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredUserPropertyPropertyValueNotApplicable:
                        case WorkflowDataValidationErrorCodes.PropertyChangeActionGroupNotFoundById:
                        case WorkflowDataValidationErrorCodes.PropertyChangeActionUserNotFoundById:
                        case WorkflowDataValidationErrorCodes.PropertyChangeActionChoicePropertyMultipleValidValuesNotAllowed:
                        case WorkflowDataValidationErrorCodes.PropertyChangeActionChoiceValueSpecifiedAsNotValidated:
                        case WorkflowDataValidationErrorCodes.PropertyChangeActionValidValueNotFoundById:
                            DeleteInvalidPropertiesFromWorkflow(workflow, null, (string)error.Element, ActionTypes.PropertyChange);
                            break;
                    }
                }
            }
        }

        private void DeleteUserStoriesAndTestCasesFromWorkflow(IeWorkflow ieWorkflow)
        {
            ieWorkflow.TransitionEvents?.ForEach(q => DeleteUserStoriesAndTestCasesFromGenerateTriggers(q.Triggers));

            ieWorkflow.NewArtifactEvents?.ForEach(q => DeleteUserStoriesAndTestCasesFromGenerateTriggers(q.Triggers));

            ieWorkflow.PropertyChangeEvents?.ForEach(q => DeleteUserStoriesAndTestCasesFromGenerateTriggers(q.Triggers));
        }

        private void DeleteInvalidArtifactTypeIdFromGenerateTriggersInWorkflow(IeWorkflow ieWorkflow, int element)
        {
            ieWorkflow.TransitionEvents?.ForEach(q => DeleteInvalidArtifactTypeIdFromGenerateTriggers(q.Triggers, element));

            ieWorkflow.PropertyChangeEvents?.ForEach(q => DeleteInvalidArtifactTypeIdFromGenerateTriggers(q.Triggers, element));

            ieWorkflow.NewArtifactEvents?.ForEach(q => DeleteInvalidArtifactTypeIdFromGenerateTriggers(q.Triggers, element));
        }

        private void DeleteUserStoriesAndTestCasesFromGenerateTriggers(List<IeTrigger> triggers)
        {
            triggers?.RemoveAll(
                queq => (queq.Action?.ActionType == ActionTypes.Generate) &&
                        (((IeGenerateAction)queq.Action).GenerateActionType ==
                         GenerateActionTypes.UserStories));

            triggers?.RemoveAll(
                queq => (queq.Action?.ActionType == ActionTypes.Generate) &&
                        (((IeGenerateAction)queq.Action).GenerateActionType ==
                         GenerateActionTypes.TestCases));
        }

        private void DeleteInvalidArtifactTypeIdFromGenerateTriggers(List<IeTrigger> triggers, int? artifactTypeId)
        {
            triggers?.RemoveAll(
                queq => (queq.Action?.ActionType == ActionTypes.Generate) &&
                        (((IeGenerateAction)queq.Action).GenerateActionType ==
                         GenerateActionTypes.Children) &&
                        ((IeGenerateAction)queq.Action).ArtifactTypeId == artifactTypeId);
        }

        private void DeleteInvalidPropertiesFromWorkflow(IeWorkflow workflow, int? propertyId, string propertyName,
            ActionTypes? actionTypes = null)
        {
            if (actionTypes == null)
            {
                if (propertyId != null)
                    workflow.PropertyChangeEvents?.RemoveAll(q => ((q.PropertyId == propertyId)));
                else if (!string.IsNullOrEmpty(propertyName))
                    workflow.PropertyChangeEvents?.RemoveAll(q => ((q.PropertyName == propertyName)));
            }

            if ((actionTypes == ActionTypes.EmailNotification))
            {
                if (propertyId != null)
                {
                    workflow.PropertyChangeEvents?.ForEach(q => q.Triggers?.RemoveAll(
                        qu => (qu.Action?.ActionType == ActionTypes.EmailNotification) &&
                              ((IeEmailNotificationAction)qu.Action)
                              .PropertyId.GetValueOrDefault() == propertyId));

                    workflow.NewArtifactEvents?.ForEach(q => q.Triggers?.RemoveAll(
                        qu => (qu.Action?.ActionType == ActionTypes.EmailNotification) &&
                              ((IeEmailNotificationAction)qu.Action)
                              .PropertyId.GetValueOrDefault() == propertyId));

                    workflow.TransitionEvents?.ForEach(q => q.Triggers?.RemoveAll(
                        qu => (qu.Action?.ActionType == ActionTypes.EmailNotification) &&
                              ((IeEmailNotificationAction)qu.Action)
                              .PropertyId.GetValueOrDefault() == propertyId));
                }
                else if (!string.IsNullOrEmpty(propertyName))
                {
                    workflow.PropertyChangeEvents?.ForEach(q => q.Triggers?.RemoveAll(
                        qu => (qu.Action?.ActionType == ActionTypes.EmailNotification) &&
                              ((IeEmailNotificationAction)qu.Action)
                              .PropertyName == propertyName));

                    workflow.NewArtifactEvents?.ForEach(q => q.Triggers?.RemoveAll(
                        qu => (qu.Action?.ActionType == ActionTypes.EmailNotification) &&
                              ((IeEmailNotificationAction)qu.Action)
                              .PropertyName == propertyName));

                    workflow.TransitionEvents?.ForEach(q => q.Triggers?.RemoveAll(
                        qu => (qu.Action?.ActionType == ActionTypes.EmailNotification) &&
                              ((IeEmailNotificationAction)qu.Action)
                              .PropertyName == propertyName));
                }
            }

            if (actionTypes == ActionTypes.PropertyChange)
            {
                if (propertyId != null)
                {
                    workflow.PropertyChangeEvents?.ForEach(q => q.Triggers?.RemoveAll(
                        qu => (qu.Action?.ActionType == ActionTypes.PropertyChange) &&
                              ((IePropertyChangeAction)qu.Action)
                              .PropertyId.GetValueOrDefault() == propertyId));

                    workflow.NewArtifactEvents?.ForEach(q => q.Triggers?.RemoveAll(
                        qu => (qu.Action?.ActionType == ActionTypes.PropertyChange) &&
                              ((IePropertyChangeAction)qu.Action)
                              .PropertyId.GetValueOrDefault() == propertyId));

                    workflow.TransitionEvents?.ForEach(q => q.Triggers?.RemoveAll(
                        qu => (qu.Action?.ActionType == ActionTypes.PropertyChange) &&
                              ((IePropertyChangeAction)qu.Action)
                              .PropertyId.GetValueOrDefault() == propertyId));
                }
                else if (!string.IsNullOrEmpty(propertyName))
                {
                    workflow.PropertyChangeEvents?.ForEach(q => q.Triggers?.RemoveAll(
                        qu => (qu.Action?.ActionType == ActionTypes.PropertyChange) &&
                              ((IePropertyChangeAction)qu.Action)
                              .PropertyName == propertyName));

                    workflow.NewArtifactEvents?.ForEach(q => q.Triggers?.RemoveAll(
                        qu => (qu.Action?.ActionType == ActionTypes.PropertyChange) &&
                              ((IePropertyChangeAction)qu.Action)
                              .PropertyName == propertyName));

                    workflow.TransitionEvents?.ForEach(q => q.Triggers?.RemoveAll(
                        qu => (qu.Action?.ActionType == ActionTypes.PropertyChange) &&
                              ((IePropertyChangeAction)qu.Action)
                              .PropertyName == propertyName));
                }
            }
        }

        private static string GetPropertyChangedName(int? propertyTypeId, WorkflowDataNameMaps dataMaps)
        {
            string name = null;
            if (propertyTypeId != null
                && !WorkflowHelper.TryGetNameOrDescriptionPropertyTypeName(propertyTypeId.Value, out name))
            {
                dataMaps.PropertyTypeMap.TryGetValue(propertyTypeId.Value, out name);
            }
            return name;
        }

        private static List<IeProject> GetProjects(IEnumerable<SqlWorkflowArtifactTypes> wpa, WorkflowMode mode)
        {
            var listWpa = wpa.ToList();
            var wprojects = listWpa.GroupBy(g => new { id = g.ProjectId, path = g.ProjectPath }).Select(w => w).ToList();

            var projects = new List<IeProject>();
            wprojects.ForEach(w =>
            {
                var project = new IeProject
                {
                    Id = w.Key.id,
                    Path = mode == WorkflowMode.Canvas ? w.Key.path : null,
                    ArtifactTypes = listWpa.Where(p => p.ProjectId == w.Key.id).
                        Select(a => new IeArtifactType
                        {
                            Id = a.ArtifactTypeId,
                            Name = a.ArtifactTypeName
                        }).Distinct().ToList()
                };
                projects.Add(project);
            });
            return projects;
        }

        private static List<IeGroup> DeserializePermissionGroups(string xGroups, ISet<int> groupIdsToCollect)
        {
            List<IeGroup> groups = new List<IeGroup>();
            var xmlGroups = SerializationHelper.FromXml<XmlTriggerPermissions>(xGroups);

            xmlGroups?.GroupIds?.ForEach(gid =>
            {
                // Name property will be assigned later after converting the entire workflow
                // since we need to know all group Ids to retrieve group information form the database.
                var group = new IeGroup
                {
                    Id = gid
                };

                groups.Add(group);
                groupIdsToCollect.Add(gid);
            });

            return groups.Count == 0 ? null : groups;
        }

        private static bool? GetSkipPermissionGroup(string xGroups)
        {
            bool? skip = null;
            var xmlGroups = SerializationHelper.FromXml<XmlTriggerPermissions>(xGroups);

            if (xmlGroups != null)
            {
                skip = xmlGroups.Skip > 0;
            }
            return skip;
        }

        private List<IeTrigger> DeserializeTriggersForWorkflowMode(string triggers, WorkflowDataNameMaps dataMaps,
            ISet<int> userIdsToCollect, ISet<int> groupIdsToCollect, WorkflowMode mode)
        {
            var xmlTriggers = SerializationHelper.FromXml<XmlWorkflowEventTriggers>(triggers);

            var ieTriggers = _triggerConverter.FromXmlModel(xmlTriggers, dataMaps, userIdsToCollect, groupIdsToCollect).ToList();

            if (ieTriggers.IsEmpty())
            {
                return null;
            }

            if (mode == WorkflowMode.Canvas)
            {
                return ieTriggers.Where(t => t.Action?.ActionType != ActionTypes.Webhook).ToList();
            }

            return ieTriggers;
        }

        private static WorkflowDataNameMaps LoadDataMaps(ProjectTypes standardTypes, IDictionary<int, string> stateMap)
        {
            var dataMaps = new WorkflowDataNameMaps();
            dataMaps.StateMap.AddRange(stateMap);

            standardTypes.ArtifactTypes.ForEach(t => dataMaps.ArtifactTypeMap.Add(t.Id, t.Name));
            standardTypes.PropertyTypes.ForEach(t => dataMaps.PropertyTypeMap.Add(t.Id, t.Name));
            standardTypes.PropertyTypes.Where(t => t.PrimitiveType == PropertyPrimitiveType.Choice)
                .ForEach(t => t?.ValidValues.Where(vv => vv.Id.HasValue)
                .ForEach(vv => dataMaps.ValidValueMap.Add(vv.Id.Value, vv.Value)));

            return dataMaps;
        }

        #region UpdateUserAndGroupInfo

        private async Task UpdateUserAndGroupInfo(IeWorkflow workflow, ISet<int> userIds, ISet<int> groupIds)
        {
            // CollectUserAndGroupIds(workflow, out userIds, out groupIds);

            var userMap = (await _usersRepository.GetExistingUsersByIdsAsync(userIds))
                .ToDictionary(u => u.UserId, u => Tuple.Create(u.Login, u.DisplayName));
            var groupMap = (await _usersRepository.GetExistingGroupsByIds(groupIds, false))
                .ToDictionary(g => g.GroupId, g => Tuple.Create(g.Name, g.ProjectId));
            UpdateUserAndGroupInfo(workflow, userMap, groupMap);
        }

        private static void UpdateUserAndGroupInfo(IeWorkflow workflow, IDictionary<int, Tuple<string, string>> userMap, IDictionary<int, Tuple<string, int?>> groupMap)
        {
            var notFoundIds = new HashSet<int>();
            workflow.TransitionEvents?.ForEach(t =>
            {
                t?.PermissionGroups?.ForEach(g =>
                {
                    if (!g.Id.HasValue)
                    {
                        return;
                    }
                    Tuple<string, int?> groupInfo;
                    if (groupMap.TryGetValue(g.Id.Value, out groupInfo))
                    {
                        g.Name = groupInfo.Item1;
                    }
                    else
                    {
                        notFoundIds.Add(g.Id.Value);
                    }
                });
            });
            workflow.TransitionEvents?.ForEach(t => t?.PermissionGroups?.RemoveAll(g => g.Id.HasValue && notFoundIds.Contains(g.Id.Value)));

            workflow.TransitionEvents?.ForEach(te => UpdateUserAndGroupInfo(te, userMap, groupMap));
            workflow.PropertyChangeEvents?.ForEach(pce => UpdateUserAndGroupInfo(pce, userMap, groupMap));
            workflow.NewArtifactEvents?.ForEach(nae => UpdateUserAndGroupInfo(nae, userMap, groupMap));
        }

        private static void UpdateUserAndGroupInfo(IeEvent wEvent, IDictionary<int, Tuple<string, string>> userMap, IDictionary<int, Tuple<string, int?>> groupMap)
        {
            var notFoundGroupIds = new HashSet<int>();
            var notFoundUserIds = new HashSet<int>();
            wEvent?.Triggers?.ForEach(t =>
            {
                if (t?.Action?.ActionType == ActionTypes.PropertyChange)
                {
                    var action = (IePropertyChangeAction)t.Action;
                    action.UsersGroups?.UsersGroups?.ForEach(ug =>
                    {
                        if (!ug.Id.HasValue)
                        {
                            return;
                        }
                        if (ug.IsGroup.GetValueOrDefault())
                        {
                            Tuple<string, int?> groupInfo;
                            if (groupMap.TryGetValue(ug.Id.Value, out groupInfo))
                            {
                                ug.Name = groupInfo.Item1;
                                ug.GroupProjectId = groupInfo.Item2;
                            }
                            else
                            {
                                notFoundGroupIds.Add(ug.Id.Value);
                            }
                        }
                        else
                        {
                            Tuple<string, string> user;
                            if (userMap.TryGetValue(ug.Id.Value, out user))
                            {
                                ug.Name = user.Item1;
                                ug.DisplayName = user.Item2;
                            }
                            else
                            {
                                notFoundUserIds.Add(ug.Id.Value);
                            }
                        }
                    });
                }
            });
            wEvent?.Triggers?.Where(t => t?.Action?.ActionType == ActionTypes.PropertyChange).Select(t => t.Action)
                .OfType<IePropertyChangeAction>().ForEach(a => a?.UsersGroups?.UsersGroups?.RemoveAll(ug =>
                ug.Id.HasValue && ((ug.IsGroup.GetValueOrDefault() && notFoundGroupIds.Contains(ug.Id.Value))
                || (!ug.IsGroup.GetValueOrDefault() && notFoundUserIds.Contains(ug.Id.Value)))));
        }

        #endregion

        private async Task<string> UploadErrorsToFileStoreAsync(string errors)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(errors ?? string.Empty)))
            {
                return
                    await
                        FileRepository.UploadFileAsync(WorkflowImportErrorsFile, null, stream,
                            DateTime.UtcNow + TimeSpan.FromDays(1));
            }
        }

        private static void ReplaceNewLinesInNames(IeWorkflow workflow)
        {
            if (workflow == null)
            {
                return;
            }

            workflow.Name = ReplaceNewLines(workflow.Name);
            workflow.States?.ForEach(s => s.Name = ReplaceNewLines(s.Name));
            workflow.TransitionEvents?.ForEach(e => e.Name = ReplaceNewLines(e.Name));
            workflow.PropertyChangeEvents?.ForEach(e => e.Name = ReplaceNewLines(e.Name));
            workflow.NewArtifactEvents?.ForEach(e => e.Name = ReplaceNewLines(e.Name));
        }

        private static string ReplaceNewLines(string text)
        {
            return text?.Replace("\n", string.Empty).Replace("\r", string.Empty);
        }

        private static WorkflowXmlValidationResult ValidateWorkflowId(IeWorkflow workflow, int workflowId)
        {
            if (workflow == null)
            {
                throw new ArgumentNullException(nameof(workflow));
            }

            var result = new WorkflowXmlValidationResult();

            if (workflowId != workflow.Id)
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    ErrorCode = WorkflowXmlValidationErrorCodes.WorkflowIdDoesNotMatchIdInUrl
                });
            }

            return result;
        }

        private async Task ReplaceProjectPathsWithIdsAsync(IeWorkflow workflow)
        {
            var projectPaths = workflow.Projects?.Where(p => !p.Id.HasValue && !string.IsNullOrWhiteSpace(p.Path))
                .Select(p => p.Path).ToHashSet() ?? new HashSet<string>();

            var wEvents = new List<IeEvent>();
            var groups = new List<IeUserGroup>();

            if (!workflow.TransitionEvents.IsEmpty()) wEvents.AddRange(workflow.TransitionEvents);
            if (!workflow.PropertyChangeEvents.IsEmpty()) wEvents.AddRange(workflow.PropertyChangeEvents);
            if (!workflow.NewArtifactEvents.IsEmpty()) wEvents.AddRange(workflow.NewArtifactEvents);

            wEvents.ForEach(e => e.Triggers?.ForEach(t =>
            {
                if (t?.Action?.ActionType != ActionTypes.PropertyChange)
                {
                    return;
                }

                var pcAction = (IePropertyChangeAction)t.Action;
                pcAction.UsersGroups?.UsersGroups?.Where(ug => ug.IsGroup.GetValueOrDefault()
                                                  && !ug.GroupProjectId.HasValue
                                                  && !string.IsNullOrWhiteSpace(ug.GroupProjectPath)).ForEach(ug =>
                                                  {
                                                      projectPaths.Add(ug.GroupProjectPath);
                                                      groups.Add(ug);
                                                  });
            }));

            if (projectPaths.IsEmpty())
            {
                return;
            }

            var projectMap = (await _workflowRepository.GetProjectIdsByProjectPathsAsync(projectPaths))
                .ToDictionary(p => p.ProjectPath, p => p.ProjectId);

            workflow.Projects?.Where(p => !p.Id.HasValue && !string.IsNullOrWhiteSpace(p.Path)).ForEach(p =>
            {
                int id;
                if (projectMap.TryGetValue(p.Path, out id))
                {
                    p.Id = id;
                    p.Path = null;
                }
            });

            groups.ForEach(g =>
            {
                int id;
                if (projectMap.TryGetValue(g.GroupProjectPath, out id))
                {
                    g.GroupProjectId = id;
                    g.GroupProjectPath = null;
                }
            });
        }

        private static IEnumerable<WorkflowDataValidationError> ValidateAndRemoveNotFoundByIdInCurrentWorkflow(
            IeWorkflow workflow, WorkflowDiffResult workflowDiffResult)
        {
            var errors = new List<WorkflowDataValidationError>();

            if (!workflow.States.IsEmpty())
            {
                for (var i = workflow.States.Count - 1; i >= 0; i--)
                {
                    var s = workflow.States.ElementAt(i);
                    if (workflowDiffResult.NotFoundStates.Contains(s))
                    {
                        errors.Add(new WorkflowDataValidationError
                        {
                            Element = s,
                            ErrorCode = WorkflowDataValidationErrorCodes.StateNotFoundByIdInCurrent
                        });
                        workflow.States.Remove(s);
                    }
                }
            }


            if (workflowDiffResult.NotFoundEvents.Any())
            {
                if (!workflow.TransitionEvents.IsEmpty())
                {
                    for (var i = workflow.TransitionEvents.Count - 1; i >= 0; i--)
                    {
                        var te = workflow.TransitionEvents.ElementAt(i);
                        if (workflowDiffResult.NotFoundEvents.Contains(te))
                        {
                            errors.Add(new WorkflowDataValidationError
                            {
                                Element = te,
                                ErrorCode = WorkflowDataValidationErrorCodes.TransitionEventNotFoundByIdInCurrent
                            });
                            workflow.TransitionEvents.Remove(te);
                        }
                    }
                }

                if (!workflow.PropertyChangeEvents.IsEmpty())
                {
                    for (var i = workflow.PropertyChangeEvents.Count - 1; i >= 0; i--)
                    {
                        var pce = workflow.PropertyChangeEvents.ElementAt(i);
                        if (workflowDiffResult.NotFoundEvents.Contains(pce))
                        {
                            errors.Add(new WorkflowDataValidationError
                            {
                                Element = pce,
                                ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeEventNotFoundByIdInCurrent
                            });
                            workflow.PropertyChangeEvents.Remove(pce);
                        }
                    }
                }

                if (!workflow.NewArtifactEvents.IsEmpty())
                {
                    for (var i = workflow.NewArtifactEvents.Count - 1; i >= 0; i--)
                    {
                        var nae = workflow.NewArtifactEvents.ElementAt(i);
                        if (workflowDiffResult.NotFoundEvents.Contains(nae))
                        {
                            errors.Add(new WorkflowDataValidationError
                            {
                                Element = nae,
                                ErrorCode =
                                    WorkflowDataValidationErrorCodes.NewArtifactEventNotFoundByIdInCurrent
                            });
                            workflow.NewArtifactEvents.Remove(nae);
                        }
                    }
                }
            }

            if (workflowDiffResult.NotFoundProjectArtifactTypes.Any())
            {
                workflow.Projects?.ForEach(p =>
                {
                    if (p.ArtifactTypes.IsEmpty())
                    {
                        return;
                    }

                    for (var i = p.ArtifactTypes.Count - 1; i >= 0; i--)
                    {
                        var at = p.ArtifactTypes.ElementAt(i);
                        if (workflowDiffResult.NotFoundProjectArtifactTypes.Contains(
                            new KeyValuePair<int, IeArtifactType>(p.Id.Value, at)))
                        {
                            errors.Add(new WorkflowDataValidationError
                            {
                                Element = Tuple.Create(p, at),
                                ErrorCode =
                                    WorkflowDataValidationErrorCodes.ProjectArtifactTypeNotFoundByIdInCurrent
                            });
                            p.ArtifactTypes.Remove(at);
                        }
                    }
                });
                workflow.Projects?.RemoveAll(p => p.ArtifactTypes.IsEmpty());
            }

            return errors;
        }

        private static void AssignStateOrderIndexes(WorkflowDiffResult workflowDiffResult, IDictionary<int, float> currentOrderIndexes)
        {
            // We do not diff order indexes.
            // The order index for existing states does not change.
            // New states go to the end of the list.
            // Later the client application will be managing the order index.
            float changedMaxIndexOrder = 0;
            workflowDiffResult.ChangedStates?.ForEach(s =>
            {
                s.OrderIndex = currentOrderIndexes[s.Id.Value];
                changedMaxIndexOrder = s.OrderIndex > changedMaxIndexOrder ? s.OrderIndex : changedMaxIndexOrder;
            });

            var i = 1;
            workflowDiffResult.AddedStates?.ForEach(s => s.OrderIndex = changedMaxIndexOrder + 10 * i++);
        }

        private NumberOfStatesActions GetNumberOfStatesAndActions(IeWorkflow workflow)
        {
            if (workflow == null)
            {
                return null;
            }

            var numberStates = workflow.States?.Count ?? 0;

            var transitionEventTriggersCount = 0;
            if (workflow.TransitionEvents != null)
            {
                foreach (var transition in workflow.TransitionEvents)
                {
                    if (transition.Triggers != null)
                        transitionEventTriggersCount += transition.Triggers.Count;
                }
            }

            var propertyChangeEventTriggersCount = 0;
            if (workflow.PropertyChangeEvents != null)
            {
                foreach (var propertyChangeEvent in workflow.PropertyChangeEvents)
                {
                    if (propertyChangeEvent.Triggers != null)
                        propertyChangeEventTriggersCount += propertyChangeEvent.Triggers.Count;
                }
            }

            var newArtifactEventTriggersCount = 0;
            if (workflow.NewArtifactEvents != null)
            {
                foreach (var newArtifactEvent in workflow.NewArtifactEvents)
                {
                    if (newArtifactEvent.Triggers != null)
                        newArtifactEventTriggersCount += newArtifactEvent.Triggers.Count;
                }
            }

            return new NumberOfStatesActions { NumberOfActions = transitionEventTriggersCount + propertyChangeEventTriggersCount + newArtifactEventTriggersCount, NumberOfStates = numberStates };
        }

        #region Update workflow entities for the workflow update via the import.

        private async Task UpdateWorkflowEntitiesAsync(IeWorkflow workflow, WorkflowDiffResult workflowDiffResult, WorkflowDataValidationResult dataValidationResult, int publishRevision, IDbTransaction transaction, WorkflowMode workflowMode = WorkflowMode.Xml)
        {
            if (workflowDiffResult.IsWorkflowPropertiesChanged)
            {
                await UpdateWorkflowPropertiesAsync(workflow, publishRevision, transaction);
            }

            var stateMap = await UpdateWorkflowStatesAsync(workflow.Id.Value, workflowDiffResult, publishRevision, transaction, workflowMode);
            var dataMaps = CreateDataMap(dataValidationResult, stateMap);

            await UpdateWebhooksAsync(workflow.Id.Value, workflowDiffResult, dataMaps, transaction);

            await UpdateWorkflowEventsAsync(workflow.Id.Value, workflowDiffResult, dataMaps, publishRevision, transaction, workflowMode);

            await UpdateArtifactAssociationsAsync(workflow.Id.Value, workflowDiffResult, transaction);
        }

        private async Task UpdateWorkflowPropertiesAsync(IeWorkflow workflow, int publishRevision, IDbTransaction transaction)
        {
            var sqlWorkflows = new List<SqlWorkflow>
            {
                new SqlWorkflow
                {
                    Name = workflow.Name,
                    Description = workflow.Description,
                    Active = false, // updated workflows should be inactive. Users need explicitly activate workflows via UI.
                    WorkflowId = workflow.Id.Value
                }
            };

            await _workflowRepository.UpdateWorkflowsAsync(sqlWorkflows, publishRevision, transaction);
        }

        private async Task<IDictionary<string, int>> UpdateWorkflowStatesAsync(int workflowId, WorkflowDiffResult workflowDiffResult, int publishRevision, IDbTransaction transaction, WorkflowMode workflowMode = WorkflowMode.Xml)
        {
            var stateMap = new Dictionary<string, int>(workflowDiffResult.UnchangedStates.ToDictionary(s => s.Name, s => s.Id.Value));

            if (workflowDiffResult.DeletedStates.Any())
            {
                await _workflowRepository.DeleteWorkflowStatesAsync(workflowDiffResult.DeletedStates.Select(s => s.Id.Value),
                    publishRevision, transaction);
            }

            if (workflowDiffResult.AddedStates.Any())
            {
                var newStates = await _workflowRepository.CreateWorkflowStatesAsync(workflowDiffResult.AddedStates.Select(s =>
                    ToSqlState(s, workflowId, workflowMode)), publishRevision, transaction);
                stateMap.AddRange(newStates.ToDictionary(s => s.Name, s => s.WorkflowStateId));
            }

            if (workflowDiffResult.ChangedStates.Any())
            {
                var updatedStates = (await _workflowRepository.UpdateWorkflowStatesAsync(workflowDiffResult.ChangedStates.Select(s =>
                    ToSqlState(s, workflowId, workflowMode)), publishRevision, transaction)).ToList();

                Debug.Assert(workflowDiffResult.ChangedStates.Select(s => s.Id.Value).ToHashSet()
                    .SetEquals(updatedStates.Select(s => s.WorkflowStateId).ToHashSet()),
                    "Ids of updated Workflow States do not match Ids of the input Workflow States parameter.");

                stateMap.AddRange(updatedStates.ToDictionary(s => s.Name, s => s.WorkflowStateId));
            }

            return stateMap;
        }

        private async Task UpdateWorkflowEventsAsync(int workflowId, WorkflowDiffResult workflowDiffResult, WorkflowDataMaps dataMaps, int publishRevision, IDbTransaction transaction, WorkflowMode workflowMode = WorkflowMode.Xml)
        {
            if (workflowDiffResult.DeletedEvents.Any())
            {
                await
                    _workflowRepository.DeleteWorkflowEventsAsync(
                        workflowDiffResult.DeletedEvents.Select(s => s.Id.Value),
                        publishRevision, transaction);
            }

            if (workflowDiffResult.AddedEvents.Any())
            {
                var eventParam = workflowDiffResult.AddedEvents.Select(e => ToSqlWorkflowEvent(e, workflowId, dataMaps, workflowMode));
                await _workflowRepository.CreateWorkflowEventsAsync(eventParam, publishRevision, transaction);
            }

            if (workflowDiffResult.ChangedEvents.Any())
            {
                var eventParam = workflowDiffResult.ChangedEvents.Select(e => ToSqlWorkflowEvent(e, workflowId, dataMaps, workflowMode));
                var updatedEvents = await _workflowRepository.UpdateWorkflowEventsAsync(eventParam, publishRevision, transaction);

                Debug.Assert(workflowDiffResult.ChangedEvents.Select(s => s.Id.Value).ToHashSet()
                    .SetEquals(updatedEvents.Select(s => s.WorkflowEventId).ToHashSet()),
                    "Ids of updated Workflow Events do not match Ids of the input Workflow Events parameter.");
            }
        }

        private async Task UpdateArtifactAssociationsAsync(int workflowId, WorkflowDiffResult workflowDiffResult, IDbTransaction transaction)
        {
            var artifactTypeToAddKvPairs = workflowDiffResult.AddedProjectArtifactTypes.Select(pAt =>
                new KeyValuePair<int, string>(pAt.Key, pAt.Value.Name));

            var artifactTypeToDeleteKvPairs = workflowDiffResult.DeletedProjectArtifactTypes.Select(pAt =>
                new KeyValuePair<int, string>(pAt.Key, pAt.Value.Name));

            await _workflowRepository.UpdateWorkflowArtifactAssignmentsAsync(artifactTypeToAddKvPairs, artifactTypeToDeleteKvPairs,
                workflowId, transaction);
        }

        private static string DeserializeStateCanvasSettings(string settings)
        {
            string result = null;
            if (!string.IsNullOrEmpty(settings))
            {
                result = SerializationHelper.FromXml<XmlStateCanvasSettings>(settings).Location;
            }
            return result;
        }

        private static string SerializeStateCanvasSettings(string location)
        {
            return location != null ? SerializationHelper.ToXml(new XmlStateCanvasSettings { Location = location }) : null;
        }

        private static IePortPair DeserializeTransitionCanvasSettings(string settings)
        {
            IePortPair iePortPair = null;
            if (!string.IsNullOrEmpty(settings))
            {
                var portPair = SerializationHelper.FromXml<XmlTransitionCanvasSettings>(settings).XmlPortPair;
                iePortPair = new IePortPair { FromPort = (DiagramPort)portPair.FromPort, ToPort = (DiagramPort)portPair.ToPort };
            }
            return iePortPair;
        }


        private static string SerializeTransitionCanvasSettings(IePortPair iePortPair)
        {
            if (iePortPair == null) return null;

            return SerializationHelper.ToXml(new XmlTransitionCanvasSettings
            {
                XmlPortPair = new XmlPortPair
                {
                    FromPort = (int)iePortPair.FromPort,
                    ToPort = (int)iePortPair.ToPort
                }
            });
        }

        #endregion

        #region Webhooks
        private async Task CreateWebooksAsync(IeWorkflow workflow, int workflowId, IDbTransaction transaction, WorkflowDataMaps dataMaps)
        {
            var importWebhooksParams = new List<SqlWebhook>();

            workflow.TransitionEvents.OfType<IeTransitionEvent>().ForEach(e =>
            {
                importWebhooksParams.AddRange(ToSqlWebhooks(e, workflowId, dataMaps));
            });
            workflow.NewArtifactEvents.OfType<IeNewArtifactEvent>().ForEach(e =>
            {
                importWebhooksParams.AddRange(ToSqlWebhooks(e, workflowId, dataMaps));
            });

            var newWebhooks = await _workflowRepository.CreateWebhooks(importWebhooksParams, transaction);

            var index = 0;
            workflow.TransitionEvents.OfType<IeTransitionEvent>().ForEach(e =>
            {
                UpdateWebhooksDataMap(e, dataMaps, newWebhooks, ref index);
            });
            workflow.NewArtifactEvents.OfType<IeNewArtifactEvent>().ForEach(e =>
            {
                UpdateWebhooksDataMap(e, dataMaps, newWebhooks, ref index);
            });
        }

        private async Task UpdateWebhooksAsync(int workflowId, WorkflowDiffResult workflowDiffResult, WorkflowDataMaps dataMaps, IDbTransaction transaction)
        {
            if (workflowDiffResult.AddedEvents.Any())
            {
                var createWebhooksParams = new List<SqlWebhook>();
                workflowDiffResult.AddedEvents.ForEach(e => createWebhooksParams.AddRange(ToSqlWebhooks(e, workflowId, dataMaps)));
                if (createWebhooksParams.Any())
                {
                    var createdWebhooks = await _workflowRepository.CreateWebhooks(createWebhooksParams, transaction);

                    var index = 0;
                    workflowDiffResult.AddedEvents.ForEach(e =>
                    {
                        UpdateWebhooksDataMap(e, dataMaps, createdWebhooks, ref index);
                    });
                }
            }

            if (workflowDiffResult.ChangedEvents.Any())
            {
                // Need to handle situations where an existing webhook has been updated or a new webhook has been added within a changed event
                var updateWebhookParams = new List<SqlWebhook>();
                var createWebhooksParams = new List<SqlWebhook>();

                // Since a workflow event can contain both new and updated webhook actions, we need to handle each trigger individually
                foreach (var changedEvent in workflowDiffResult.ChangedEvents)
                {
                    foreach (var webhookAction in changedEvent.Triggers.Select(t => t.Action).OfType<IeWebhookAction>())
                    {
                        // If a webhook does not have an assigned Id, assume that it needs to be created
                        if (webhookAction.IdSerializable > 0)
                        {
                            updateWebhookParams.Add(ToSqlWebhook(changedEvent.EventType, webhookAction, workflowId, dataMaps));
                        }
                        else
                        {
                            createWebhooksParams.Add(ToSqlWebhook(changedEvent.EventType, webhookAction, workflowId, dataMaps));
                        }
                    }
                }

                var createdAndUpdatedWebhooks = new List<SqlWebhook>();
                // Updated all webhooks that already exist
                if (updateWebhookParams.Any())
                {
                    createdAndUpdatedWebhooks.AddRange(await _workflowRepository.UpdateWebhooks(updateWebhookParams, transaction));
                }

                // Create any newly added webhook
                if (createWebhooksParams.Any())
                {
                    createdAndUpdatedWebhooks.AddRange(await _workflowRepository.CreateWebhooks(createWebhooksParams, transaction));
                }

                // After All Webhooks have been created / updated, we need to go back and update our dataMap for all events
                if (createdAndUpdatedWebhooks.Any())
                {
                    var index = 0;
                    workflowDiffResult.ChangedEvents.ForEach(e =>
                    {
                        UpdateWebhooksDataMap(e, dataMaps, createdAndUpdatedWebhooks, ref index);
                    });
                }
            }
        }

        private List<SqlWebhook> ToSqlWebhooks(IeEvent wEvent, int workflowId, WorkflowDataMaps dataMaps)
        {
            // Bulk conversion of all webhook action triggers within an event to SqlWebhook
            var sqlWebhooks = new List<SqlWebhook>();

            if (wEvent == null || (wEvent.EventType != EventTypes.Transition && wEvent.EventType != EventTypes.NewArtifact))
            {
                return sqlWebhooks;
            }

            foreach (var action in wEvent.Triggers.Select(t => t.Action).OfType<IeWebhookAction>())
            {
                var sqlwebhook = new SqlWebhook
                {
                    WebhookId = action.IdSerializable,
                    Url = action.Url,
                    Scope = DWebhookScope.Workflow.ToString(),
                    State = true,
                    EventType = GetWebhookEventType(wEvent.EventType),
                    SecurityInfo = SerializeWebhookSecurityInfo(action),
                    WorkflowId = workflowId
                };
                sqlWebhooks.Add(sqlwebhook);
                dataMaps.WebhooksByActionObj.Add(action, action.IdSerializable);
            }

            return sqlWebhooks;
        }

        private SqlWebhook ToSqlWebhook(EventTypes eventType, IeWebhookAction webhookAction, int workflowId, WorkflowDataMaps dataMaps)
        {
            SqlWebhook sqlWebhook = null;
            if (eventType != EventTypes.Transition && eventType != EventTypes.NewArtifact)
            {
                return sqlWebhook;
            }

            sqlWebhook = new SqlWebhook()
            {
                WebhookId = webhookAction.IdSerializable,
                Url = webhookAction.Url,
                Scope = DWebhookScope.Workflow.ToString(),
                State = true,
                EventType = GetWebhookEventType(eventType),
                SecurityInfo = SerializeWebhookSecurityInfo(webhookAction),
                WorkflowId = workflowId
            };
            dataMaps.WebhooksByActionObj.Add(webhookAction, webhookAction.IdSerializable);

            return sqlWebhook;
        }

        private DWebhookEventType GetWebhookEventType(EventTypes eventType)
        {
            switch (eventType)
            {
                case EventTypes.NewArtifact:
                    return DWebhookEventType.ArtifactCreated;
                case EventTypes.PropertyChange:
                    return DWebhookEventType.None;
                case EventTypes.Transition:
                    return DWebhookEventType.WorkflowTransition;
                case EventTypes.None:
                    return DWebhookEventType.None;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), "Could not map  Workflow Event Type to Webhook Event Type.");
            }
        }

        private string SerializeWebhookSecurityInfo(IeWebhookAction webhook)
        {
            if (webhook == null)
            {
                return string.Empty;
            }

            XmlWebhookSecurityInfo securityInfo = new XmlWebhookSecurityInfo();
            if (webhook.ShouldSerializeIgnoreInvalidSSLCertificate())
            {
                securityInfo.IgnoreInvalidSSLCertificate = webhook.IgnoreInvalidSSLCertificate.Value;
            }
            if (webhook.ShouldSerializeHttpHeaders())
            {
                foreach (var header in webhook.HttpHeaders)
                {
                    securityInfo.HttpHeaders.Add(SystemEncryptions.Encrypt(header));
                }
            }

            if (webhook.ShouldSerializeBasicAuth())
            {
                securityInfo.BasicAuth = new XmlWebhookBasicAuth()
                {
                    Username = SystemEncryptions.Encrypt(webhook.BasicAuth.Username),
                    Password = SystemEncryptions.Encrypt(webhook.BasicAuth.Password)
                };
            }

            if (webhook.ShouldSerializeSignature())
            {
                securityInfo.Signature = new XmlWebhookSignature
                {
                    SecretToken = SystemEncryptions.Encrypt(webhook.Signature.SecretToken),
                    Algorithm = webhook.Signature.Algorithm ?? "HMACSHA256"
                };
            }

            return SerializationHelper.ToXml(securityInfo);
        }

        private void UpdateWebhooksDataMap(IeEvent e, WorkflowDataMaps dataMaps, IEnumerable<SqlWebhook> newWebhooks, ref int counter)
        {
            if (e == null)
            {
                return;
            }

            if (!e.Triggers.Any())
            {
                return;
            }

            if (!newWebhooks.Any())
            {
                return;
            }

            foreach (var trigger in e.Triggers)
            {
                if (trigger.Action.ActionType != ActionTypes.Webhook)
                {
                    continue;
                }

                var webhookAction = (IeWebhookAction)trigger.Action;
                if (webhookAction != null)
                {
                    if (!dataMaps.WebhooksByActionObj.ContainsKey(webhookAction))
                    {
                        throw new KeyNotFoundException("Webhook DataMap does not contain Webhook specificied within Trigger.");
                    }

                    // Webhook Actions are only added to DataMap after an SqlWebhook obj has been created in preparation of being updated/created within the DB
                    dataMaps.WebhooksByActionObj[webhookAction] = newWebhooks.ElementAt(counter).WebhookId;
                    counter++;
                }
            }
        }

        private async Task LookupWebhookActionsFromIds(List<IeTrigger> triggers)
        {
            var webhookIds = triggers.Select(t => t.Action).OfType<IeWebhookAction>().Select(a => (int)a.Id);

            var webhooks = await _workflowRepository.GetWebhooks(webhookIds);

            foreach (var webhook in webhooks)
            {
                var action = triggers.Select(t => t.Action).OfType<IeWebhookAction>().FirstOrDefault(a => a.Id == webhook.WebhookId);
                if (action != null)
                {
                    var securityInfo = SerializationHelper.FromXml<XmlWebhookSecurityInfo>(webhook.SecurityInfo);
                    action.Url = webhook.Url;
                    action.IgnoreInvalidSSLCertificate = securityInfo.IgnoreInvalidSSLCertificate;
                    if (securityInfo.HttpHeaders.Any())
                    {
                        action.HttpHeaders = new List<string>();
                        securityInfo.HttpHeaders.ForEach(h => action.HttpHeaders.Add(SystemEncryptions.Decrypt(h)));
                    }
                    if (securityInfo.BasicAuth != null)
                    {
                        action.BasicAuth = new IeBasicAuth
                        {
                            Username = SystemEncryptions.Decrypt(securityInfo.BasicAuth?.Username),
                            Password = SystemEncryptions.Decrypt(securityInfo.BasicAuth?.Password)
                        };
                    }
                    if (securityInfo.Signature != null)
                    {
                        action.Signature = new IeSignature
                        {
                            Algorithm = securityInfo.Signature?.Algorithm ?? "HMACSHA256",
                            SecretToken = SystemEncryptions.Decrypt(securityInfo.Signature?.SecretToken)
                        };
                    }
                }
            }
        }
        #endregion Webhooks
    }
}
