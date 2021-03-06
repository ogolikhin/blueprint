﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AdminStore.Helpers.Workflow;
using AdminStore.Models.Enums;
using AdminStore.Models.Workflow;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Services.Workflow.Validation.Xml
{
    public class WorkflowXmlValidator : IWorkflowXmlValidator
    {
        public const int MinWorkflowNameLength = 1;
        public const int MaxWorkflowNameLength = 24;

        #region Interface Implementation

        public WorkflowXmlValidationResult ValidateXml(IeWorkflow workflow)
        {
            ResetErrorFlags();

            if (workflow == null)
            {
                throw new ArgumentNullException(nameof(workflow));
            }

            var result = new WorkflowXmlValidationResult();

            if (!ValidateWorkflowName(workflow.Name))
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    Element = workflow,
                    ErrorCode = WorkflowXmlValidationErrorCodes.WorkflowNameMissingOrInvalid
                });
            }

            if (!ValidateMaximumLength(workflow.Description, 4000))
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    Element = workflow,
                    ErrorCode = WorkflowXmlValidationErrorCodes.WorkflowDescriptionExceedsLimit4000
                });
            }

            if (!ValidateWorkflowContainsStates(workflow.States))
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    Element = workflow,
                    ErrorCode = WorkflowXmlValidationErrorCodes.WorkflowDoesNotContainAnyStates
                });
            }
            else
            {
                var initialStatesCount = workflow.States.Count(s => s.IsInitial.GetValueOrDefault());
                if (initialStatesCount == 0)
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = workflow,
                        ErrorCode = WorkflowXmlValidationErrorCodes.NoInitialState
                    });
                }
                else if (initialStatesCount > 1)
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = workflow,
                        ErrorCode = WorkflowXmlValidationErrorCodes.MultipleInitialStates
                    });
                }
            }

            if (!ValidateStatesCount(workflow.States))
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    Element = workflow,
                    ErrorCode = WorkflowXmlValidationErrorCodes.StatesCountExceedsLimit100
                });
            }

            var stateNames = new HashSet<string>();
            var duplicateStateNames = new HashSet<string>();
            var initialStates = new HashSet<string>();
            var hasStateNameEmptyError = false;

            foreach (var state in workflow.States.FindAll(s => s != null))
            {
                if (!ValidateNotEmpty(state.Name))
                {
                    // There should be only one such an error.
                    if (!hasStateNameEmptyError)
                    {
                        result.Errors.Add(new WorkflowXmlValidationError
                        {
                            Element = state,
                            ErrorCode = WorkflowXmlValidationErrorCodes.StateNameEmpty
                        });
                        hasStateNameEmptyError = true;
                    }
                }
                else
                {
                    if (stateNames.Contains(state.Name))
                    {
                        // There should be only one such an error for a particular duplicate name.
                        if (!duplicateStateNames.Contains(state.Name))
                        {
                            result.Errors.Add(new WorkflowXmlValidationError
                            {
                                Element = state,
                                ErrorCode = WorkflowXmlValidationErrorCodes.StateNameNotUnique
                            });
                            duplicateStateNames.Add(state.Name);
                        }
                    }
                    else
                    {
                        stateNames.Add(state.Name);
                    }
                }

                if (!ValidateMaximumLength(state.Name, 24))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = state,
                        ErrorCode = WorkflowXmlValidationErrorCodes.StateNameExceedsLimit24
                    });
                }

                if (state.IsInitial.GetValueOrDefault())
                {
                    initialStates.Add(state.Name);

                    if (workflow.TransitionEvents.All(t => t.FromState != state.Name))
                    {
                        result.Errors.Add(new WorkflowXmlValidationError
                        {
                            Element = state,
                            ErrorCode = WorkflowXmlValidationErrorCodes.InitialStateDoesNotHaveOutgoingTransition
                        });
                    }
                }
            }

            var stateTransitions = stateNames.ToDictionary(s => s, s => new List<string>());
            var statesWithIncomingTransitions = new HashSet<string>();
            var stateOutgoingTransitionSet = new HashSet<Tuple<string, string>>();
            var statesWithDuplicateOutgoingTransitions = new HashSet<string>();
            var hasTransitionNameEmptyError = false;
            var hasActionTriggerNotSpecifiedError = false;

            foreach (var transition in workflow.TransitionEvents.FindAll(s => s != null))
            {
                statesWithIncomingTransitions.Add(transition.ToState);

                if (!ValidateNotEmpty(transition.Name))
                {
                    if (!hasTransitionNameEmptyError)
                    {
                        result.Errors.Add(new WorkflowXmlValidationError
                        {
                            ErrorCode = WorkflowXmlValidationErrorCodes.TransitionEventNameEmpty
                        });
                        hasTransitionNameEmptyError = true;
                    }
                }
                else if (ValidateNotEmpty(transition.FromState)
                    && !stateOutgoingTransitionSet.Add(Tuple.Create(transition.FromState, transition.Name)))
                {
                    if (!statesWithDuplicateOutgoingTransitions.Contains(transition.FromState))
                    {
                        result.Errors.Add(new WorkflowXmlValidationError
                        {
                            Element = transition.FromState,
                            ErrorCode = WorkflowXmlValidationErrorCodes.StateWithDuplicateOutgoingTransitions
                        });

                        statesWithDuplicateOutgoingTransitions.Add(transition.FromState);
                    }
                }

                if (!ValidateMaximumLength(transition.Name, 24))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = transition,
                        ErrorCode = WorkflowXmlValidationErrorCodes.TransitionEventNameExceedsLimit24
                    });
                }

                var from = ValidateNotEmpty(transition.FromState) ? transition.FromState : string.Empty;
                var to = ValidateNotEmpty(transition.ToState) ? transition.ToState : string.Empty;

                if (!string.IsNullOrEmpty(from) && stateTransitions.ContainsKey(from))
                {
                    stateTransitions[from].Add(transition.Name);
                }

                if (!string.IsNullOrEmpty(to) && stateTransitions.ContainsKey(to))
                {
                    stateTransitions[to].Add(transition.Name);
                }

                if (string.IsNullOrEmpty(from))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = transition,
                        ErrorCode = WorkflowXmlValidationErrorCodes.TransitionStartStateNotSpecified
                    });
                }

                if (string.IsNullOrEmpty(to))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = transition,
                        ErrorCode = WorkflowXmlValidationErrorCodes.TransitionEndStateNotSpecified
                    });
                }

                if (from != null && from.Equals(to))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = transition,
                        ErrorCode = WorkflowXmlValidationErrorCodes.TransitionFromAndToStatesSame
                    });
                }

                if ((!string.IsNullOrEmpty(from) && !stateNames.Contains(from))
                    || (!string.IsNullOrEmpty(to) && !stateNames.Contains(to)))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = transition,
                        ErrorCode = WorkflowXmlValidationErrorCodes.TransitionStateNotFound
                    });
                }

                if (transition.Triggers?.Count > 10)
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = transition,
                        ErrorCode = WorkflowXmlValidationErrorCodes.TriggerCountOnEventExceedsLimit10
                    });
                }

                if (transition.Triggers != null && transition.Triggers.Any(t => t.Action == null))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = transition,
                        ErrorCode = WorkflowXmlValidationErrorCodes.ActionTriggerNotSpecified
                    });
                    hasActionTriggerNotSpecifiedError = true;
                }

                ValidateTriggerConditions(transition, stateNames, result);

                ValidatePropertyChangeActionDuplicatePropertiesOnEvent(transition, result);
                transition.Triggers?.ForEach(t => ValidateAction(t?.Action, result));
            }

            var hasPcEventNoAnyTriggersError = false;
            var hasPcEventDuplicatePropertyError = false;
            var pcEventPropertyNames = new HashSet<string>();

            foreach (var pcEvent in workflow.PropertyChangeEvents.FindAll(s => s != null))
            {
                if (!ValidateMaximumLength(pcEvent.Name, 24))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = pcEvent,
                        ErrorCode = WorkflowXmlValidationErrorCodes.PropertyChangeEventNameExceedsLimit24
                    });
                }

                if (!ValidateNotEmpty(pcEvent.PropertyName))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = pcEvent,
                        ErrorCode = WorkflowXmlValidationErrorCodes.PropertyChangeEventPropertyNotSpecified
                    });
                }
                else if (!pcEventPropertyNames.Add(pcEvent.PropertyName) && !hasPcEventDuplicatePropertyError)
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        ErrorCode = WorkflowXmlValidationErrorCodes.PropertyChangeEventDuplicateProperties
                    });
                    hasPcEventDuplicatePropertyError = true;
                }

                if (pcEvent.Triggers.IsEmpty() && !hasPcEventNoAnyTriggersError)
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = pcEvent,
                        ErrorCode = WorkflowXmlValidationErrorCodes.PropertyChangeEventNoAnyTriggersNotSpecified
                    });
                    hasPcEventNoAnyTriggersError = true;
                }

                if (pcEvent.Triggers?.Count > 10)
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = pcEvent,
                        ErrorCode = WorkflowXmlValidationErrorCodes.TriggerCountOnEventExceedsLimit10
                    });
                }

                if (!hasActionTriggerNotSpecifiedError
                    && pcEvent.Triggers != null && pcEvent.Triggers.Any(t => t.Action == null))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = pcEvent,
                        ErrorCode = WorkflowXmlValidationErrorCodes.ActionTriggerNotSpecified
                    });
                    hasActionTriggerNotSpecifiedError = true;
                }

                ValidateTriggerConditions(pcEvent, stateNames, result);
                ValidatePermittedActions(pcEvent, result);

                pcEvent.Triggers?.ForEach(t => ValidateAction(t?.Action, result));
            }

            var hasNaEventNoAnyTriggersError = false;

            foreach (var naEvent in workflow.NewArtifactEvents.FindAll(s => s != null))
            {
                if (!ValidateMaximumLength(naEvent.Name, 24))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = naEvent,
                        ErrorCode = WorkflowXmlValidationErrorCodes.NewArtifactEventNameExceedsLimit24
                    });
                }

                if (naEvent.Triggers.IsEmpty())
                {
                    if (!hasNaEventNoAnyTriggersError)
                    {
                        result.Errors.Add(new WorkflowXmlValidationError
                        {
                            Element = naEvent,
                            ErrorCode = WorkflowXmlValidationErrorCodes.NewArtifactEventNoAnyTriggersNotSpecified
                        });
                        hasNaEventNoAnyTriggersError = true;
                    }
                }

                if (naEvent.Triggers?.Count > 10)
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = naEvent,
                        ErrorCode = WorkflowXmlValidationErrorCodes.TriggerCountOnEventExceedsLimit10
                    });
                }

                if (!hasActionTriggerNotSpecifiedError
                    && naEvent.Triggers != null && naEvent.Triggers.Any(t => t.Action == null))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = naEvent,
                        ErrorCode = WorkflowXmlValidationErrorCodes.ActionTriggerNotSpecified
                    });
                    hasActionTriggerNotSpecifiedError = true;
                }

                ValidateTriggerConditions(naEvent, stateNames, result);

                ValidatePropertyChangeActionDuplicatePropertiesOnEvent(naEvent, result);
                naEvent.Triggers?.ForEach(t => ValidateAction(t?.Action, result));
            }

            foreach (var stateName in stateTransitions.Keys)
            {
                var transitionNames = stateTransitions[stateName];
                if (transitionNames.IsEmpty())
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = stateName,
                        ErrorCode = WorkflowXmlValidationErrorCodes.StateDoesNotHaveAnyTransitions
                    });
                }

                if (transitionNames.Count > 10)
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = stateName,
                        ErrorCode = WorkflowXmlValidationErrorCodes.TransitionCountOnStateExceedsLimit10
                    });
                }
            }

            ValidateWorkflowConnectivity(workflow, result);

            var hasProjectNoSpecifieError = false;
            var hasAmbiguousProjectReference = false;
            var hasDuplicateProjectIdError = false;
            var hasDuplicateProjectPathError = false;
            var hasProjectDoesNotHaveAnyArtfactTypesError = false;
            var hasArtifactTypeNoSpecifiedError = false;
            var hasDuplicateArtifactTypesInProjectError = false;
            var projectIds = new HashSet<int>();
            var projectPaths = new HashSet<string>();

            foreach (var project in workflow.Projects.FindAll(p => p != null))
            {
                if (!hasProjectNoSpecifieError && !project.Id.HasValue && !ValidateNotEmpty(project.Path))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = project,
                        ErrorCode = WorkflowXmlValidationErrorCodes.ProjectNoSpecified
                    });
                    hasProjectNoSpecifieError = true;
                }

                if (!hasAmbiguousProjectReference && project.Id.HasValue && ValidateNotEmpty(project.Path))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = project,
                        ErrorCode = WorkflowXmlValidationErrorCodes.AmbiguousProjectReference
                    });
                    hasAmbiguousProjectReference = true;
                }

                if (!_hasInvalidIdError && project.Id.HasValue && project.Id < 1)
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        ErrorCode = WorkflowXmlValidationErrorCodes.InvalidId
                    });
                    _hasInvalidIdError = true;
                }

                if (project.Id.HasValue)
                {
                    if (!hasDuplicateProjectIdError && projectIds.Contains(project.Id.Value))
                    {
                        result.Errors.Add(new WorkflowXmlValidationError
                        {
                            Element = project,
                            ErrorCode = WorkflowXmlValidationErrorCodes.ProjectDuplicateId
                        });
                        hasDuplicateProjectIdError = true;
                    }

                    projectIds.Add(project.Id.Value);
                }
                else
                {
                    if (!hasDuplicateProjectPathError
                        && ValidateNotEmpty(project.Path)
                        && projectPaths.Contains(project.Path))
                    {
                        result.Errors.Add(new WorkflowXmlValidationError
                        {
                            Element = project,
                            ErrorCode = WorkflowXmlValidationErrorCodes.ProjectDuplicatePath
                        });
                        hasDuplicateProjectPathError = true;
                    }

                    if (ValidateNotEmpty(project.Path))
                    {
                        projectPaths.Add(project.Path);
                    }
                }

                if (project.ArtifactTypes.IsEmpty())
                {
                    if (!hasProjectDoesNotHaveAnyArtfactTypesError)
                    {
                        result.Errors.Add(new WorkflowXmlValidationError
                        {
                            Element = project,
                            ErrorCode = WorkflowXmlValidationErrorCodes.ProjectDoesNotHaveAnyArtfactTypes
                        });
                        hasProjectDoesNotHaveAnyArtfactTypesError = true;
                    }

                    continue;
                }

                var projectArtifactTypes = new HashSet<string>();

                foreach (var artifactType in project.ArtifactTypes.FindAll(at => at != null))
                {
                    if (!ValidateNotEmpty(artifactType.Name))
                    {
                        // There should be only one such an error.
                        if (!hasArtifactTypeNoSpecifiedError)
                        {
                            result.Errors.Add(new WorkflowXmlValidationError
                            {
                                Element = artifactType,
                                ErrorCode = WorkflowXmlValidationErrorCodes.ArtifactTypeNoSpecified
                            });
                            hasArtifactTypeNoSpecifiedError = true;
                        }
                    }
                    else
                    {
                        if (!hasDuplicateArtifactTypesInProjectError
                            && projectArtifactTypes.Contains(artifactType.Name))
                        {
                            result.Errors.Add(new WorkflowXmlValidationError
                            {
                                Element = project,
                                ErrorCode = WorkflowXmlValidationErrorCodes.DuplicateArtifactTypesInProject
                            });
                            hasDuplicateArtifactTypesInProjectError = true;
                        }

                        projectArtifactTypes.Add(artifactType.Name);
                    }
                }
            }

            return result;
        }

        // The update xml validated updates State Name references.
        public WorkflowXmlValidationResult ValidateUpdateXml(IeWorkflow workflow)
        {
            ResetUpdateErrorFlags();
            UpdateReferenceStateNames(workflow);
            ValidateUpdateId(workflow);
            var result = ValidateXml(workflow);
            ValidateDuplicateIds(workflow, result);

            if (_hasUpdateInvalidIdError && result.Errors.All(e => e.ErrorCode != WorkflowXmlValidationErrorCodes.InvalidId))
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    ErrorCode = WorkflowXmlValidationErrorCodes.InvalidId
                });
            }

            return result;
        }

        #endregion

        #region Private Methods

        private static bool ValidateWorkflowName(string workflowName)
        {
            return ValidateNotEmpty(workflowName)
                && ValidateMinimumLength(workflowName, MinWorkflowNameLength)
                && ValidateMaximumLength(workflowName, MaxWorkflowNameLength);
        }

        private static bool ValidateNotEmpty(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        private static bool ValidateMinimumLength(string value, int minimumLength)
        {
            return value?.Length >= minimumLength;
        }

        private static bool ValidateMaximumLength(string value, int maximumLength)
        {
            if (value == null)
            {
                return true;
            }

            return !(value.Length > maximumLength);
        }

        private static bool ValidateWorkflowContainsStates(IEnumerable<IeState> states)
        {
            return !states.IsEmpty();
        }

        private static bool ValidateStatesCount(IEnumerable<IeState> states)
        {
            return !((states?.Count()).GetValueOrDefault() > 100);
        }

        private bool _hasPropertyChangeActionDuplicatePropertiesOnEventError;
        private bool _hasRecipientsEmailNotificationActionNotSpecifiedError;
        private bool _hasAmbiguousRecipientsSourcesEmailNotificationActionError;
        private bool _hasMessageEmailNotificationActionNotSpecifiedError;
        private bool _hasPropertyNamePropertyChangeActionNotSpecifiedError;
        private bool _hasPropertyValuePropertyChangeActionNotSpecifiedError;
        private bool _hasAmbiguousPropertyValuePropertyChangeActionError;
        private bool _hasArtifactTypeGenerateChildrenActionNotSpecifiedError;
        private bool _hasChildCountGenerateChildrenActionNotSpecifiedError;
        private bool _hasChildCountGenerateChildrenActionNotValidError;
        private bool _hasArtifactTypeApplicableOnlyToGenerateChildArtifactActionError;
        private bool _hasChildCountApplicableOnlyToGenerateChildArtifactActionError;
        private bool _hasStateConditionNotOnTriggerOfPropertyChangeEventError;
        private bool _hasStateStateConditionNotSpecifiedError;
        private bool _hasPropertyNamePropertyChangeActionNotSupportedError;
        private bool _hasInvalidIdError;
        private bool _hasAmbiguousGroupProjectReference;
        private bool _hasPropertyChangeActionUserOrGroupNameNotSpecifiedError;
        private bool _hasWebhookActionUrlNotSpecified;
        private bool _hasWebhookActionUrlInvalid;
        private bool _hasWebhookActionHttpHeaderInvalid;
        private bool _hasWebhookActionBasicAuthInvalid;
        private bool _hasWebhookActionSignatureSecretTokenEmpty;
        private bool _hasWebhookActionSignatureAlgorithmInvalid;

        private void ResetErrorFlags()
        {
            _hasPropertyChangeActionDuplicatePropertiesOnEventError = false;
            _hasRecipientsEmailNotificationActionNotSpecifiedError = false;
            _hasAmbiguousRecipientsSourcesEmailNotificationActionError = false;
            _hasMessageEmailNotificationActionNotSpecifiedError = false;
            _hasPropertyNamePropertyChangeActionNotSpecifiedError = false;
            _hasAmbiguousPropertyValuePropertyChangeActionError = false;
            _hasPropertyValuePropertyChangeActionNotSpecifiedError = false;
            _hasArtifactTypeGenerateChildrenActionNotSpecifiedError = false;
            _hasChildCountGenerateChildrenActionNotSpecifiedError = false;
            _hasChildCountGenerateChildrenActionNotValidError = false;
            _hasArtifactTypeApplicableOnlyToGenerateChildArtifactActionError = false;
            _hasChildCountApplicableOnlyToGenerateChildArtifactActionError = false;
            _hasStateConditionNotOnTriggerOfPropertyChangeEventError = false;
            _hasStateStateConditionNotSpecifiedError = false;
            _hasPropertyNamePropertyChangeActionNotSupportedError = false;
            _hasInvalidIdError = false;
            _hasAmbiguousGroupProjectReference = false;
            _hasPropertyChangeActionUserOrGroupNameNotSpecifiedError = false;
            _hasWebhookActionUrlNotSpecified = false;
            _hasWebhookActionUrlInvalid = false;
            _hasWebhookActionHttpHeaderInvalid = false;
            _hasWebhookActionBasicAuthInvalid = false;
            _hasWebhookActionSignatureSecretTokenEmpty = false;
            _hasWebhookActionSignatureAlgorithmInvalid = false;
        }

        private void ValidatePropertyChangeActionDuplicatePropertiesOnEvent(IeEvent wEvent, WorkflowXmlValidationResult result)
        {
            var propertyNames = wEvent.Triggers?.Where(t => t?.Action?.ActionType == ActionTypes.PropertyChange)
                .Select(t => ((IePropertyChangeAction)t.Action).PropertyName).ToList() ?? new List<string>();
            if (!_hasPropertyChangeActionDuplicatePropertiesOnEventError
                && propertyNames.Count != propertyNames.Distinct().Count())
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    ErrorCode = WorkflowXmlValidationErrorCodes.PropertyChangeActionDuplicatePropertiesOnEvent
                });
                _hasPropertyChangeActionDuplicatePropertiesOnEventError = true;
            }
        }

        private void ValidatePermittedActions(IeEvent wEvent, WorkflowXmlValidationResult result)
        {
            // Currently the only action constrain is that
            // Property Change Event can have only Email Notification Action.
            if (!_hasPropertyNamePropertyChangeActionNotSupportedError
                && wEvent.EventType == EventTypes.PropertyChange
                && wEvent.Triggers != null
                && wEvent.Triggers.Any(t => t?.Action != null && t.Action.ActionType != ActionTypes.EmailNotification))
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    Element = wEvent,
                    ErrorCode = WorkflowXmlValidationErrorCodes.PropertyChangeEventActionNotSupported
                });
                _hasPropertyNamePropertyChangeActionNotSupportedError = true;
            }
        }

        private void ValidateAction(IeBaseAction action, WorkflowXmlValidationResult result)
        {
            if (action == null || result == null)
            {
                return;
            }

            switch (action.ActionType)
            {
                case ActionTypes.EmailNotification:
                    ValidateEmailNotificationAction((IeEmailNotificationAction)action, result);
                    break;
                case ActionTypes.PropertyChange:
                    ValidatePropertyChangeAction((IePropertyChangeAction)action, result);
                    break;
                case ActionTypes.Generate:
                    ValidateGenerateAction((IeGenerateAction)action, result);
                    break;
                case ActionTypes.Webhook:
                    ValidateWebhookAction((IeWebhookAction)action, result);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action.ActionType));
            }
        }

        private void ValidateEmailNotificationAction(IeEmailNotificationAction action,
            WorkflowXmlValidationResult result)
        {
            if (!_hasRecipientsEmailNotificationActionNotSpecifiedError
                && ((action.Emails.IsEmpty())
                && string.IsNullOrWhiteSpace(action.PropertyName)))
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    Element = action,
                    ErrorCode = WorkflowXmlValidationErrorCodes.RecipientsEmailNotificationActionNotSpecified
                });
                _hasRecipientsEmailNotificationActionNotSpecifiedError = true;
            }

            if (!_hasAmbiguousRecipientsSourcesEmailNotificationActionError
                && !action.Emails.IsEmpty()
                && !string.IsNullOrWhiteSpace(action.PropertyName))
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    Element = action,
                    ErrorCode = WorkflowXmlValidationErrorCodes.AmbiguousRecipientsSourcesEmailNotificationAction
                });
                _hasAmbiguousRecipientsSourcesEmailNotificationActionError = true;
            }

            action.Emails?.ForEach(email =>
            {
                if (!UserManagementHelper.IsValidEmail(email))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = email,
                        ErrorCode = WorkflowXmlValidationErrorCodes.EmailInvalidEmailNotificationAction
                    });
                }
            });

            if (!_hasMessageEmailNotificationActionNotSpecifiedError
                && !ValidateNotEmpty(action.Message))
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    Element = action,
                    ErrorCode = WorkflowXmlValidationErrorCodes.MessageEmailNotificationActionNotSpecified
                });
                _hasMessageEmailNotificationActionNotSpecifiedError = true;
            }
        }

        private void ValidatePropertyChangeAction(IePropertyChangeAction action, WorkflowXmlValidationResult result)
        {
            if (!_hasPropertyNamePropertyChangeActionNotSpecifiedError
                && !ValidateNotEmpty(action.PropertyName))
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    Element = action,
                    ErrorCode = WorkflowXmlValidationErrorCodes.PropertyNamePropertyChangeActionNotSpecified
                });
                _hasPropertyNamePropertyChangeActionNotSpecifiedError = true;
            }

            var pvCount = 0;
            if (action.PropertyValue != null)
            {
                pvCount++;
            }
            if (action.ValidValues?.Count > 0)
            {
                pvCount++;
            }

            if (action.UsersGroups != null)
            {
                pvCount++;

                action.UsersGroups?.UsersGroups?.ForEach(ug =>
                {
                    if (!_hasPropertyChangeActionUserOrGroupNameNotSpecifiedError
                        && !ValidateNotEmpty(ug.Name))
                    {
                        result.Errors.Add(new WorkflowXmlValidationError
                        {
                            Element = ug,
                            ErrorCode = WorkflowXmlValidationErrorCodes.PropertyChangeActionUserOrGroupNameNotSpecified
                        });
                        _hasPropertyChangeActionUserOrGroupNameNotSpecifiedError = true;
                    }

                    if (!_hasInvalidIdError && ug.GroupProjectId.HasValue && ug.GroupProjectId < 1)
                    {
                        result.Errors.Add(new WorkflowXmlValidationError
                        {
                            ErrorCode = WorkflowXmlValidationErrorCodes.InvalidId
                        });
                        _hasInvalidIdError = true;
                    }

                    if (!_hasAmbiguousGroupProjectReference && ug.GroupProjectId.HasValue
                        && ValidateNotEmpty(ug.GroupProjectPath))
                    {
                        result.Errors.Add(new WorkflowXmlValidationError
                        {
                            Element = action,
                            ErrorCode = WorkflowXmlValidationErrorCodes.AmbiguousGroupProjectReference
                        });
                        _hasAmbiguousGroupProjectReference = true;
                    }
                });
            }

            if (!_hasPropertyValuePropertyChangeActionNotSpecifiedError
                && pvCount == 0)
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    Element = action,
                    ErrorCode = WorkflowXmlValidationErrorCodes.PropertyValuePropertyChangeActionNotSpecified
                });
                _hasPropertyValuePropertyChangeActionNotSpecifiedError = true;
            }
            else if (!_hasAmbiguousPropertyValuePropertyChangeActionError
                && pvCount > 1)
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    Element = action,
                    ErrorCode = WorkflowXmlValidationErrorCodes.AmbiguousPropertyValuePropertyChangeAction
                });
                _hasAmbiguousPropertyValuePropertyChangeActionError = true;
            }
        }

        private void ValidateGenerateAction(IeGenerateAction action, WorkflowXmlValidationResult result)
        {
            if (action.GenerateActionType == GenerateActionTypes.Children)
            {
                if (!_hasArtifactTypeGenerateChildrenActionNotSpecifiedError
                    && !ValidateNotEmpty(action.ArtifactType))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = action,
                        ErrorCode = WorkflowXmlValidationErrorCodes.ArtifactTypeGenerateChildrenActionNotSpecified
                    });
                    _hasArtifactTypeGenerateChildrenActionNotSpecifiedError = true;
                }

                if (!_hasChildCountGenerateChildrenActionNotSpecifiedError
                    && !action.ChildCount.HasValue)
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = action,
                        ErrorCode = WorkflowXmlValidationErrorCodes.ChildCountGenerateChildrenActionNotSpecified
                    });
                    _hasChildCountGenerateChildrenActionNotSpecifiedError = true;
                }
                else if (!_hasChildCountGenerateChildrenActionNotValidError
                    && action.ChildCount.HasValue
                    && (action.ChildCount.Value < 1 || action.ChildCount.Value > 10))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = action,
                        ErrorCode = WorkflowXmlValidationErrorCodes.ChildCountGenerateChildrenActionNotValid
                    });
                    _hasChildCountGenerateChildrenActionNotValidError = true;
                }
            }
            else
            {
                if (!_hasArtifactTypeApplicableOnlyToGenerateChildArtifactActionError
                    && action.ArtifactType != null)
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = action,
                        ErrorCode = WorkflowXmlValidationErrorCodes.ArtifactTypeApplicableOnlyToGenerateChildArtifactAction
                    });
                    _hasArtifactTypeApplicableOnlyToGenerateChildArtifactActionError = true;
                }

                if (!_hasChildCountApplicableOnlyToGenerateChildArtifactActionError
                    && action.ChildCount != null)
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = action,
                        ErrorCode = WorkflowXmlValidationErrorCodes.ChildCountApplicableOnlyToGenerateChildArtifactAction
                    });
                    _hasChildCountApplicableOnlyToGenerateChildArtifactActionError = true;
                }
            }
        }

        private void ValidateWebhookAction(IeWebhookAction action, WorkflowXmlValidationResult result)
        {
            if (!_hasWebhookActionUrlNotSpecified && action.Url.IsEmpty())
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    Element = action,
                    ErrorCode = WorkflowXmlValidationErrorCodes.WebhookActionUrlNotSpecified
                });
                _hasWebhookActionUrlNotSpecified = true;
            }
            else
            {
                Uri uriResult;
                if (!_hasWebhookActionUrlInvalid &&
                    !(Uri.TryCreate(action.Url, UriKind.Absolute, out uriResult) &&
                    (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = action,
                        ErrorCode = WorkflowXmlValidationErrorCodes.WebhookActionUrlInvalid
                    });
                    _hasWebhookActionUrlInvalid = true;
                }
            }

            if (action.HttpHeaders != null && !action.HttpHeaders.IsEmpty())
            {
                foreach (var header in action.HttpHeaders)
                {
                    if (!_hasWebhookActionHttpHeaderInvalid &&
                        !Regex.IsMatch(header, "^[^:]+:[^:]+$"))
                    {
                        result.Errors.Add(new WorkflowXmlValidationError
                        {
                            Element = action,
                            ErrorCode = WorkflowXmlValidationErrorCodes.WebhookActionHttpHeaderInvalid
                        });
                        _hasWebhookActionHttpHeaderInvalid = true;
                    }
                }
            }

            if (action.BasicAuth != null)
            {
                if (!_hasWebhookActionBasicAuthInvalid &&
                    (string.IsNullOrEmpty(action.BasicAuth.Username) ||
                    string.IsNullOrEmpty(action.BasicAuth.Password)))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = action,
                        ErrorCode = WorkflowXmlValidationErrorCodes.WebhookActionBasicAuthInvalid
                    });
                    _hasWebhookActionBasicAuthInvalid = true;
                }
            }

            if (action.Signature != null)
            {
                if (!_hasWebhookActionSignatureSecretTokenEmpty && string.IsNullOrEmpty(action.Signature.SecretToken))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = action,
                        ErrorCode = WorkflowXmlValidationErrorCodes.WebhookActionSignatureSecretTokenEmpty
                    });
                    _hasWebhookActionSignatureSecretTokenEmpty = true;
                }

                if (!_hasWebhookActionSignatureAlgorithmInvalid &&
                    !string.IsNullOrEmpty(action.Signature.Algorithm) &&
                    !action.Signature.Algorithm.Equals("HMACSHA256") &&
                    !action.Signature.Algorithm.Equals("HMACSHA1"))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = action,
                        ErrorCode = WorkflowXmlValidationErrorCodes.WebhookActionSignatureAlgorithmInvalid
                    });
                    _hasWebhookActionSignatureAlgorithmInvalid = true;
                }
            }
        }

        private void ValidateTriggerConditions(IeEvent wEvent, ICollection<string> states, WorkflowXmlValidationResult result)
        {
            ValidateConditionTriggerConditions(wEvent, states, result);
        }

        private void ValidateConditionTriggerConditions(IeEvent wEvent, ICollection<string> states, WorkflowXmlValidationResult result)
        {
            if (wEvent.Triggers.IsEmpty())
            {
                return;
            }

            if (!_hasStateConditionNotOnTriggerOfPropertyChangeEventError
                && wEvent.EventType != EventTypes.PropertyChange
                && wEvent.Triggers.Any(t => t?.Condition?.ConditionType == ConditionTypes.State))
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    Element = wEvent,
                    ErrorCode = WorkflowXmlValidationErrorCodes.StateConditionNotOnTriggerOfPropertyChangeEvent
                });
                _hasStateConditionNotOnTriggerOfPropertyChangeEventError = true;
            }

            if (wEvent.EventType == EventTypes.PropertyChange)
            {
                if (!_hasStateStateConditionNotSpecifiedError
                && wEvent.Triggers.Any(t => t?.Condition?.ConditionType == ConditionTypes.State
                                        && !ValidateNotEmpty(((IeStateCondition)t.Condition).State)))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = wEvent,
                        ErrorCode = WorkflowXmlValidationErrorCodes.StateStateConditionNotSpecified
                    });
                    _hasStateStateConditionNotSpecifiedError = true;
                }


                foreach (var trigger in wEvent.Triggers.Where(t => t?.Condition?.ConditionType == ConditionTypes.State))
                {
                    var stateCondition = (IeStateCondition)trigger.Condition;
                    if (ValidateNotEmpty(stateCondition.State) && !states.Contains(stateCondition.State))
                    {
                        result.Errors.Add(new WorkflowXmlValidationError
                        {
                            Element = stateCondition.State,
                            ErrorCode = WorkflowXmlValidationErrorCodes.StateStateConditionNotFound
                        });
                    }
                }
            }
        }

        private bool _hasUpdateInvalidIdError;
        private void ResetUpdateErrorFlags()
        {
            _hasUpdateInvalidIdError = false;
        }

        private static void UpdateReferenceStateNames(IeWorkflow workflow)
        {
            var stateMap = workflow?.States?.Where(s => s.Id.HasValue).
                ToDictionary(s => s.Id.Value, s => s.Name);
            if (stateMap.IsEmpty())
            {
                return;
            }

            workflow?.TransitionEvents.ForEach(e =>
            {
                string name;
                if (e.FromStateId.HasValue && stateMap.TryGetValue(e.FromStateId.Value, out name))
                {
                    e.FromState = name;
                }
                if (e.ToStateId.HasValue && stateMap.TryGetValue(e.ToStateId.Value, out name))
                {
                    e.ToState = name;
                }
            });

            // State Conditions can be only on triggers of PropertyChangeEvents.
            workflow?.PropertyChangeEvents?.ForEach(e => e.Triggers?
                .Where(t => t?.Condition?.ConditionType == ConditionTypes.State).ForEach(t =>
                {
                    var stateCondition = (IeStateCondition)t.Condition;
                    string name;
                    if (stateCondition.StateId.HasValue && stateMap.TryGetValue(stateCondition.StateId.Value, out name))
                    {
                        stateCondition.State = name;
                    }
                }));
        }

        #region Validate Update Id

        private void ValidateUpdateId(IeWorkflow workflow)
        {
            workflow?.States?.ForEach(ValidateUpdateId);
            workflow?.TransitionEvents?.ForEach(ValidateUpdateId);
            workflow?.PropertyChangeEvents?.ForEach(ValidateUpdateId);
            workflow?.NewArtifactEvents?.ForEach(ValidateUpdateId);
            workflow?.Projects?.ForEach(UpdateProjectToConventionNames);
        }

        private void ValidateUpdateId(IeState state)
        {
            ValidateUpdateId(state?.Id);
        }

        private void ValidateUpdateId(IeTransitionEvent tEvent)
        {
            ValidateUpdateId(tEvent?.Id);
            ValidateUpdateId(tEvent?.FromStateId);
            ValidateUpdateId(tEvent?.ToStateId);

            tEvent?.PermissionGroups?.ForEach(ValidateUpdateId);
            tEvent?.Triggers?.ForEach(UpdateTriggerToConventionNames);
        }

        private void ValidateUpdateId(IeGroup group)
        {
            ValidateUpdateId(group?.Id);
        }

        private void ValidateUpdateId(IePropertyChangeEvent pcEvent)
        {
            ValidateUpdateId(pcEvent?.Id);

            if (pcEvent?.PropertyId != null
                    && !WorkflowHelper.IsNameOrDescriptionProperty(pcEvent.PropertyId.Value))
            {
                ValidateUpdateId(pcEvent.PropertyId);
            }

            pcEvent?.Triggers?.ForEach(UpdateTriggerToConventionNames);
        }

        private void ValidateUpdateId(IeNewArtifactEvent naEvent)
        {
            ValidateUpdateId(naEvent?.Id);

            naEvent?.Triggers?.ForEach(UpdateTriggerToConventionNames);
        }

        private void UpdateProjectToConventionNames(IeProject project)
        {
            project?.ArtifactTypes?.ForEach(ValidateUpdateId);
        }

        private void ValidateUpdateId(IeArtifactType artifactType)
        {
            ValidateUpdateId(artifactType?.Id);
        }

        private void UpdateTriggerToConventionNames(IeTrigger trigger)
        {
            UpdateConditionToConventionNames(trigger?.Condition);
            UpdateActionToConventionNames(trigger?.Action);
        }

        private void UpdateConditionToConventionNames(IeCondition condition)
        {
            switch (condition?.ConditionType)
            {
                case ConditionTypes.State:
                    var stateCondition = (IeStateCondition)condition;
                    ValidateUpdateId(stateCondition.StateId);
                    break;
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(condition.ConditionType));
            }
        }

        private void UpdateActionToConventionNames(IeBaseAction action)
        {
            switch (action?.ActionType)
            {
                case ActionTypes.EmailNotification:
                    ValidateUpdateId((IeEmailNotificationAction)action);
                    break;
                case ActionTypes.PropertyChange:
                    ValidateUpdateId((IePropertyChangeAction)action);
                    break;
                case ActionTypes.Generate:
                    ValidateUpdateId((IeGenerateAction)action);
                    break;
                case ActionTypes.Webhook:
                    break;
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action.ActionType));
            }
        }

        private void ValidateUpdateId(IeEmailNotificationAction enAction)
        {
            ValidateUpdateId(enAction?.PropertyId);
        }

        private void ValidateUpdateId(IePropertyChangeAction pcAction)
        {
            if (pcAction?.PropertyId != null
                   && !WorkflowHelper.IsNameOrDescriptionProperty(pcAction.PropertyId.Value))
            {
                ValidateUpdateId(pcAction.PropertyId.Value);
            }

            pcAction?.ValidValues?.ForEach(ValidateUpdateId);
            pcAction?.UsersGroups?.UsersGroups?.ForEach(ValidateUpdateId);
        }

        private void ValidateUpdateId(IeValidValue validValue)
        {
            ValidateUpdateId(validValue?.Id);
        }

        private void ValidateUpdateId(IeUserGroup userGroup)
        {
            ValidateUpdateId(userGroup?.Id);
        }

        private void ValidateUpdateId(IeGenerateAction gAction)
        {
            ValidateUpdateId(gAction?.ArtifactTypeId);
        }

        private void ValidateUpdateId(int? id)
        {
            if (id.HasValue && id < 1)
            {
                _hasUpdateInvalidIdError = true;
            }
        }

        #endregion

        #region Validate Duplicate Ids

        private static void ValidateDuplicateIds(IeWorkflow workflow, WorkflowXmlValidationResult result)
        {
            ValidateDuplicateStateIds(workflow, result);
            ValidateDuplicateWorkflowEventIds(workflow, result);
            ValidateDuplicateProjectIds(workflow, result);
        }

        private static void ValidateDuplicateStateIds(IeWorkflow workflow, WorkflowXmlValidationResult result)
        {
            var stateIds = workflow?.States?.Where(s => s.Id.HasValue).Select(s => s.Id.Value).ToList();
            if (stateIds?.Count != stateIds?.Distinct().Count())
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    Element = workflow,
                    ErrorCode = WorkflowXmlValidationErrorCodes.DuplicateStateIds
                });
            }
        }

        private static void ValidateDuplicateWorkflowEventIds(IeWorkflow workflow, WorkflowXmlValidationResult result)
        {
            var weIds = new List<int>();

            var tIds = workflow?.TransitionEvents?.Where(te => te.Id.HasValue).Select(te => te.Id.Value);
            if (tIds != null)
            {
                weIds.AddRange(tIds);
            }

            var pcIds = workflow?.PropertyChangeEvents?.Where(pce => pce.Id.HasValue).Select(pce => pce.Id.Value);
            if (pcIds != null)
            {
                weIds.AddRange(pcIds);
            }

            var naIds = workflow?.NewArtifactEvents?.Where(nae => nae.Id.HasValue).Select(nae => nae.Id.Value);
            if (naIds != null)
            {
                weIds.AddRange(naIds);
            }

            if (weIds.Count != weIds.Distinct().Count())
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    Element = workflow,
                    ErrorCode = WorkflowXmlValidationErrorCodes.DuplicateWorkflowEventIds
                });
            }
        }

        private static void ValidateDuplicateProjectIds(IeWorkflow workflow, WorkflowXmlValidationResult result)
        {
            var projectIds = workflow?.Projects?.Where(p => p.Id.HasValue).Select(p => p.Id.Value).ToList();
            if (projectIds?.Count != projectIds?.Distinct().Count())
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    Element = workflow,
                    ErrorCode = WorkflowXmlValidationErrorCodes.DuplicateProjectIds
                });
            }

            workflow?.Projects?.ForEach(p => ValidateDuplicateArtifactTypeIdsInProject(p, result));
        }

        private static void ValidateDuplicateArtifactTypeIdsInProject(IeProject project, WorkflowXmlValidationResult result)
        {
            var atIds = project?.ArtifactTypes?.Where(at => at.Id.HasValue).Select(at => at.Id.Value).ToList();
            if (atIds?.Count != atIds?.Distinct().Count())
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    Element = project,
                    ErrorCode = WorkflowXmlValidationErrorCodes.DuplicateArtifactTypeIdsInProject
                });
            }
        }

        #endregion

        private static void ValidateWorkflowConnectivity(IeWorkflow workflow, WorkflowXmlValidationResult result)
        {
            var blockingErrors = new[]
            {
                WorkflowXmlValidationErrorCodes.WorkflowDoesNotContainAnyStates,
                WorkflowXmlValidationErrorCodes.NoInitialState,
                WorkflowXmlValidationErrorCodes.MultipleInitialStates,
                WorkflowXmlValidationErrorCodes.InitialStateDoesNotHaveOutgoingTransition,
                WorkflowXmlValidationErrorCodes.StateNameEmpty,
                WorkflowXmlValidationErrorCodes.StateNameNotUnique,
                WorkflowXmlValidationErrorCodes.StateDoesNotHaveAnyTransitions,
                WorkflowXmlValidationErrorCodes.TransitionStartStateNotSpecified,
                WorkflowXmlValidationErrorCodes.TransitionEndStateNotSpecified,
                WorkflowXmlValidationErrorCodes.TransitionStateNotFound
            };

            if (result.Errors.Any(error => blockingErrors.Contains(error.ErrorCode)))
            {
                return;
            }

            var workflowGraph = new WorkflowGraph(workflow);
            if (!workflowGraph.IsConnected())
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    Element = workflow,
                    ErrorCode = WorkflowXmlValidationErrorCodes.StatesNotConnectedToInitialState
                });
            }
        }

        #endregion
    }
}
