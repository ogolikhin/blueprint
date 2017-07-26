using System;
using System.Collections.Generic;
using System.Linq;
using AdminStore.Models.Workflow;
using ServiceLibrary.Helpers;

namespace AdminStore.Services.Workflow
{
    public class WorkflowXmlValidator : IWorkflowXmlValidator
    {
        public WorkflowXmlValidationResult ValidateXml(IeWorkflow workflow)
        {
            if (workflow == null)
            {
                throw new ArgumentNullException(nameof(workflow));
            }

            var result = new WorkflowXmlValidationResult();

            if (!ValidatePropertyNotEmpty(workflow.Name))
            {
                result.Errors.Add(new WorkflowXmlValidationError { Element = workflow, ErrorCode = WorkflowXmlValidationErrorCodes.WorkflowNameEmpty});
            }

            if (!ValidatePropertyLimit(workflow.Name, 24))
            {
                result.Errors.Add(new WorkflowXmlValidationError { Element = workflow, ErrorCode = WorkflowXmlValidationErrorCodes.WorkflowNameExceedsLimit24 });
            }

            if (!ValidatePropertyLimit(workflow.Description, 4000))
            {
                result.Errors.Add(new WorkflowXmlValidationError { Element = workflow, ErrorCode = WorkflowXmlValidationErrorCodes.WorkflowDescriptionExceedsLimit4000 });
            }

            if (!ValidateWorkflowContainsStates(workflow.States))
            {
                result.Errors.Add(new WorkflowXmlValidationError { Element = workflow, ErrorCode = WorkflowXmlValidationErrorCodes.WorkflowDoesNotContainAnyStates });
            }
            else
            {
                var initialStatesCount = workflow.States.Count(s => s.IsInitial.GetValueOrDefault());
                if (initialStatesCount == 0)
                {
                    result.Errors.Add(new WorkflowXmlValidationError { Element = workflow, ErrorCode = WorkflowXmlValidationErrorCodes.NoInitialState });
                }
                else if (initialStatesCount > 1)
                {
                    result.Errors.Add(new WorkflowXmlValidationError { Element = workflow, ErrorCode = WorkflowXmlValidationErrorCodes.MultipleInitialStates });
                }
            }

            if (!ValidateStatesCount(workflow.States))
            {
                result.Errors.Add(new WorkflowXmlValidationError { Element = workflow, ErrorCode = WorkflowXmlValidationErrorCodes.StatesCountExceedsLimit100 });
            }

            if (workflow.Projects.Any() && !workflow.ArtifactTypes.Any())
            {
                result.Errors.Add(new WorkflowXmlValidationError { Element = workflow, ErrorCode = WorkflowXmlValidationErrorCodes.ProjectsProvidedWithoutArifactTypes });
            }
            else if (!workflow.Projects.Any() && workflow.ArtifactTypes.Any())
            {
                result.Errors.Add(new WorkflowXmlValidationError { Element = workflow, ErrorCode = WorkflowXmlValidationErrorCodes.ArtifactTypesProvidedWithoutProjects });
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
                    if(!hasStateNameEmptyError)
                    {
                        result.Errors.Add(new WorkflowXmlValidationError { Element = state, ErrorCode = WorkflowXmlValidationErrorCodes.StateNameEmpty });
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
                            result.Errors.Add(new WorkflowXmlValidationError { Element = state, ErrorCode = WorkflowXmlValidationErrorCodes.StateNameNotUnique });
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
                    result.Errors.Add(new WorkflowXmlValidationError { Element = state, ErrorCode = WorkflowXmlValidationErrorCodes.StateNameExceedsLimit24 });
                }

                if (state.IsInitial.GetValueOrDefault())
                {
                    initialStates.Add(state.Name);

                    if (workflow.TransitionEvents.All(t => t.FromState != state.Name))
                    {
                        result.Errors.Add(new WorkflowXmlValidationError { Element = state, ErrorCode = WorkflowXmlValidationErrorCodes.InitialStateDoesNotHaveOutgoingTransition });
                    }
                }
            }

            var stateTransitions = stateNames.ToDictionary(s => s, s => new List<string>());
            var statesWithIncomingTransitions = new HashSet<string>();
            var workflowEventNames = new HashSet<string>();
            var duplicateworkflowEventNames = new HashSet<string>();
            var hasTransitionNameEmptyError = false;
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
            }

            var hasPcEventNameEmptyError = false;
            foreach (var pcEvent in workflow.PropertyChangeEvents.FindAll(s => s != null))
            {
                if (!ValidatePropertyNotEmpty(pcEvent.Name))
                {
                    if(!hasPcEventNameEmptyError)
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
                        ErrorCode = WorkflowXmlValidationErrorCodes.PropertyChangEventPropertyNotSpecified
                    });
                }

                if (pcEvent.Triggers?.Count > 10)
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = pcEvent,
                        ErrorCode = WorkflowXmlValidationErrorCodes.TriggerCountOnEventExceedsLimit10
                    });
                }
            }

            var hasNaEventNameEmptyError = false;
            foreach (var naEvent in workflow.NewArtifactEvents.FindAll(s => s != null))
            {
                if (!ValidatePropertyNotEmpty(naEvent.Name))
                {
                    if(!hasNaEventNameEmptyError)
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

                if (naEvent.Triggers?.Count > 10)
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = naEvent,
                        ErrorCode = WorkflowXmlValidationErrorCodes.TriggerCountOnEventExceedsLimit10
                    });
                }
            }

            foreach (var stateName in stateTransitions.Keys)
            {
                var transitionNames = stateTransitions[stateName];
                if (!transitionNames.Any())
                {
                    result.Errors.Add(new WorkflowXmlValidationError { Element = stateName, ErrorCode = WorkflowXmlValidationErrorCodes.StateDoesNotHaveAnyTransitions });
                }

                if (transitionNames.Count > 10)
                {
                    result.Errors.Add(new WorkflowXmlValidationError { Element = stateName, ErrorCode = WorkflowXmlValidationErrorCodes.TransitionCountOnStateExceedsLimit10 });
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
                        result.Errors.Add(new WorkflowXmlValidationError { Element = project, ErrorCode = WorkflowXmlValidationErrorCodes.ProjectNoSpecified });
                        hasProjectNoSpecifieError = true;
                    }
                }

                if (project.Id.HasValue && project.Id.Value < 1)
                {
                    result.Errors.Add(new WorkflowXmlValidationError { Element = project, ErrorCode = WorkflowXmlValidationErrorCodes.ProjectInvalidId });
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
                        result.Errors.Add(new WorkflowXmlValidationError { Element = artifactType, ErrorCode = WorkflowXmlValidationErrorCodes.ArtifactTypeNoSpecified });
                        hasArtifactTypeNoSpecifiedError = true;
                    }
                }
            }

            return result;
        }

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
    }
}