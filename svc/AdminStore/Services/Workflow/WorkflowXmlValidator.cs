using System;
using System.Collections.Generic;
using System.Linq;
using AdminStore.Models.Workflow;
using ServiceLibrary.Helpers;

namespace AdminStore.Services.Workflow
{
    public class WorkflowXmlValidator : IWorkflowXmlValidator
    {
        public WorkflowXmlValidationResult Validate(IeWorkflow workflow)
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
            foreach (var state in workflow.States.FindAll(s => s != null))
            {

                if (!ValidatePropertyNotEmpty(state.Name))
                {
                    result.Errors.Add(new WorkflowXmlValidationError { Element = state, ErrorCode = WorkflowXmlValidationErrorCodes.StateNameEmpty });
                }
                else
                {
                    if (stateNames.Contains(state.Name))
                    {
                        result.Errors.Add(new WorkflowXmlValidationError { Element = state, ErrorCode = WorkflowXmlValidationErrorCodes.StateNameNotUnique });
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

                if (!ValidatePropertyLimit(state.Description, 4000))
                {
                    result.Errors.Add(new WorkflowXmlValidationError { Element = state, ErrorCode = WorkflowXmlValidationErrorCodes.StateDescriptionExceedsLimit4000 });
                }
            }

            var stateTransitions = stateNames.ToDictionary(s => s, s => new List<string>());
            foreach (var transitionEvent in workflow.TransitionEvents.FindAll(s => s != null))
            {
                if (!ValidatePropertyNotEmpty(transitionEvent.Name))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = transitionEvent,
                        ErrorCode = WorkflowXmlValidationErrorCodes.TransitionEventNameEmpty
                    });
                }

                if (!ValidatePropertyLimit(transitionEvent.Name, 24))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = transitionEvent,
                        ErrorCode = WorkflowXmlValidationErrorCodes.TransitionEventNameExceedsLimit24
                    });
                }

                if (!ValidatePropertyLimit(transitionEvent.Description, 4000))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = transitionEvent,
                        ErrorCode = WorkflowXmlValidationErrorCodes.TransitionEventDescriptionExceedsLimit4000
                    });
                }

               
                var transition = transitionEvent as IeTransitionEvent;

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
              

                if (transitionEvent.Triggers?.Count > 10)
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = transitionEvent,
                        ErrorCode = WorkflowXmlValidationErrorCodes.TriggerCountOnEventExceedsLimit10
                    });
                }
            }

            foreach (var propertyChangeEvent in workflow.PropertyChangeEvents.FindAll(s => s != null))
            {
                if (!ValidatePropertyNotEmpty(propertyChangeEvent.Name))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = propertyChangeEvent,
                        ErrorCode = WorkflowXmlValidationErrorCodes.PropertyChangeEventNameEmpty
                    });
                }

                if (!ValidatePropertyLimit(propertyChangeEvent.Name, 24))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = propertyChangeEvent,
                        ErrorCode = WorkflowXmlValidationErrorCodes.PropertyChangeEventNameExceedsLimit24
                    });
                }

                if (!ValidatePropertyLimit(propertyChangeEvent.Description, 4000))
                {
                    result.Errors.Add(new WorkflowXmlValidationError
                    {
                        Element = propertyChangeEvent,
                        ErrorCode = WorkflowXmlValidationErrorCodes.PropertyChangeEventDescriptionExceedsLimit4000
                    });
                }

              
                var pcEvent = propertyChangeEvent as IePropertyChangeEvent;

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
                        Element = propertyChangeEvent,
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

                if (transitionNames.Count != transitionNames.Distinct().Count())
                {
                    result.Errors.Add(new WorkflowXmlValidationError { Element = stateName, ErrorCode = WorkflowXmlValidationErrorCodes.TransitionNameNotUniqueOnState });
                }
            }

            foreach (var project in workflow.Projects.FindAll(p => p != null))
            {
                

                if (!project.Id.HasValue && !ValidatePropertyNotEmpty(project.Path))
                {
                    result.Errors.Add(new WorkflowXmlValidationError { Element = project, ErrorCode = WorkflowXmlValidationErrorCodes.ProjectNoSpecified });
                }

                if (project.Id.HasValue && project.Id.Value < 1)
                {
                    result.Errors.Add(new WorkflowXmlValidationError { Element = project, ErrorCode = WorkflowXmlValidationErrorCodes.ProjectInvalidId });
                }
            }

            foreach (var artifactType in workflow.ArtifactTypes.FindAll(at => at != null))
            {
                if (!ValidatePropertyNotEmpty(artifactType.Name))
                {
                    result.Errors.Add(new WorkflowXmlValidationError { Element = artifactType, ErrorCode = WorkflowXmlValidationErrorCodes.ArtifactTypeNoSpecified });
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