using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Helpers;
using ArtifactStore.Models;
using ArtifactStore.Models.Workflow;
using ArtifactStore.Models.Workflow.Actions;
using ArtifactStore.Repositories;
using ArtifactStore.Repositories.Reuse;
using ArtifactStore.Repositories.Workflow;
using ArtifactStore.Services.VersionControl;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.Reuse;
using ServiceLibrary.Models.VersionControl;
using ServiceLibrary.Models.Workflow;

namespace ArtifactStore.Executors
{
    public sealed class StateChangeExecutor
    {
        private readonly int _userId;
        private readonly WorkflowStateChangeParameterEx _input;
        private readonly ISqlHelper _sqlHelper;
        private readonly IArtifactVersionsRepository _artifactVersionsRepository;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IVersionControlService _versionControlService;
        private readonly IReuseRepository _reuseRepository;
        private readonly ISaveArtifactRepository _saveArtifactRepository;

        public StateChangeExecutor(
            WorkflowStateChangeParameterEx input,
            int userId,
            IArtifactVersionsRepository artifactVersionsRepository,
            IWorkflowRepository workflowRepository,
            ISqlHelper sqlHelper,
            IVersionControlService versionControlService,
            IReuseRepository reuseRepository,
            ISaveArtifactRepository saveArtifactRepository
            )
        {
            _input = input;
            _userId = userId;
            _artifactVersionsRepository = artifactVersionsRepository;
            _workflowRepository = workflowRepository;
            _sqlHelper = sqlHelper;
            _versionControlService = versionControlService;
            _reuseRepository = reuseRepository;
            _saveArtifactRepository = saveArtifactRepository;
        }

        public async Task<QuerySingleResult<WorkflowState>> Execute()
        {
            return await _sqlHelper.RunInTransactionAsync(ServiceConstants.RaptorMain, GetTransactionAction());
        }

        private Func<IDbTransaction, Task<QuerySingleResult<WorkflowState>>> GetTransactionAction()
        {
            Func<IDbTransaction, Task<QuerySingleResult<WorkflowState>>> action = async transaction =>
            {
                var publishRevision = await CreateRevision(transaction);
                _input.RevisionId = publishRevision;

                var artifactInfo =
                await _artifactVersionsRepository.GetVersionControlArtifactInfoAsync(_input.ArtifactId,
                    null,
                    _userId);

                await ValidateArtifact(artifactInfo);

                var currentState = await ValidateCurrentState();

                var desiredTransition = await GetDesiredTransition(currentState);

                var triggers = GetTransitionTriggers(desiredTransition);

                var constraints = new List<IConstraint>();

                var preOpTriggers = triggers.Item1;
                var postOpTriggers = triggers.Item2;

                await ProcessConstraints(constraints);

                var result = await ChangeStateForArtifactAsync(_input, transaction);


                var executionParameters = await BuildTriggerExecutionParameters(artifactInfo, preOpTriggers, transaction);
                var errors = await preOpTriggers.ProcessTriggers(executionParameters);
                //var errors = (await ProcessPreopEventTriggers(preOpTriggers, executionParameters, artifactInfo, transaction)).ToDictionary(entry => entry.Key, entry => entry.Value);

                await PublishArtifacts(publishRevision, transaction);

                //These should be converted to messages and sent as a part of state change event to handler service
                foreach (var entry in await postOpTriggers.ProcessTriggers(executionParameters))
                {
                    errors.Add(entry.Key, entry.Value);
                }

                //Collecting all errors so that we can distinguish between errors at a later stage.
                if (errors.Count > 0)
                {
                    throw new ConflictException("State cannot be modified as the trigger cannot be executed");
                }

                return result;
            };
            return action;
        }

        private async Task<ExecutionParameters> BuildTriggerExecutionParameters(VersionControlArtifactInfo artifactInfo,
            WorkflowEventTriggers triggers, IDbTransaction transaction = null)
        {
            if (triggers.IsEmpty())
            {
                return null;
            }
            //TODO: detect if artifact has readonly reuse
            var isArtifactReadOnlyReuse = await _reuseRepository.DoItemsContainReadonlyReuse(new [] {_input.ArtifactId}, transaction);

            ItemTypeReuseTemplate reuseTemplate = null;
            var artifactId2StandardTypeId =
                await
                    _reuseRepository.GetStandardTypeIdsForArtifactsIdsAsync(new HashSet<int> { _input.ArtifactId });
            var instanceItemTypeId = artifactId2StandardTypeId[_input.ArtifactId].InstanceTypeId;
            if (instanceItemTypeId == null)
            {
                throw new BadRequestException("Artifact is not a standard artifact type");
            }
            if (isArtifactReadOnlyReuse.ContainsKey(_input.ArtifactId) && isArtifactReadOnlyReuse[_input.ArtifactId])
            {
                reuseTemplate = await LoadReuseSettings(instanceItemTypeId.Value);
            }
            var customItemTypeToPropertiesMap = await LoadCustomPropertyInformation(new [] { instanceItemTypeId.Value }, triggers, artifactInfo.ProjectId);

            var propertyTypes = new List<DPropertyType>();
            if (customItemTypeToPropertiesMap.ContainsKey(artifactInfo.ItemTypeId))
            {
                propertyTypes = customItemTypeToPropertiesMap[artifactInfo.ItemTypeId];
            }

            return new ExecutionParameters(artifactInfo, reuseTemplate, propertyTypes, _saveArtifactRepository, transaction);
        }

        private async Task<int> CreateRevision(IDbTransaction transaction)
        {
            var publishRevision =
                await
                    _sqlHelper.CreateRevisionInTransactionAsync(
                        transaction,
                        _userId,
                        I18NHelper.FormatInvariant(
                            "State Change Publish: publishing changes and changing artifact {0} state to {1}",
                            _input.ArtifactId, _input.ToStateId));
            return publishRevision;
        }

        private async Task ValidateArtifact(VersionControlArtifactInfo artifactInfo)
        {
            //Confirm that the artifact is not deleted
            var isDeleted = await _artifactVersionsRepository.IsItemDeleted(_input.ArtifactId);
            if (isDeleted)
            {
                throw new ConflictException(
                    I18NHelper.FormatInvariant(
                        "Artifact has been deleted and is no longer available for workflow state change. Please refresh your view."));
            }

            if (artifactInfo.IsDeleted)
            {
                throw new ConflictException(
                    I18NHelper.FormatInvariant(
                        "Artifact has been deleted and is no longer available for workflow state change. Please refresh your view."));
            }

            if (artifactInfo.VersionCount != _input.CurrentVersionId)
            {
                throw new ConflictException(
                    I18NHelper.FormatInvariant(
                        "Artifact has been updated. The current version of the artifact {0} does not match the specified version {1}. Please refresh your view.",
                        artifactInfo.VersionCount, _input.CurrentVersionId));
            }

            //Lock is obtained by current user inside the stored procedure itself
            //Check that it is not locked by some other user
            if (artifactInfo.LockedByUser != null && artifactInfo.LockedByUser.Id != _userId)
            {
                throw new ConflictException(
                    I18NHelper.FormatInvariant(
                        "Artifact has been updated. Artifact is locked by another user. Please refresh your view."));
            }
        }

        private async Task<WorkflowState> ValidateCurrentState()
        {
            //Get current state and validate current state
            var currentState =
                await _workflowRepository.GetStateForArtifactAsync(_userId, _input.ArtifactId, int.MaxValue, true);
            if (currentState == null)
            {
                throw new ConflictException(
                    I18NHelper.FormatInvariant(
                        "Artifact has been updated. There is no workflow state associated with the artifact. Please refresh your view."));
            }
            if (currentState.Id != _input.FromStateId)
            {
                throw new ConflictException(
                    I18NHelper.FormatInvariant(
                        "Artifact has been updated. The current workflow state id {0} of the artifact does not match the specified state {1}. Please refresh your view.",
                        currentState.Id, _input.FromStateId));
            }
            return currentState;
        }

        private async Task<WorkflowTransition> GetDesiredTransition(WorkflowState currentState)
        {
            //Get available transitions and validate the required transition
            var desiredTransition =
                await
                    _workflowRepository.GetTransitionForAssociatedStatesAsync(_userId, _input.ArtifactId,
                        currentState.WorkflowId, _input.FromStateId, _input.ToStateId);

            if (desiredTransition == null)
            {
                throw new ConflictException(
                    I18NHelper.FormatInvariant(
                        "No transitions available. Workflow could have been updated. Please refresh your view."));
            }
            return desiredTransition;
        }

        private Tuple<WorkflowEventTriggers, WorkflowEventTriggers> GetTransitionTriggers(WorkflowTransition desiredTransition)
        {
            var preOpTriggers = new PreopWorkflowEventTriggers();
            var postOpTriggers = new PostopWorkflowEventTriggers();
            foreach (var workflowEventTrigger in desiredTransition.Triggers.Where(t => t?.Action != null))
            {
                if (workflowEventTrigger.Action is IWorkflowEventSynchronousAction)
                {
                    preOpTriggers.Add(workflowEventTrigger);
                }
                else if (workflowEventTrigger.Action is IWorkflowEventASynchronousAction)
                {
                    postOpTriggers.Add(workflowEventTrigger);
                }
            }
            return new Tuple<WorkflowEventTriggers, WorkflowEventTriggers>(preOpTriggers, postOpTriggers);
        }

        private async Task ProcessConstraints(List<IConstraint> constraints)
        {
            foreach (var constraint in constraints)
            {
                if (!(await constraint.IsFulfilled()))
                {
                    throw new ConflictException("State cannot be modified as the constrating is not fulfilled");
                }
            }
        }

        private async Task<QuerySingleResult<WorkflowState>> ChangeStateForArtifactAsync(WorkflowStateChangeParameterEx input, IDbTransaction transaction = null)
        {
            var newState = await _workflowRepository.ChangeStateForArtifactAsync(_userId, input.ArtifactId, input, transaction);

            if (newState == null)
            {
                return new QuerySingleResult<WorkflowState>
                {
                    ResultCode = QueryResultCode.Failure,
                    Message = I18NHelper.FormatInvariant("State could not be modified for Artifact: {0} from State: {1} to New State: {2}",
                    input.ArtifactId,
                    input.FromStateId,
                    input.ToStateId)
                };
            }
            return new QuerySingleResult<WorkflowState>
            {
                ResultCode = QueryResultCode.Success,
                Item = newState
            };
        }

        private async Task PublishArtifacts(int publishRevision, IDbTransaction transaction)
        {
            await _versionControlService.PublishArtifacts(new PublishParameters

            {
                All = false,
                ArtifactIds = new[] { _input.ArtifactId },
                UserId = _userId,
                RevisionId = publishRevision
            }, transaction);
        }
        
        private async Task<ItemTypeReuseTemplate> LoadReuseSettings(int itemTypeId, IDbTransaction transaction = null)
        {

            var reuseSettingsDictionary = await _reuseRepository.GetReuseItemTypeTemplatesAsyc(new[] { itemTypeId }, transaction);
            
            ItemTypeReuseTemplate reuseTemplateSettings;

            if (reuseSettingsDictionary.Count == 0 ||
                !reuseSettingsDictionary.TryGetValue(itemTypeId, out reuseTemplateSettings))
            {
                return null;
            }

            return reuseTemplateSettings;
        }

        private async Task<Dictionary<int, List<DPropertyType>>> LoadCustomPropertyInformation(IEnumerable<int> instanceItemTypeIds, WorkflowEventTriggers triggers, int projectId)
        {
            var propertyChangeActions = triggers.Select(t => t.Action).OfType<PropertyChangeAction>().ToList();
            if (propertyChangeActions.Count == 0)
            {
                return new Dictionary<int, List<DPropertyType>>();
            }

            var instancePropertyTypeIds =
                propertyChangeActions.Select(b => b.InstancePropertyTypeId);

            return await
                _workflowRepository.GetCustomItemTypeToPropertiesMap(_userId, _input.ArtifactId, projectId, instanceItemTypeIds,
                    instancePropertyTypeIds);
        }
    }
}