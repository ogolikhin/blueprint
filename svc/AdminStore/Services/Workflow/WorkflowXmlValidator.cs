﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AdminStore.Helpers.Workflow;
using AdminStore.Models.Workflow;
using ArtifactStore.Helpers;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Services.Workflow
{
    public class WorkflowXmlValidator : IWorkflowXmlValidator
    {
        private bool _isConventionNamesInUse;
        private const string ConventionNamePattern = "{0}[Id = {1}]";
        private const string ConventionNameRegex = @"\[Id = [0-9]+]$";

        #region Interface Implementation

        public WorkflowXmlValidationResult ValidateXml(IeWorkflow workflow)
        {
            ResetErrorFlags();

            if (workflow == null)
            {
                throw new ArgumentNullException(nameof(workflow));
            }

            var result = new WorkflowXmlValidationResult();

            if (!ValidatePropertyNotEmpty(workflow.Name))
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    Element = workflow,
                    ErrorCode = WorkflowXmlValidationErrorCodes.WorkflowNameEmpty
                });
            }

            if (!ValidatePropertyLimit(workflow.Name, 24))
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    Element = workflow,
                    ErrorCode = WorkflowXmlValidationErrorCodes.WorkflowNameExceedsLimit24
                });
            }

            if (!ValidatePropertyLimit(workflow.Description, 4000))
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

                if (!ValidatePropertyNotEmpty(state.Name))
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

                if (!ValidatePropertyLimit(state.Name, 24))
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
            var workflowEventNames = new HashSet<string>();
            var duplicateworkflowEventNames = new HashSet<string>();
            var hasTransitionNameEmptyError = false;
            var hasActionTriggerNotSpecifiedError = false;
            foreach (var transition in workflow.TransitionEvents.FindAll(s => s != null))
            {
                statesWithIncomingTransitions.Add(transition.ToState);

                if (!ValidatePropertyNotEmpty(transition.Name))
                {
                    // There should be only one such an error.
                    if (!hasTransitionNameEmptyError)
                    {
                        result.Errors.Add(new WorkflowXmlValidationError
                        {
                            Element = transition,
                            ErrorCode = WorkflowXmlValidationErrorCodes.TransitionEventNameEmpty
                        });
                        hasTransitionNameEmptyError = true;
                    }
                }
                else if (!workflowEventNames.Add(transition.Name))
                {
                    // There should be only one such an error for a particular duplicate name.
                    if (!duplicateworkflowEventNames.Contains(transition.Name))
                    {
                        result.Errors.Add(new WorkflowXmlValidationError
                        {
                            Element = transition,
                            ErrorCode = WorkflowXmlValidationErrorCodes.WorkflowEventNameNotUniqueInWorkflow
                        });
                        duplicateworkflowEventNames.Add(transition.Name);
                    }
                }

                if (!ValidatePropertyLimit(transition.Name, 24))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = transition,
                        ErrorCode = WorkflowXmlValidationErrorCodes.TransitionEventNameExceedsLimit24
                    });
                }

                var from = ValidatePropertyNotEmpty(transition.FromState) ? transition.FromState : string.Empty;
                var to = ValidatePropertyNotEmpty(transition.ToState) ? transition.ToState : string.Empty;

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

                if (from != null && from.EqualsOrdinalIgnoreCase(to))
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

                transition.Triggers?.ForEach(t => ValidateAction(t?.Action, result));
            }

            var hasPcEventNameEmptyError = false;
            var hasPcEventNoAnyTriggersError = false;
            foreach (var pcEvent in workflow.PropertyChangeEvents.FindAll(s => s != null))
            {
                if (!ValidatePropertyNotEmpty(pcEvent.Name))
                {
                    if (!hasPcEventNameEmptyError)
                    {
                        result.Errors.Add(new WorkflowXmlValidationError
                        {
                            Element = pcEvent,
                            ErrorCode = WorkflowXmlValidationErrorCodes.PropertyChangeEventNameEmpty
                        });
                        hasPcEventNameEmptyError = true;
                    }
                }
                else if (!workflowEventNames.Add(pcEvent.Name))
                {
                    // There should be only one such an error for a particular duplicate name.
                    if (!duplicateworkflowEventNames.Contains(pcEvent.Name))
                    {
                        result.Errors.Add(new WorkflowXmlValidationError
                        {
                            Element = pcEvent,
                            ErrorCode = WorkflowXmlValidationErrorCodes.WorkflowEventNameNotUniqueInWorkflow
                        });
                        duplicateworkflowEventNames.Add(pcEvent.Name);
                    }
                }

                if (!ValidatePropertyLimit(pcEvent.Name, 24))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = pcEvent,
                        ErrorCode = WorkflowXmlValidationErrorCodes.PropertyChangeEventNameExceedsLimit24
                    });
                }

                if (!ValidatePropertyNotEmpty(pcEvent.PropertyName))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = pcEvent,
                        ErrorCode = WorkflowXmlValidationErrorCodes.PropertyChangeEventPropertyNotSpecified
                    });
                }

                if (pcEvent.Triggers.IsEmpty())
                {
                    if (!hasPcEventNoAnyTriggersError)
                    {
                        result.Errors.Add(new WorkflowXmlValidationError
                        {
                            Element = pcEvent,
                            ErrorCode = WorkflowXmlValidationErrorCodes.PropertyChangeEventNoAnyTriggersNotSpecified
                        });
                        hasPcEventNoAnyTriggersError = true;
                    }
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

            var hasNaEventNameEmptyError = false;
            var hasNaEventNoAnyTriggersError = false;
            foreach (var naEvent in workflow.NewArtifactEvents.FindAll(s => s != null))
            {
                if (!ValidatePropertyNotEmpty(naEvent.Name))
                {
                    if (!hasNaEventNameEmptyError)
                    {
                        result.Errors.Add(new WorkflowXmlValidationError
                        {
                            Element = naEvent,
                            ErrorCode = WorkflowXmlValidationErrorCodes.NewArtifactEventNameEmpty
                        });
                        hasNaEventNameEmptyError = true;
                    }
                }
                else if (!workflowEventNames.Add(naEvent.Name))
                {
                    // There should be only one such an error for a particular duplicate name.
                    if (!duplicateworkflowEventNames.Contains(naEvent.Name))
                    {
                        result.Errors.Add(new WorkflowXmlValidationError
                        {
                            Element = naEvent,
                            ErrorCode = WorkflowXmlValidationErrorCodes.WorkflowEventNameNotUniqueInWorkflow
                        });
                        duplicateworkflowEventNames.Add(naEvent.Name);
                    }
                }

                if (!ValidatePropertyLimit(naEvent.Name, 24))
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

            var hasProjectNoSpecifieError = false;
            var hasDuplicateProjectIdError = false;
            var hasDuplicateProjectPathError = false;
            var hasProjectDoesNotHaveAnyArtfactTypesError = false;
            var hasArtifactTypeNoSpecifiedError = false;
            var hasDuplicateArtifactTypesInProjectError = false;
            var projectIds = new HashSet<int>();
            var projectPaths = new HashSet<string>();
            foreach (var project in workflow.Projects.FindAll(p => p != null))
            {
                if (!project.Id.HasValue && !ValidatePropertyNotEmpty(project.Path))
                {
                    // There should be only one such an error.
                    if (!hasProjectNoSpecifieError)
                    {
                        result.Errors.Add(new WorkflowXmlValidationError
                        {
                            Element = project,
                            ErrorCode = WorkflowXmlValidationErrorCodes.ProjectNoSpecified
                        });
                        hasProjectNoSpecifieError = true;
                    }
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
                    && ValidatePropertyNotEmpty(project.Path)
                    && projectPaths.Contains(project.Path))
                    {
                        result.Errors.Add(new WorkflowXmlValidationError
                        {
                            Element = project,
                            ErrorCode = WorkflowXmlValidationErrorCodes.ProjectDuplicatePath
                        });
                        hasDuplicateProjectPathError = true;
                    }

                    if(ValidatePropertyNotEmpty(project.Path))
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
                    if (!ValidatePropertyNotEmpty(artifactType.Name))
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
                        if(!hasDuplicateArtifactTypesInProjectError
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

        public WorkflowXmlValidationResult ValidateUpdateXml(IeWorkflow workflow)
        {
            _isConventionNamesInUse = true;
            var clone = WorkflowHelper.CloneViaXmlSerialization(workflow);
            UpdateToConventionNames(clone);
            var result = ValidateXml(clone);
            ValidateDuplicateIds(clone, result);
            _isConventionNamesInUse = false;
            return result;
        }

        #endregion

        #region Private Methods

        private static bool ValidatePropertyNotEmpty(string property)
        {
            return !string.IsNullOrWhiteSpace(property);
        }

        private bool ValidatePropertyLimit(string property, int limit)
        {
            if (property == null)
            {
                return true;
            }

            var conventionExtra = 0;
            if (_isConventionNamesInUse)
            {
                var match = Regex.Match(property, ConventionNameRegex);
                conventionExtra = match.Value.Length + (property.Length > match.Value.Length ? 1 : 0); // take into account the space
            }

            return !(property.Length > limit + conventionExtra);
        }

        private static bool ValidateWorkflowContainsStates(IEnumerable<IeState> states)
        {
            return !states.IsEmpty();
        }

        private static bool ValidateStatesCount(IEnumerable<IeState> states)
        {
            return !((states?.Count()).GetValueOrDefault() > 100);
        }

        private bool _hasRecipientsEmailNotificationActionNotSpecitiedError;
        private bool _hasAmbiguousRecipientsSourcesEmailNotificationActionError;
        private bool _hasMessageEmailNotificationActionNotSpecitiedError;
        private bool _hasPropertyNamePropertyChangeActionNotSpecitiedError;
        private bool _hasPropertyValuePropertyChangeActionNotSpecitiedError;
        private bool _hasAmbiguousPropertyValuePropertyChangeActionError;
        private bool _hasArtifactTypeGenerateChildrenActionNotSpecitiedError;
        private bool _hasChildCountGenerateChildrenActionNotSpecitiedError;
        private bool _hasStateConditionNotOnTriggerOfPropertyChangeEventError;
        private bool _hasStateStateConditionNotSpecifiedError;
        private bool _hasPropertyNamePropertyChangeActionNotSupportedError;
        private bool _hasInvalidIdError;

        private void ResetErrorFlags()
        {
            _hasRecipientsEmailNotificationActionNotSpecitiedError = false;
            _hasAmbiguousRecipientsSourcesEmailNotificationActionError = false;
            _hasMessageEmailNotificationActionNotSpecitiedError = false;
            _hasPropertyNamePropertyChangeActionNotSpecitiedError = false;
            _hasAmbiguousPropertyValuePropertyChangeActionError = false;
            _hasPropertyValuePropertyChangeActionNotSpecitiedError = false;
            _hasArtifactTypeGenerateChildrenActionNotSpecitiedError = false;
            _hasChildCountGenerateChildrenActionNotSpecitiedError = false;
            _hasStateConditionNotOnTriggerOfPropertyChangeEventError = false;
            _hasStateStateConditionNotSpecifiedError = false;
            _hasPropertyNamePropertyChangeActionNotSupportedError = false;
            _hasInvalidIdError = false;
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
                    ValidateEmailNotificationAction((IeEmailNotificationAction) action, result);
                    break;
                case ActionTypes.PropertyChange:
                    ValidatePropertyChangeAction((IePropertyChangeAction) action, result);
                    break;
                case ActionTypes.Generate:
                    ValidateGenerateAction((IeGenerateAction) action, result);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action.ActionType));
            }
        }

        private void ValidateEmailNotificationAction(IeEmailNotificationAction action,
            WorkflowXmlValidationResult result)
        {
            if (!_hasRecipientsEmailNotificationActionNotSpecitiedError
                && ((action.Emails.IsEmpty())
                && string.IsNullOrWhiteSpace(action.PropertyName)))
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    Element = action,
                    ErrorCode = WorkflowXmlValidationErrorCodes.RecipientsEmailNotificationActionNotSpecitied
                });
                _hasRecipientsEmailNotificationActionNotSpecitiedError = true;
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

            if (!_hasMessageEmailNotificationActionNotSpecitiedError
                && !ValidatePropertyNotEmpty(action.Message))
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    Element = action,
                    ErrorCode = WorkflowXmlValidationErrorCodes.MessageEmailNotificationActionNotSpecitied
                });
                _hasMessageEmailNotificationActionNotSpecitiedError = true;
            }
        }

        private void ValidatePropertyChangeAction(IePropertyChangeAction action, WorkflowXmlValidationResult result)
        {
            if (!_hasPropertyNamePropertyChangeActionNotSpecitiedError
                && !ValidatePropertyNotEmpty(action.PropertyName))
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    Element = action,
                    ErrorCode = WorkflowXmlValidationErrorCodes.PropertyNamePropertyChangeActionNotSpecitied
                });
                _hasPropertyNamePropertyChangeActionNotSpecitiedError = true;
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
            if (action.UsersGroups?.Count > 0)
            {
                pvCount++;

                action.UsersGroups.ForEach(ug =>
                {
                    if (!_hasInvalidIdError && ug.GroupProjectId.HasValue && ug.GroupProjectId < 1)
                    {
                        result.Errors.Add(new WorkflowXmlValidationError
                        {
                            ErrorCode = WorkflowXmlValidationErrorCodes.InvalidId
                        });
                        _hasInvalidIdError = true;
                    }
                });
            }

            if (!_hasPropertyValuePropertyChangeActionNotSpecitiedError
                && pvCount == 0)
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    Element = action,
                    ErrorCode = WorkflowXmlValidationErrorCodes.PropertyValuePropertyChangeActionNotSpecitied
                });
                _hasPropertyValuePropertyChangeActionNotSpecitiedError = true;
            }
            else if(!_hasAmbiguousPropertyValuePropertyChangeActionError
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
                if (!_hasArtifactTypeGenerateChildrenActionNotSpecitiedError
                    && !ValidatePropertyNotEmpty(action.ArtifactType))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = action,
                        ErrorCode = WorkflowXmlValidationErrorCodes.ArtifactTypeGenerateChildrenActionNotSpecitied
                    });
                    _hasArtifactTypeGenerateChildrenActionNotSpecitiedError = true;
                }

                if (!_hasChildCountGenerateChildrenActionNotSpecitiedError
                    && action.ChildCount.GetValueOrDefault() < 1)
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = action,
                        ErrorCode = WorkflowXmlValidationErrorCodes.ChildCountGenerateChildrenActionNotSpecitied
                    });
                    _hasChildCountGenerateChildrenActionNotSpecitiedError = true;
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
                                        && !ValidatePropertyNotEmpty(((IeStateCondition)t.Condition).State)))
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
                    var stateCondition = (IeStateCondition) trigger.Condition;
                    if (ValidatePropertyNotEmpty(stateCondition.State) && !states.Contains(stateCondition.State))
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

        #region Update To Convention Names

        private static void UpdateToConventionNames(IeWorkflow workflow)
        {
            workflow?.States?.ForEach(UpdateStateToConventionNames);
            workflow?.TransitionEvents?.ForEach(UpdateTransitionEventToConventionNames);
            workflow?.PropertyChangeEvents?.ForEach(UpdatePropertyChangeEventToConventionNames);
            workflow?.NewArtifactEvents?.ForEach(UpdateNewArtifactEventToConventionNames);
            workflow?.Projects?.ForEach(UpdateProjectToConventionNames);
        }

        private static void UpdateStateToConventionNames(IeState state)
        {
            if (state?.Id.HasValue ?? false)
            {
                state.Name = GetConventionName(state.Name, state.Id.Value);
            }
        }

        private static void UpdateTransitionEventToConventionNames(IeTransitionEvent tEvent)
        {
            if (tEvent?.Id.HasValue ?? false)
            {
                tEvent.Name = GetConventionName(tEvent.Name, tEvent.Id.Value);
            }

            if (tEvent?.FromStateId.HasValue ?? false)
            {
                tEvent.FromState = GetConventionName(tEvent.FromState, tEvent.FromStateId.Value);
            }

            if (tEvent?.ToStateId.HasValue ?? false)
            {
                tEvent.ToState = GetConventionName(tEvent.ToState, tEvent.ToStateId.Value);
            }

            tEvent?.PermissionGroups?.ForEach(UpdatePermissionGroupToConventionNames);
            tEvent?.Triggers?.ForEach(UpdateTriggerToConventionNames);
        }

        private static void UpdatePermissionGroupToConventionNames(IeGroup group)
        {
            if (group?.Id.HasValue ?? false)
            {
                group.Name = GetConventionName(group.Name, group.Id.Value);
            }
        }

        private static void UpdatePropertyChangeEventToConventionNames(IePropertyChangeEvent pcEvent)
        {
            if (pcEvent?.Id.HasValue ?? false)
            {
                pcEvent.Name = GetConventionName(pcEvent.Name, pcEvent.Id.Value);
            }

            if (pcEvent?.PropertyId.HasValue ?? false)
            {
                pcEvent.PropertyName = GetConventionName(pcEvent.PropertyName, pcEvent.PropertyId.Value);
            }

            pcEvent?.Triggers?.ForEach(UpdateTriggerToConventionNames);
        }

        private static void UpdateNewArtifactEventToConventionNames(IeNewArtifactEvent naEvent)
        {
            if (naEvent?.Id.HasValue ?? false)
            {
                naEvent.Name = GetConventionName(naEvent.Name, naEvent.Id.Value);
            }

            naEvent?.Triggers?.ForEach(UpdateTriggerToConventionNames);
        }

        private static void UpdateProjectToConventionNames(IeProject project)
        {
            project?.ArtifactTypes?.ForEach(UpdateArtifactTypeToConventionNames);
        }

        private static void UpdateArtifactTypeToConventionNames(IeArtifactType artifactType)
        {
            if (artifactType?.Id.HasValue ?? false)
            {
                artifactType.Name = GetConventionName(artifactType.Name, artifactType.Id.Value);
            }
        }

        private static void UpdateTriggerToConventionNames(IeTrigger trigger)
        {
            UpdateConditionToConventionNames(trigger?.Condition);
            UpdateActionToConventionNames(trigger?.Action);
        }

        private static void UpdateConditionToConventionNames(IeCondition condition)
        {
            switch (condition?.ConditionType)
            {
                case ConditionTypes.State:
                    var stateCondition = (IeStateCondition) condition;
                    if (stateCondition?.StateId.HasValue ?? false)
                    {
                        stateCondition.State = GetConventionName(stateCondition.State, stateCondition.StateId.Value);
                    }
                    break;
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(condition.ConditionType));
            }
        }

        private static void UpdateActionToConventionNames(IeBaseAction action)
        {
            switch (action?.ActionType)
            {
                case ActionTypes.EmailNotification:
                    UpdateEmailNotificationActionToConventionNames((IeEmailNotificationAction) action);
                    break;
                case ActionTypes.PropertyChange:
                    UpdatePropertyChangeActionToConventionNames((IePropertyChangeAction) action);
                    break;
                case ActionTypes.Generate:
                    UpdateGenerateActionToConventionNames((IeGenerateAction) action);
                    break;
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action.ActionType));
            }
        }

        private static void UpdateEmailNotificationActionToConventionNames(IeEmailNotificationAction enAction)
        {
            if (enAction?.PropertyId.HasValue ?? false)
            {
                enAction.PropertyName = GetConventionName(enAction.PropertyName, enAction.PropertyId.Value);
            }
        }

        private static void UpdatePropertyChangeActionToConventionNames(IePropertyChangeAction pcAction)
        {
            if (pcAction?.PropertyId.HasValue ?? false)
            {
                pcAction.PropertyName = GetConventionName(pcAction.PropertyName, pcAction.PropertyId.Value);
            }

            pcAction?.ValidValues?.ForEach(UpdateValidValueToConventionNames);
            pcAction?.UsersGroups?.ForEach(UpdateUserGroupToConventionNames);
        }

        private static void UpdateValidValueToConventionNames(IeValidValue validValue)
        {
            if (validValue?.Id.HasValue ?? false)
            {
                validValue.Value = GetConventionName(validValue.Value, validValue.Id.Value);
            }
        }

        private static void UpdateUserGroupToConventionNames(IeUserGroup userGroup)
        {
            if (userGroup?.Id.HasValue ?? false)
            {
                userGroup.Name = GetConventionName(userGroup.Name, userGroup.Id.Value);
            }
        }

        private static void UpdateGenerateActionToConventionNames(IeGenerateAction gAction)
        {
            if (gAction?.ArtifactTypeId.HasValue ?? false)
            {
                gAction.ArtifactType = GetConventionName(gAction.ArtifactType, gAction.ArtifactTypeId.Value);
            }
        }

        private static string GetConventionName(string name, int id)
        {
            var prefix = name == null ? string.Empty : I18NHelper.FormatInvariant("{0} ", name);
            return I18NHelper.FormatInvariant(ConventionNamePattern, prefix, id);
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

        #endregion
    }
}