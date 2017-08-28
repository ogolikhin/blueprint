using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Helpers;
using BluePrintSys.Messaging.CrossCutting.Helpers;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.Reuse;
using ServiceLibrary.Models.VersionControl;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Models.Workflow.Actions;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;

namespace ArtifactStore.Executors
{
    public sealed class StateChangeExecutor
    {
        private const string LogSource = "StateChangeExecutor";
        private readonly int _userId;
        private readonly WorkflowStateChangeParameterEx _input;
        private readonly ISqlHelper _sqlHelper;
        private readonly IStateChangeExecutorRepositories _stateChangeExecutorRepositories;

        public StateChangeExecutor(int userId, WorkflowStateChangeParameterEx input, ISqlHelper sqlHelper,
            IStateChangeExecutorRepositories stateChangeExecutorRepositories)
        {
            _userId = userId;
            _input = input;
            _sqlHelper = sqlHelper;
            _stateChangeExecutorRepositories = stateChangeExecutorRepositories;
        }

        class StateChangeResult
        {
            public QuerySingleResult<WorkflowState> Result { get; set; }

            public IList<ActionMessage> ActionMessages { get; } = new List<ActionMessage>();
        }

        public async Task<QuerySingleResult<WorkflowState>> Execute()
        {
            var result = await _sqlHelper.RunInTransactionAsync(ServiceConstants.RaptorMain, GetTransactionAction());

            return result.Result;
        }

        private Func<IDbTransaction, Task<StateChangeResult>> GetTransactionAction()
        {
            Func<IDbTransaction, Task<StateChangeResult>> action = async transaction =>
            {
                var publishRevision = await CreateRevision(transaction);
                _input.RevisionId = publishRevision;

                var artifactInfo =
                await _stateChangeExecutorRepositories.ArtifactVersionsRepository.GetVersionControlArtifactInfoAsync(_input.ArtifactId,
                    null,
                    _userId);

                await ValidateArtifact(artifactInfo);

                var currentState = await ValidateCurrentState();

                var triggers = await _stateChangeExecutorRepositories.WorkflowRepository.GetWorkflowEventTriggersForTransition(
                    _userId, 
                    _input.ArtifactId, 
                    currentState.WorkflowId, 
                    _input.FromStateId, 
                    _input.ToStateId);
                
                var constraints = new List<IConstraint>();


                await ProcessConstraints(constraints);

                var stateChangeResult = await ChangeStateForArtifactAsync(_input, transaction);

                var executionParameters = await BuildTriggerExecutionParameters(artifactInfo, triggers.SynchronousTriggers, transaction);

                var errors = await triggers.AsynchronousTriggers.ProcessTriggers(executionParameters);

                var artifactResultSet = await PublishArtifacts(publishRevision, transaction);

                //Collecting all errors so that we can distinguish between errors at a later stage.
                if (errors.Count > 0)
                {
                    throw new ConflictException("State cannot be modified as the trigger cannot be executed");
                }

                var result = new StateChangeResult
                {
                    Result = stateChangeResult
                };

                //Generate asynchronous messages for sending
                GenerateMessages(triggers.AsynchronousTriggers, artifactInfo, artifactResultSet, publishRevision, result);

                var tenantInfo = await _stateChangeExecutorRepositories.ApplicationSettingsRepository.GetTenantInfo();
                if (string.IsNullOrWhiteSpace(tenantInfo?.TenantId))
                {
                    throw new TenantInfoNotFoundException("No tenant information found. Please contact your administrator.");
                }

                await ProcessMessages(tenantInfo.TenantId, result);

                return result;
            };
            return action;
        }

        private async Task ProcessMessages(string tenantId, StateChangeResult result)
        {
            if (result?.ActionMessages?.Count <= 0)
            {
                return;
            }
            foreach (var actionMessage in result.ActionMessages.Where(a => a != null))
            {
                try
                {
                    await WorkflowMessaging.Instance.SendMessageAsync(tenantId, actionMessage);
                    string message = $"Sent {actionMessage.ActionType} message: {actionMessage.ToJSON()} with tenant id: {tenantId} to the Message queue";
                    await
                        _stateChangeExecutorRepositories.ServiceLogRepository.LogInformation(LogSource, message);
                }
                catch (Exception ex)
                {
                    string message =
                        $"Error while sending {actionMessage.ActionType} message with content {actionMessage.ToJSON()} on successful transition of artifact: {_input.ArtifactId} from {_input.FromStateId} to {_input.ToStateId}. Exception: {ex.Message}. StackTrace: {ex.StackTrace ?? string.Empty}";
                    await
                        _stateChangeExecutorRepositories.ServiceLogRepository.LogError(LogSource, message);
                    throw;
                }
            }
        }

        private void GenerateMessages(WorkflowEventTriggers postOpTriggers, VersionControlArtifactInfo artifactInfo,
            ArtifactResultSet artifactResultSet, int publishRevision, StateChangeResult result)
        {
            var project = artifactResultSet?.Projects?.FirstOrDefault(d => d.Id == artifactInfo.ProjectId);
            var baseHostUri = ServerUriHelper.GetBaseHostUri()?.ToString();

            foreach (var workflowEventTrigger in postOpTriggers)
            {
                if (workflowEventTrigger?.Action == null)
                {
                    continue;
                }
                switch (workflowEventTrigger.ActionType)
                {
                    case MessageActionType.Notification:
                        var notificationAction = workflowEventTrigger.Action as EmailNotificationAction;
                        if (notificationAction == null)
                        {
                            continue;
                        }
                        var notificationMessage = GetNotificationMessage(artifactInfo, artifactResultSet, publishRevision,
                            notificationAction);
                        result.ActionMessages.Add(notificationMessage);
                        break;
                    case MessageActionType.GenerateChildren:
                        var generateChildrenAction = workflowEventTrigger.Action as GenerateChildrenAction;
                        if (generateChildrenAction == null)
                        {
                            continue;
                        }
                        var generateChildrenMessage = new GenerateDescendantsMessage
                        {
                            ChildCount = generateChildrenAction.ChildCount.GetValueOrDefault(10),
                            DesiredArtifactTypeId = generateChildrenAction.ArtifactTypeId,
                            ArtifactId = artifactInfo.Id,
                            RevisionId = publishRevision,
                            UserId = _userId,
                            ProjectId = artifactInfo.ProjectId,
                            UserName = _input.UserName,
                            BaseHostUri = baseHostUri,
                            ProjectName = project?.Name,
                            TypePredefined = (int)artifactInfo.PredefinedType
                        };
                        result.ActionMessages.Add(generateChildrenMessage);
                        break;
                    case MessageActionType.GenerateTests:
                        var generateTestsAction = workflowEventTrigger.Action as GenerateTestCasesAction;
                        if (generateTestsAction == null || artifactInfo.PredefinedType != ItemTypePredefined.Process)
                        {
                            continue;
                        }
                        var generateTestsMessage = new GenerateTestsMessage
                        {
                            ArtifactId = artifactInfo.Id,
                            RevisionId = publishRevision,
                            UserId = _userId,
                            ProjectId = artifactInfo.ProjectId,
                            UserName = _input.UserName,
                            BaseHostUri = baseHostUri,
                            ProjectName = project?.Name
                        };
                        result.ActionMessages.Add(generateTestsMessage);
                        break;
                    case MessageActionType.GenerateUserStories:
                        var generateUserStories = workflowEventTrigger.Action as GenerateUserStoriesAction;
                        if (generateUserStories == null || artifactInfo.PredefinedType != ItemTypePredefined.Process)
                        {
                            continue;
                        }
                        var generateUserStoriesMessage = new GenerateUserStoriesMessage
                        {
                            ArtifactId = artifactInfo.Id,
                            RevisionId = publishRevision,
                            UserId = _userId,
                            ProjectId = artifactInfo.ProjectId,
                            UserName = _input.UserName,
                            BaseHostUri = baseHostUri,
                            ProjectName = project?.Name
                        };
                        result.ActionMessages.Add(generateUserStoriesMessage);
                        break;
                }
            }

            //Add published artifact message
            var publishedMessage =
                GetPublishedMessage(_userId, publishRevision, artifactInfo, artifactResultSet) as ArtifactsPublishedMessage;

            if (publishedMessage?.Artifacts?.Count > 0)
            {
                result.ActionMessages.Add(publishedMessage);
            }
        }

        private NotificationMessage GetNotificationMessage(VersionControlArtifactInfo artifactInfo,
            ArtifactResultSet artifactResultSet, int publishRevision, EmailNotificationAction notificationAction)
        {
            var artifactPartUrl = ServerUriHelper.GetArtifactUrl(artifactInfo.Id, true);
            if (artifactPartUrl == null)
            {
                return null;
            }
            var notificationMessage = new NotificationMessage
            {
                ArtifactName = artifactInfo.Name,
                ProjectName = artifactResultSet.Projects?.FirstOrDefault(p => p.Id == artifactInfo.ProjectId)?.Name,
                Subject = notificationAction.Subject,
                From = notificationAction.FromDisplayName,
                To = notificationAction.Emails,
                MessageTemplate = notificationAction.Message,
                RevisionId = publishRevision,
                UserId = _userId,
                ArtifactTypeId = artifactInfo.ItemTypeId,
                ArtifactId = artifactInfo.Id,
                ArtifactUrl = artifactPartUrl,
                ArtifactTypePredefined = (int)artifactInfo.PredefinedType,
                ProjectId = artifactInfo.ProjectId
            };
            return notificationMessage;
        }

        private ActionMessage GetPublishedMessage(int userId, int revisionId, VersionControlArtifactInfo artifactInfo, ArtifactResultSet artifactResultSet)
        {
            var message = new ArtifactsPublishedMessage
            {
                UserId = userId,
                RevisionId = revisionId
            };
            var artifacts = new List<PublishedArtifactInformation>();
            var artifact = new PublishedArtifactInformation
            {
                Id = artifactInfo.Id,
                Name = artifactInfo.Name,
                Predefined = (int)artifactInfo.PredefinedType,
                IsFirstTimePublished = false, //State change always occurs on published artifacts
                ProjectId = artifactInfo.ProjectId,
                Url = ServerUriHelper.GetArtifactUrl(artifactInfo.Id, true),
                ModifiedProperties = new List<PublishedPropertyInformation>()
            };

            IList<Property> modifiedProperties;
            if (artifactResultSet?.ModifiedProperties?.Count > 0 && artifactResultSet.ModifiedProperties.TryGetValue(artifactInfo.Id, out modifiedProperties) && modifiedProperties?.Count > 0)
            {
                artifact.ModifiedProperties.AddRange(modifiedProperties.Select(p => new PublishedPropertyInformation
                {
                    TypeId = p.PropertyTypeId,
                    PredefinedType = (int)p.Predefined
                }));
                //Only add artifact to list if there is a list of modified properties
                artifacts.Add(artifact);
            }

            message.Artifacts = artifacts;
            return message;
        }

        private async Task<ExecutionParameters> BuildTriggerExecutionParameters(VersionControlArtifactInfo artifactInfo, WorkflowEventTriggers triggers, IDbTransaction transaction = null)
        {
            if (triggers.IsEmpty())
            {
                return null;
            }
            //TODO: detect if artifact has readonly reuse
            var isArtifactReadOnlyReuse = await _stateChangeExecutorRepositories.ReuseRepository.DoItemsContainReadonlyReuse(new[] { _input.ArtifactId }, transaction);

            ItemTypeReuseTemplate reuseTemplate = null;
            var artifactId2StandardTypeId = await _stateChangeExecutorRepositories.ReuseRepository.GetStandardTypeIdsForArtifactsIdsAsync(new HashSet<int> { _input.ArtifactId });
            var instanceItemTypeId = artifactId2StandardTypeId[_input.ArtifactId].InstanceTypeId;
            if (instanceItemTypeId == null)
            {
                throw new BadRequestException("Artifact is not a standard artifact type");
            }
            if (isArtifactReadOnlyReuse.ContainsKey(_input.ArtifactId) && isArtifactReadOnlyReuse[_input.ArtifactId])
            {
                reuseTemplate = await LoadReuseSettings(instanceItemTypeId.Value);
            }
            var customItemTypeToPropertiesMap = await LoadCustomPropertyInformation(new[] { instanceItemTypeId.Value }, triggers, artifactInfo.ProjectId);

            var propertyTypes = new List<DPropertyType>();
            if (customItemTypeToPropertiesMap.ContainsKey(artifactInfo.ItemTypeId))
            {
                propertyTypes = customItemTypeToPropertiesMap[artifactInfo.ItemTypeId];
            }

            return new ExecutionParameters(_userId, artifactInfo, reuseTemplate, propertyTypes, _stateChangeExecutorRepositories.SaveArtifactRepository, transaction);
        }

        private async Task<int> CreateRevision(IDbTransaction transaction)
        {
            var publishRevision = await _sqlHelper.CreateRevisionInTransactionAsync(transaction, _userId, I18NHelper.FormatInvariant("State Change Publish: publishing changes and changing artifact {0} state to {1}", _input.ArtifactId, _input.ToStateId));
            return publishRevision;
        }

        private async Task ValidateArtifact(VersionControlArtifactInfo artifactInfo)
        {
            //Confirm that the artifact is not deleted
            var isDeleted = await _stateChangeExecutorRepositories.ArtifactVersionsRepository.IsItemDeleted(_input.ArtifactId);
            if (isDeleted)
            {
                throw new ConflictException(I18NHelper.FormatInvariant("Artifact has been deleted and is no longer available for workflow state change. Please refresh your view."));
            }

            if (artifactInfo.IsDeleted)
            {
                throw new ConflictException(I18NHelper.FormatInvariant("Artifact has been deleted and is no longer available for workflow state change. Please refresh your view."));
            }

            if (artifactInfo.VersionCount != _input.CurrentVersionId)
            {
                throw new ConflictException(I18NHelper.FormatInvariant("Artifact has been updated. The current version of the artifact {0} does not match the specified version {1}. Please refresh your view.", artifactInfo.VersionCount, _input.CurrentVersionId));
            }

            //Lock is obtained by current user inside the stored procedure itself
            //Check that it is not locked by some other user
            if (artifactInfo.LockedByUser != null && artifactInfo.LockedByUser.Id != _userId)
            {
                throw new ConflictException(I18NHelper.FormatInvariant("Artifact has been updated. Artifact is locked by another user. Please refresh your view."));
            }
        }

        private async Task<WorkflowState> ValidateCurrentState()
        {
            //Get current state and validate current state
            var currentState = await _stateChangeExecutorRepositories.WorkflowRepository.GetStateForArtifactAsync(_userId, _input.ArtifactId, int.MaxValue, true);
            if (currentState == null)
            {
                throw new ConflictException(I18NHelper.FormatInvariant("Artifact has been updated. There is no workflow state associated with the artifact. Please refresh your view."));
            }
            if (currentState.Id != _input.FromStateId)
            {
                throw new ConflictException(I18NHelper.FormatInvariant("Artifact has been updated. The current workflow state id {0} of the artifact does not match the specified state {1}. Please refresh your view.", currentState.Id, _input.FromStateId));
            }
            return currentState;
        }

        private async Task<WorkflowTransition> GetDesiredTransition(WorkflowState currentState)
        {
            //Get available transitions and validate the required transition
            var desiredTransition = await _stateChangeExecutorRepositories.WorkflowRepository.GetTransitionForAssociatedStatesAsync(_userId, _input.ArtifactId, currentState.WorkflowId, _input.FromStateId, _input.ToStateId);

            if (desiredTransition == null)
            {
                throw new ConflictException(I18NHelper.FormatInvariant("No transitions available. Workflow could have been updated. Please refresh your view."));
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
            var newState = await _stateChangeExecutorRepositories.WorkflowRepository.ChangeStateForArtifactAsync(_userId, input.ArtifactId, input, transaction);

            if (newState == null)
            {
                return new QuerySingleResult<WorkflowState>
                {
                    ResultCode = QueryResultCode.Failure,
                    Message = I18NHelper.FormatInvariant("State could not be modified for Artifact: {0} from State: {1} to New State: {2}", input.ArtifactId, input.FromStateId, input.ToStateId)
                };
            }
            return new QuerySingleResult<WorkflowState>
            {
                ResultCode = QueryResultCode.Success,
                Item = newState
            };
        }

        private async Task<ArtifactResultSet> PublishArtifacts(int publishRevision, IDbTransaction transaction)
        {
            return await _stateChangeExecutorRepositories.VersionControlService.PublishArtifacts(new PublishParameters

            {
                All = false,
                ArtifactIds = new[] { _input.ArtifactId },
                UserId = _userId,
                RevisionId = publishRevision
            }, transaction);
        }

        private async Task<ItemTypeReuseTemplate> LoadReuseSettings(int itemTypeId, IDbTransaction transaction = null)
        {
            var reuseSettingsDictionary = await _stateChangeExecutorRepositories.ReuseRepository.GetReuseItemTypeTemplatesAsyc(new[] { itemTypeId }, transaction);

            ItemTypeReuseTemplate reuseTemplateSettings;

            if (reuseSettingsDictionary.Count == 0 || !reuseSettingsDictionary.TryGetValue(itemTypeId, out reuseTemplateSettings))
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

            var instancePropertyTypeIds = propertyChangeActions.Select(b => b.InstancePropertyTypeId);

            return await _stateChangeExecutorRepositories.WorkflowRepository.GetCustomItemTypeToPropertiesMap(_userId, _input.ArtifactId, projectId, instanceItemTypeIds, instancePropertyTypeIds);
        }
    }
}