using System;
using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using AdminStore.Models.Workflow;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Services.Workflow
{
    public class WorkflowXmlValidator : IWorkflowXmlValidator
    {
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

            if (workflow.Projects.Any() && !workflow.ArtifactTypes.Any())
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    Element = workflow,
                    ErrorCode = WorkflowXmlValidationErrorCodes.ProjectsProvidedWithoutArifactTypes
                });
            }
            else if (!workflow.Projects.Any() && workflow.ArtifactTypes.Any())
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    Element = workflow,
                    ErrorCode = WorkflowXmlValidationErrorCodes.ArtifactTypesProvidedWithoutProjects
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

                if (pcEvent.Triggers == null || !pcEvent.Triggers.Any())
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

                if (naEvent.Triggers == null || !naEvent.Triggers.Any())
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
                if (!transitionNames.Any())
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

                if (project.Id.HasValue && project.Id.Value < 1)
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = project,
                        ErrorCode = WorkflowXmlValidationErrorCodes.ProjectInvalidId
                    });
                }
            }

            var hasArtifactTypeNoSpecifiedError = false;
            foreach (var artifactType in workflow.ArtifactTypes.FindAll(at => at != null))
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
            }

            return result;
        }

        #endregion

        #region Private Methods

        private static bool ValidatePropertyNotEmpty(string property)
        {
            return !string.IsNullOrWhiteSpace(property);
        }

        private static bool ValidatePropertyLimit(string property, int limit)
        {
            return !(property?.Length > limit);
        }

        private static bool ValidateWorkflowContainsStates(IEnumerable<IeState> states)
        {
            return (states?.Any()).GetValueOrDefault();
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
        private bool _hasArtifactTypeGenerateChildrenActionNotSpecitiedError;
        private bool _hasChildCountGenerateChildrenActionNotSpecitiedError;
        private bool _hasStateConditionNotOnTriggerOfPropertyChangeEventError;
        private bool _stateStateConditionNotSpecifiedError;
        private bool _propertyNamePropertyChangeActionNotSpecitied;

        private void ResetErrorFlags()
        {
            _hasRecipientsEmailNotificationActionNotSpecitiedError = false;
            _hasAmbiguousRecipientsSourcesEmailNotificationActionError = false;
            _hasMessageEmailNotificationActionNotSpecitiedError = false;
            _hasPropertyNamePropertyChangeActionNotSpecitiedError = false;
            _hasPropertyValuePropertyChangeActionNotSpecitiedError = false;
            _hasArtifactTypeGenerateChildrenActionNotSpecitiedError = false;
            _hasChildCountGenerateChildrenActionNotSpecitiedError = false;
            _hasStateConditionNotOnTriggerOfPropertyChangeEventError = false;
            _stateStateConditionNotSpecifiedError = false;
            _propertyNamePropertyChangeActionNotSpecitied = false;
        }

        private void ValidatePermittedActions(IeEvent wEvent, WorkflowXmlValidationResult result)
        {
            // Currently the only action constrain is that
            // Property Change Event can have only Email Notification Action.
            if (!_propertyNamePropertyChangeActionNotSpecitied
                && wEvent.EventType == EventTypes.PropertyChange
                && wEvent.Triggers != null
                && wEvent.Triggers.Any(t => t?.Action != null && t.Action.ActionType != ActionTypes.EmailNotification))
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    Element = wEvent,
                    ErrorCode = WorkflowXmlValidationErrorCodes.PropertyChangeEventActionNotSupported
                });
                _propertyNamePropertyChangeActionNotSpecitied = true;
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
                && ((action.Emails == null || !action.Emails.Any())
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
                && action.Emails != null && action.Emails.Any()
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

            if (!_hasPropertyValuePropertyChangeActionNotSpecitiedError
                && !ValidatePropertyNotEmpty(action.PropertyValue))
            {
                result.Errors.Add(new WorkflowXmlValidationError
                {
                    Element = action,
                    ErrorCode = WorkflowXmlValidationErrorCodes.PropertyValuePropertyChangeActionNotSpecitied
                });
                _hasPropertyValuePropertyChangeActionNotSpecitiedError = true;
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
            if (wEvent.Triggers == null || !wEvent.Triggers.Any())
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
                if (!_stateStateConditionNotSpecifiedError
                && wEvent.Triggers.Any(t => t?.Condition?.ConditionType == ConditionTypes.State
                                        && !ValidatePropertyNotEmpty(((IeStateCondition)t.Condition).State)))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = wEvent,
                        ErrorCode = WorkflowXmlValidationErrorCodes.StateStateConditionNotSpecified
                    });
                    _stateStateConditionNotSpecifiedError = true;
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

        #endregion
    }
}