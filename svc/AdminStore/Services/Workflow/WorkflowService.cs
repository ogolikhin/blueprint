using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdminStore.Models.Workflow;
using AdminStore.Repositories;
using AdminStore.Repositories.Workflow;
using ArtifactStore.Helpers;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Repositories.Files;
using ServiceLibrary.Repositories.ProjectMeta;
using File = ServiceLibrary.Models.Files.File;

namespace AdminStore.Services.Workflow
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IUserRepository _userRepository;

        private readonly IWorkflowXmlValidator _workflowXmlValidator;
        private readonly IWorkflowDataValidator _workflowDataValidator;
        private readonly IWorkflowValidationErrorBuilder _workflowValidationErrorBuilder;
        private readonly ISqlProjectMetaRepository _projectMetaRepository;
        private readonly ITriggerConverter _triggerConverter;
        private readonly IWorkflowActionPropertyValueValidator _propertyValueValidator;
        private readonly IWorkflowDiff _workflowDiff;

        private const string WorkflowImportErrorsFile = "$workflow_import_errors$.txt";

        public WorkflowService()
            : this(new WorkflowRepository(), new WorkflowXmlValidator(), new SqlUserRepository(),
                  new WorkflowValidationErrorBuilder(), new SqlProjectMetaRepository(),
                  new TriggerConverter(), new WorkflowActionPropertyValueValidator(),
                  new WorkflowDiff())
        {
            _workflowDataValidator = new WorkflowDataValidator(_workflowRepository, _userRepository,
                _projectMetaRepository, _propertyValueValidator);
        }

        public WorkflowService(IWorkflowRepository workflowRepository,
            IWorkflowXmlValidator workflowXmlValidator,
            IUserRepository userRepository,
            IWorkflowValidationErrorBuilder workflowValidationErrorBuilder,
            ISqlProjectMetaRepository projectMetaRepository,
            ITriggerConverter triggerConverter,
            IWorkflowActionPropertyValueValidator propertyValueValidator,
            IWorkflowDiff workflowDiff)
        {
            _workflowRepository = workflowRepository;
            _workflowXmlValidator = workflowXmlValidator;
            _userRepository = userRepository;
            _workflowValidationErrorBuilder = workflowValidationErrorBuilder;
            _projectMetaRepository = projectMetaRepository;
            _triggerConverter = triggerConverter;
            _propertyValueValidator = propertyValueValidator;
            _workflowDiff = workflowDiff;
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

        public async Task<ImportWorkflowResult> ImportWorkflowAsync(IeWorkflow workflow, string fileName, int userId)
        {
            if (workflow == null)
            {
                throw new NullReferenceException(nameof(workflow));
            }

            var importResult = new ImportWorkflowResult();

            ReplaceNewLinesInNames(workflow);

            var xmlValidationResult = _workflowXmlValidator.ValidateXml(workflow);
            if (xmlValidationResult.HasErrors)
            {
                var textErrors = _workflowValidationErrorBuilder.BuildTextXmlErrors(xmlValidationResult.Errors, fileName);
                var guid = await UploadErrorsToFileStore(textErrors);

                importResult.ErrorsGuid = guid;
                importResult.ResultCode = ImportWorkflowResultCodes.InvalidModel;

#if DEBUG
                importResult.ErrorMessage = textErrors;
#endif

                return importResult;
            }

            var dataValidationResult = await _workflowDataValidator.ValidateData(workflow);
            if (dataValidationResult.HasErrors)
            {
                var textErrors = _workflowValidationErrorBuilder.BuildTextDataErrors(dataValidationResult.Errors, fileName);
                var guid = await UploadErrorsToFileStore(textErrors);

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
                var publishRevision = await _workflowRepository.CreateRevisionInTransactionAsync(transaction, userId, "Workflow import.");

                var importParams = new SqlWorkflow
                {
                    Name = workflow.Name,
                    Description = workflow.Description,
                    Active = false // imported workflows are inactive. Users need explicitly activate workflows via UI.
                };
                newWorkflow = (await _workflowRepository.CreateWorkflowsAsync(new[] { importParams }, publishRevision, transaction)).FirstOrDefault();

                if (newWorkflow != null)
                {
                    await ImportWorkflowComponentsAsync(workflow, newWorkflow, publishRevision, transaction, dataValidationResult, userId);
                    await _workflowRepository.UpdateWorkflowsChangedWithRevisions(newWorkflow.WorkflowId, publishRevision, transaction);

                    importResult.ResultCode = ImportWorkflowResultCodes.Ok;
                }
            };

            await _workflowRepository.RunInTransactionAsync(action);
            importResult.WorkflowId = newWorkflow?.WorkflowId;

            return importResult;

        }

        public async Task<ImportWorkflowResult> UpdateWorkflowViaImport(int workflowId, IeWorkflow workflow, string fileName, int userId)
        {
            var importResult = new ImportWorkflowResult();

            var xmlValidationResult = ValidateWorkflowId(workflow, workflowId);
            if (xmlValidationResult.HasErrors)
            {
                var textErrors = _workflowValidationErrorBuilder.BuildTextXmlErrors(xmlValidationResult.Errors, fileName);
                var guid = await UploadErrorsToFileStore(textErrors);

                importResult.ErrorsGuid = guid;
                importResult.ResultCode = ImportWorkflowResultCodes.InvalidModel;

#if DEBUG
                importResult.ErrorMessage = textErrors;
#endif

                return importResult;
            }

            var currentWorkflow = await GetWorkflowExportAsync(workflowId);
            if (currentWorkflow.IsActive)
            {
                var dataValidationErrors = new[]
                {
                    new WorkflowDataValidationError
                    {
                        Element = workflow,
                        ErrorCode = WorkflowDataValidationErrorCodes.WorkflowActive
                    }
                };
                var textErrors = _workflowValidationErrorBuilder.BuildTextDataErrors(dataValidationErrors, fileName, false);
                var guid = await UploadErrorsToFileStore(textErrors);

                importResult.ErrorsGuid = guid;
                importResult.ResultCode = ImportWorkflowResultCodes.Conflict;

#if DEBUG
                importResult.ErrorMessage = textErrors;
#endif

                return importResult;

            }

            xmlValidationResult = _workflowXmlValidator.ValidateUpdateXml(workflow);
            if (xmlValidationResult.HasErrors)
            {
                var textErrors = _workflowValidationErrorBuilder.BuildTextXmlErrors(xmlValidationResult.Errors, fileName);
                var guid = await UploadErrorsToFileStore(textErrors);

                importResult.ErrorsGuid = guid;
                importResult.ResultCode = ImportWorkflowResultCodes.Conflict;

#if DEBUG
                importResult.ErrorMessage = textErrors;
#endif

                return importResult;
            }

            await ReplaceProjectPathsWithIds(workflow);

            var workflowDiffResult = _workflowDiff.DiffWorkflows(workflow, currentWorkflow);

            var notFoundErrors = ValidateAndRemoveNotFoundByIdInCurrentWorkflow(workflow, workflowDiffResult);
            // Even if the validation of not found by Id in current has errors,
            // anyway we do the data validation.
            var dataValidationResult = await _workflowDataValidator.ValidateUpdateData(workflow);
            dataValidationResult.Errors.AddRange(notFoundErrors);

            if (!dataValidationResult.HasErrors && !workflowDiffResult.HasChanges)
            {
                dataValidationResult.Errors.Add(new WorkflowDataValidationError
                {
                    Element = workflow,
                    ErrorCode = WorkflowDataValidationErrorCodes.WorkflowNothingToUpdate
                });
            }


            if (dataValidationResult.HasErrors)
            {
                var textErrors = _workflowValidationErrorBuilder.BuildTextDataErrors(dataValidationResult.Errors, fileName);
                var guid = await UploadErrorsToFileStore(textErrors);

                importResult.ErrorsGuid = guid;
                importResult.ResultCode = ImportWorkflowResultCodes.Conflict;

#if DEBUG
                importResult.ErrorMessage = textErrors;
#endif

                return importResult;
            }

            Func<IDbTransaction, Task> action = async transaction =>
            {
                var publishRevision = await _workflowRepository.CreateRevisionInTransactionAsync(transaction, userId, "Workflow update via import.");

                // TODO: Update, Create, Delete in the database

                importResult.ResultCode = ImportWorkflowResultCodes.Ok;
            };

            await _workflowRepository.RunInTransactionAsync(action);

            importResult.ResultCode = ImportWorkflowResultCodes.Ok;
            return importResult;
        }

        public async Task<WorkflowDto> GetWorkflowDetailsAsync(int workflowId)
        {
            var workflowDetails = await _workflowRepository.GetWorkflowDetailsAsync(workflowId);
            if (workflowDetails == null)
            {
                throw new ResourceNotFoundException(ErrorMessages.WorkflowNotExist, ErrorCodes.ResourceNotFound);
            }

            var workflowDto = new WorkflowDto {Name = workflowDetails.Name, Description = workflowDetails.Description, Active = workflowDetails.Active, WorkflowId = workflowDetails.WorkflowId,
                VersionId = workflowDetails.VersionId}; 

            var workflowProjectsAndArtifactTypes = (await _workflowRepository.GetWorkflowArtifactTypesAsync(workflowId)).ToList();

            workflowDto.Projects = workflowProjectsAndArtifactTypes.Select(e => new WorkflowProjectDto {Id = e.ProjectId, Name = e.ProjectPath}).Distinct().ToList();
            workflowDto.ArtifactTypes = workflowProjectsAndArtifactTypes.Select(e => new WorkflowArtifactTypeDto {Name = e.ArtifactTypeName}).Distinct().ToList();

            return workflowDto;
        }
        public async Task UpdateWorkflowStatusAsync(StatusUpdate statusUpdate, int workflowId, int userId)
        {
            var existingWorkflow = await _workflowRepository.GetWorkflowDetailsAsync(workflowId);
            if (existingWorkflow == null)
            {
                throw new ResourceNotFoundException(ErrorMessages.WorkflowNotExist, ErrorCodes.ResourceNotFound);
            }

            if (existingWorkflow.VersionId != statusUpdate.VersionId)
            {
                throw new ConflictException(ErrorMessages.WorkflowVersionsNotEqual, ErrorCodes.Conflict);
            }

            var workflows = new List<SqlWorkflow> {new SqlWorkflow {Name = existingWorkflow.Name, Description = existingWorkflow.Description, Active = statusUpdate.Active, WorkflowId = workflowId} };

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

        private async Task ImportWorkflowComponentsAsync(IeWorkflow workflow, SqlWorkflow newWorkflow, int publishRevision, 
            IDbTransaction transaction, WorkflowDataValidationResult dataValidationResult, int userId)
        {
            var importStateParams = new List<SqlState>();

            float orderIndex = 0;
            workflow.States.ForEach(state =>
            {

                importStateParams.Add(new SqlState
                {
                    Name = state.Name,
                    WorkflowId = newWorkflow.WorkflowId,
                    Default = state.IsInitial.HasValue && state.IsInitial.Value,
                    OrderIndex = orderIndex
                });
                orderIndex += 10;
            });
            var newStates = (await _workflowRepository.CreateWorkflowStatesAsync(importStateParams, publishRevision, transaction)).ToList();

            var dataMaps = CreateDataMap(dataValidationResult, newStates);

            await ImportWorkflowEvents(workflow, newWorkflow.WorkflowId, publishRevision, transaction, newStates,
                dataMaps, userId);

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
                await _workflowRepository.CreateWorkflowArtifactAssociationsAsync(kvPairs,
                    newWorkflow.WorkflowId, publishRevision, transaction);
            }
        }

        private static WorkflowDataMaps CreateDataMap(WorkflowDataValidationResult dataValidationResult, List<SqlState> newStates)
        {
            var dataMaps = new WorkflowDataMaps();
            dataMaps.UserMap.AddRange(dataValidationResult.Users.ToDictionary(u => u.Login, u => u.UserId));
            dataMaps.GroupMap.AddRange(dataValidationResult.Groups.ToDictionary(u => Tuple.Create(u.Name, u.ProjectId), u => u.GroupId));
            dataMaps.StateMap.AddRange(newStates.ToDictionary(s => s.Name, s => s.WorkflowStateId));
            dataMaps.ArtifactTypeMap.AddRange(dataValidationResult.StandardTypes.ArtifactTypes.ToDictionary(at => at.Name, at => at.Id));
            dataValidationResult.StandardTypes.PropertyTypes.ForEach(pt =>
            {
                dataMaps.PropertyTypeMap.Add(pt.Name, pt.Id);

                if (pt.PrimitiveType == PropertyPrimitiveType.Choice)
                {
                    var vvMap = new Dictionary<string, int>();
                    pt.ValidValues?.ForEach(vv =>
                    {
                        if (!vvMap.ContainsKey(vv.Value))
                        {
                            vvMap.Add(vv.Value, vv.Id.GetValueOrDefault());
                        }
                    });
                    dataMaps.ValidValueMap.Add(pt.Id, vvMap);
                }
            });
            dataMaps.PropertyTypeMap.Add(WorkflowConstants.PropertyNameName, WorkflowConstants.PropertyTypeFakeIdName);
            dataMaps.PropertyTypeMap.Add(WorkflowConstants.PropertyNameDescription, WorkflowConstants.PropertyTypeFakeIdDescription);

            return dataMaps;
        }

        private async Task ImportWorkflowEvents(IeWorkflow workflow, int newWorkflowId, int publishRevision,
            IDbTransaction transaction, IEnumerable<SqlState> newStates, WorkflowDataMaps dataMaps, int userId)
        {
            var newStatesArray = newStates.ToArray();
            var importTriggersParams = new List<SqlWorkflowEvent>();

            workflow.TransitionEvents.OfType<IeTransitionEvent>().ForEach(e =>
            {
                importTriggersParams.Add(ToSqlWorkflowEvent(e, newWorkflowId, newStatesArray,
                    dataMaps, userId));
            });
            workflow.PropertyChangeEvents.OfType<IePropertyChangeEvent>().ForEach(e =>
            {
                importTriggersParams.Add(ToSqlWorkflowEvent(e, newWorkflowId, newStatesArray,
                    dataMaps, userId));
            });
            workflow.NewArtifactEvents.OfType<IeNewArtifactEvent>().ForEach(e =>
            {
                importTriggersParams.Add(ToSqlWorkflowEvent(e, newWorkflowId, newStatesArray,
                    dataMaps, userId));
            });

            await _workflowRepository.CreateWorkflowEventsAsync(importTriggersParams, publishRevision, transaction);
        }

        private SqlWorkflowEvent ToSqlWorkflowEvent(IeEvent wEvent, int newWorkflowId, ICollection<SqlState> newStates,
            WorkflowDataMaps dataMaps, int userId)
        {
            var sqlEvent = new SqlWorkflowEvent
            {
                Name = wEvent.Name,
                WorkflowId = newWorkflowId,
                Validations = null,
                Triggers = wEvent.Triggers == null ? null
                    : SerializationHelper.ToXml(_triggerConverter.ToXmlModel(wEvent.Triggers, dataMaps, userId))
            };

            switch (wEvent.EventType)
            {
                case EventTypes.Transition:
                    sqlEvent.Type = DWorkflowEventType.Transition;
                    var transition = (IeTransitionEvent) wEvent;
                    var skipPermissionGroups = transition.SkipPermissionGroups.GetValueOrDefault();
                    sqlEvent.Permissions = skipPermissionGroups || !transition.PermissionGroups.IsEmpty()
                        ? sqlEvent.Permissions = SerializationHelper.ToXml(new XmlTriggerPermissions
                        {
                            Skip = skipPermissionGroups ? 1 : (int?)null,
                            GroupIds = transition.PermissionGroups.Select(pg =>
                                dataMaps.GroupMap[Tuple.Create(pg.Name, (int?) null)]).ToList()
                        }) : null;


                    sqlEvent.WorkflowState1Id =
                        newStates.FirstOrDefault(s => s.Name.Equals(transition.FromState))?.WorkflowStateId;
                    sqlEvent.WorkflowState2Id =
                        newStates.FirstOrDefault(s => s.Name.Equals(transition.ToState))?.WorkflowStateId;
                    break;
                case EventTypes.PropertyChange:
                    sqlEvent.Type = DWorkflowEventType.PropertyChange;
                    var pcEvent = (IePropertyChangeEvent) wEvent;
                    int propertyTypeId;
                    if (!dataMaps.PropertyTypeMap.TryGetValue(pcEvent.PropertyName, out propertyTypeId))
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

        public async Task<IeWorkflow> GetWorkflowExportAsync(int workflowId)
        {
            SqlWorkflow workflowDetails = await _workflowRepository.GetWorkflowDetailsAsync(workflowId);
            if (workflowDetails == null)
            {
                throw new ResourceNotFoundException(ErrorMessages.WorkflowNotExist, ErrorCodes.ResourceNotFound);
            }
            var workflowArtifactTypes = (await _workflowRepository.GetWorkflowArtifactTypesAsync(workflowId)).ToList();
            var workflowStates = (await _workflowRepository.GetWorkflowStatesAsync(workflowId)).ToList();
            var workflowEvents = (await _workflowRepository.GetWorkflowEventsAsync(workflowId)).ToList();

            WorkflowDataNameMaps dataMaps = await LoadDataMaps(workflowDetails.WorkflowId);

            IeWorkflow ieWorkflow = new IeWorkflow
            {
                Id = workflowDetails.WorkflowId,
                Name = workflowDetails.Name,
                Description = workflowDetails.Description,
                IsActive = workflowDetails.Active,
                States = workflowStates.Select(e => new IeState { Id = e.WorkflowStateId, IsInitial = e.Default, Name = e.Name }).Distinct().ToList(),
                TransitionEvents = workflowEvents.Where(e => e.Type == (int)DWorkflowEventType.Transition).
                    Select(e => new IeTransitionEvent
                    {
                        Id = e.WorkflowEventId,
                        Name = e.Name,
                        FromStateId = e.FromStateId,
                        FromState = e.FromState,
                        ToState = e.ToState,
                        ToStateId = e.ToStateId,
                        PermissionGroups = DeserializePermissionGroups(e.Permissions, dataMaps),
                        SkipPermissionGroups = GetSkipPermissionGroup(e.Permissions),
                        Triggers = DeserializeTriggers(e.Triggers, dataMaps)
                    }).Distinct().ToList(),
                PropertyChangeEvents = workflowEvents.Where(e => e.Type == (int)DWorkflowEventType.PropertyChange).
                    Select(e => new IePropertyChangeEvent
                    {
                        Id = e.WorkflowEventId,
                        Name = e.Name,
                        PropertyId = e.PropertyTypeId,
                        PropertyName = GetPropertyChangedName(e.PropertyTypeId, dataMaps),
                        Triggers = DeserializeTriggers(e.Triggers, dataMaps)
                    }).Distinct().ToList(),
                NewArtifactEvents = workflowEvents.Where(e => e.Type == (int)DWorkflowEventType.NewArtifact).
                    Select(e => new IeNewArtifactEvent
                    {
                        Id = e.WorkflowEventId,
                        Name = e.Name,
                        Triggers = DeserializeTriggers(e.Triggers, dataMaps)
                    }).Distinct().ToList(),
                Projects = GetProjects(workflowArtifactTypes)
            };

            return ieWorkflow;
        }
       
        private string GetPropertyChangedName(int? propertyTypeId, WorkflowDataNameMaps dataMaps)
        {
            string name = null;
            if (propertyTypeId != null)
            {
                dataMaps.PropertyTypeMap.TryGetValue((int)propertyTypeId, out name);
            }
            return name;
        }
        private List<IeProject> GetProjects(IEnumerable<SqlWorkflowArtifactTypes> wpa)
        {
            var wprojects = wpa.GroupBy(g => g.ProjectId).Select(w => w).ToList(); 

            List<IeProject> projects = new List<IeProject>();
            foreach (var w in wprojects)
            {
                var project = new IeProject
                {
                    Id = w.Key,
                    ArtifactTypes = wpa.Where(p => p.ProjectId == w.Key).
                    Select(a => new IeArtifactType
                    {
                        Id = a.ArtifactTypeId,
                        Name = a.ArtifactTypeName
                    }).Distinct().ToList()
                };
                projects.Add(project);
            };
            return projects;
        }

        private List<IeGroup> DeserializePermissionGroups(string xGroups, WorkflowDataNameMaps dataMaps)
        {
            List<IeGroup> groups = new List<IeGroup>();
            var xmlGroups = SerializationHelper.FromXml<XmlTriggerPermissions>(xGroups);
            if (xmlGroups != null)
            {
                foreach (var gid in xmlGroups.GroupIds)
                {
                    Tuple<string, int?> nameProjectId;
                    dataMaps.GroupMap.TryGetValue(gid, out nameProjectId);
                    var group = new IeGroup
                    {
                        Id = gid,
                        Name = nameProjectId?.Item1
                    };

                    groups.Add(group);
                }
            }
            return groups.Count == 0 ? null : groups;
        }
        private bool? GetSkipPermissionGroup(string xGroups)
        {
            bool? skip = null;
            var xmlGroups = SerializationHelper.FromXml<XmlTriggerPermissions>(xGroups);
            
            if (xmlGroups != null)
            {
                skip = xmlGroups.Skip > 0;
            }
            return skip;
        }

        private List<IeTrigger> DeserializeTriggers(string triggers, WorkflowDataNameMaps dataMaps)
        {
            var xmlTriggers = SerializationHelper.FromXml<XmlWorkflowEventTriggers>(triggers);

            List<IeTrigger> ieTriggers = _triggerConverter.FromXmlModel(xmlTriggers, dataMaps) as List<IeTrigger>;

            return ieTriggers;
        }

        private async Task<WorkflowDataNameMaps> LoadDataMaps(int workflowId)
        {
            var dataMaps = new WorkflowDataNameMaps();

            dataMaps.UserMap.AddRange(await GetUsersMap());
            dataMaps.GroupMap.AddRange(await GetGroupsMap());
            dataMaps.StateMap.AddRange(await GetStatesMap(workflowId));
            dataMaps.ArtifactTypeMap.AddRange(await GetArtifactTypesMap());
            dataMaps.PropertyTypeMap.AddRange(await GetPropertyTypesMap());
            dataMaps.ValidValueMap.AddRange(await GetValidValueMap());

            return dataMaps;
        }

        private async Task<Dictionary<int, string>> GetUsersMap()
        {
            var map = new Dictionary<int, string>();

            // TODO: It does not work correctly if there are over 1000 users.
            // It has to be replaced later
            var result = await _userRepository.GetUsersAsync(new Pagination { Offset = 0, Limit = 1000 });
            if (result != null && result.Items != null)
            {
                result.Items.ForEach(u => map.Add(u.Id, u.Login));
            }

            return map;
        }

        private async Task<Dictionary<int, Tuple<string, int?>>> GetGroupsMap()
        {
            var map = new Dictionary<int, Tuple<string, int?>>();

            var groups = await _userRepository.GetGroupsMapAsync();
            groups?.ForEach(g => map.Add(g.GroupId, Tuple.Create(g.Name, g.ProjectId)));

            return map;
        }

        private async Task<Dictionary<int, string>> GetStatesMap(int workflowId)
        {
            var map = new Dictionary<int, string>();

            var states = await _workflowRepository.GetWorkflowStatesMapAsync(workflowId);
            if (states != null)
            {
                states.ForEach(s => map.Add(s.Id, s.Name));
            }
                
            return map;
        }

        private async Task<Dictionary<int, string>> GetArtifactTypesMap()
        {
            var map = new Dictionary<int, string>();

            var artifacts = await _projectMetaRepository.GetStandardProjectTypesAsync();

            if (artifacts != null)
            {
                artifacts.ArtifactTypes.ForEach(a => map.Add(a.Id, a.Name));
            }
            
            return map;
        }

        private async Task<Dictionary<int, string>> GetValidValueMap()
        {
            var map = new Dictionary<int, string>();

            var types = await _projectMetaRepository.GetStandardProjectTypesAsync();

            if (types != null)
            {
                foreach (var p in types.PropertyTypes)
                {
                    if (p.ValidValues != null)
                    {
                        foreach(var v in p.ValidValues)
                        {
                            map.Add((int)v.Id, v.Value);
                        }
                    }
                };
            }

            return map;
        }

        private async Task<Dictionary<int, string>> GetPropertyTypesMap()
        {
            var map = new Dictionary<int, string>();

            var types = await _projectMetaRepository.GetStandardProjectTypesAsync();

            if (types != null)
            {
                foreach (var p in types.PropertyTypes)
                {
                    map.Add(p.Id, p.Name);
                };
            }

            return map;
        }
        
        private async Task<string> UploadErrorsToFileStore(string errors)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(errors ?? string.Empty)))
            {
                return await FileRepository.UploadFileAsync(WorkflowImportErrorsFile, null, stream, DateTime.UtcNow + TimeSpan.FromDays(1));
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

        private async Task ReplaceProjectPathsWithIds(IeWorkflow workflow)
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

                var pcAction = (IePropertyChangeAction) t.Action;
                pcAction.UsersGroups?.Where(ug => ug.IsGroup.GetValueOrDefault()
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

            var projectMap = (await _workflowRepository.GetProjectIdsByProjectPaths(projectPaths))
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
        private static IEnumerable<WorkflowDataValidationError> ValidateAndRemoveNotFoundByIdInCurrentWorkflow(IeWorkflow workflow, WorkflowDiffResult workflowDiffResult)
        {
            var errors = new List<WorkflowDataValidationError>();
            if (workflowDiffResult.NotFoundStates.Any())
            {
                workflow.States?.ForEach(s =>
                {
                    if (workflowDiffResult.NotFoundStates.Contains(s))
                    {
                        errors.Add(new WorkflowDataValidationError
                        {
                            Element = s,
                            ErrorCode = WorkflowDataValidationErrorCodes.StateNotFoundByIdInCurrent
                        });
                        workflow.States.Remove(s);
                    }
                });
            }

            if (workflowDiffResult.NotFoundEvents.Any())
            {
                workflow.TransitionEvents?.ForEach(te =>
                {
                    if (workflowDiffResult.NotFoundEvents.Contains(te))
                    {
                        errors.Add(new WorkflowDataValidationError
                        {
                            Element = te,
                            ErrorCode = WorkflowDataValidationErrorCodes.TransitionEventNotFoundByIdInCurrent
                        });
                        workflow.TransitionEvents.Remove(te);
                    }
                });

                workflow.PropertyChangeEvents?.ForEach(pce =>
                {
                    if (workflowDiffResult.NotFoundEvents.Contains(pce))
                    {
                        errors.Add(new WorkflowDataValidationError
                        {
                            Element = pce,
                            ErrorCode = WorkflowDataValidationErrorCodes.ProjectArtifactTypeNotFoundByIdInCurrent
                        });
                        workflow.PropertyChangeEvents.Remove(pce);
                    }
                });

                workflow.NewArtifactEvents?.ForEach(nae =>
                {
                    if (workflowDiffResult.NotFoundEvents.Contains(nae))
                    {
                        errors.Add(new WorkflowDataValidationError
                        {
                            Element = nae,
                            ErrorCode = WorkflowDataValidationErrorCodes.NewArtifactEventNotFoundByIdInCurrent
                        });
                        workflow.NewArtifactEvents.Remove(nae);
                    }
                });
            }

            if (workflowDiffResult.NotFoundProjectArtifactTypes.Any())
            {
                workflow.Projects?.ForEach(p => p.ArtifactTypes?.ForEach(at =>
                {
                    if (workflowDiffResult.NotFoundProjectArtifactTypes.Contains(at))
                    {
                        errors.Add(new WorkflowDataValidationError
                        {
                            Element = Tuple.Create(p, at),
                            ErrorCode = WorkflowDataValidationErrorCodes.ProjectArtifactTypeNotFoundByIdInCurrent
                        });
                        p.ArtifactTypes.Remove(at);
                    }
                }));
            }
            workflow.Projects?.RemoveAll(p => p.ArtifactTypes.IsEmpty());

            return errors;
        }
    }
}