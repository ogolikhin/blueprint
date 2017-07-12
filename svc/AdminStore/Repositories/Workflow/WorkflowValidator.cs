using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Models;
using AdminStore.Models.Workflow;
using ArtifactStore.Helpers;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace AdminStore.Repositories.Workflow
{
    public class WorkflowValidator : IWorkflowValidator
    {
        private HashSet<int> _validProjectIdIds = new HashSet<int>();
        public HashSet<int> ValidProjectIds => _validProjectIdIds;

        private HashSet<SqlGroup> _validGroups = new HashSet<SqlGroup>();
        public HashSet<SqlGroup> ValidGroups => _validGroups;

        public WorkflowValidationResult Validate(IeWorkflow workflow)
        {
            if (workflow == null)
            {
                throw new ArgumentNullException(nameof(workflow));
            }

            var result = new WorkflowValidationResult();

            if (!ValidatePropertyNotEmpty(workflow.Name))
            {
                result.Errors.Add(new WorkflowValidationError { Element = workflow, ErrorCode = WorkflowValidationErrorCodes.WorkflowNameEmpty});
            }

            if (!ValidatePropertyLimit(workflow.Name, 24))
            {
                result.Errors.Add(new WorkflowValidationError { Element = workflow, ErrorCode = WorkflowValidationErrorCodes.WorkflowNameExceedsLimit24 });
            }

            if (!ValidatePropertyLimit(workflow.Description, 4000))
            {
                result.Errors.Add(new WorkflowValidationError { Element = workflow, ErrorCode = WorkflowValidationErrorCodes.WorkflowDescriptionExceedsLimit4000 });
            }

            if (!ValidateWorkflowContainsStates(workflow.States))
            {
                result.Errors.Add(new WorkflowValidationError { Element = workflow, ErrorCode = WorkflowValidationErrorCodes.WorkflowDoesNotContainAnyStates });
            }
            else
            {
                var initialStatesCount = workflow.States.Count(s => s.IsInitial.GetValueOrDefault());
                if (initialStatesCount == 0)
                {
                    result.Errors.Add(new WorkflowValidationError { Element = workflow, ErrorCode = WorkflowValidationErrorCodes.NoInitialState });
                }
                else if (initialStatesCount > 1)
                {
                    result.Errors.Add(new WorkflowValidationError { Element = workflow, ErrorCode = WorkflowValidationErrorCodes.MultipleInitialStates });
                }
            }

            if (!ValidateStatesCount(workflow.States))
            {
                result.Errors.Add(new WorkflowValidationError { Element = workflow, ErrorCode = WorkflowValidationErrorCodes.StatesCountExceedsLimit100 });
            }


            var stateNames = new HashSet<string>();
            foreach (var state in workflow.States.FindAll(s => s != null))
            {

                if (!ValidatePropertyNotEmpty(state.Name))
                {
                    result.Errors.Add(new WorkflowValidationError { Element = state, ErrorCode = WorkflowValidationErrorCodes.StateNameEmpty });
                }
                else
                {
                    if (stateNames.Contains(state.Name))
                    {
                        result.Errors.Add(new WorkflowValidationError { Element = state, ErrorCode = WorkflowValidationErrorCodes.StateNameNotUnique });
                    }
                    else
                    {
                        stateNames.Add(state.Name);
                    }
                }

                if (!ValidatePropertyLimit(state.Name, 24))
                {
                    result.Errors.Add(new WorkflowValidationError { Element = state, ErrorCode = WorkflowValidationErrorCodes.StateNameExceedsLimit24 });
                }

                if (!ValidatePropertyLimit(state.Description, 4000))
                {
                    result.Errors.Add(new WorkflowValidationError { Element = state, ErrorCode = WorkflowValidationErrorCodes.StateDescriptionExceedsLimit4000 });
                }
            }

            var stateTransitions = stateNames.ToDictionary(s => s, s => new List<string>());
            foreach (var trigger in workflow.Triggers.FindAll(s => s != null))
            {
                if (trigger.TriggerType == TriggerTypes.Transition)
                {
                    var transition = trigger as IeTransitionTrigger;

                    var from = ValidatePropertyNotEmpty(transition.FromState) ? transition.FromState : string.Empty;
                    var to = ValidatePropertyNotEmpty(transition.ToState) ? transition.ToState : string.Empty;

                    if (!ValidatePropertyNotEmpty(transition.Name))
                    {
                        result.Errors.Add(new WorkflowValidationError
                        {
                            Element = transition,
                            ErrorCode = WorkflowValidationErrorCodes.TriggerNameEmpty
                        });
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(from) && stateTransitions.ContainsKey(from))
                        {
                            stateTransitions[from].Add(transition.Name);
                        }

                        if (!string.IsNullOrEmpty(to) && stateTransitions.ContainsKey(to))
                        {
                            stateTransitions[to].Add(transition.Name);
                        }
                    }

                    if (!ValidatePropertyLimit(transition.Name, 24))
                    {
                        result.Errors.Add(new WorkflowValidationError
                        {
                            Element = transition,
                            ErrorCode = WorkflowValidationErrorCodes.TriggerNameExceedsLimit24
                        });
                    }

                    if (!ValidatePropertyLimit(transition.Description, 4000))
                    {
                        result.Errors.Add(new WorkflowValidationError
                        {
                            Element = transition,
                            ErrorCode = WorkflowValidationErrorCodes.TriggerDescriptionExceedsLimit4000
                        });
                    }

                    if (string.IsNullOrEmpty(from))
                    {
                        result.Errors.Add(new WorkflowValidationError
                        {
                            Element = transition,
                            ErrorCode = WorkflowValidationErrorCodes.TransitionStartStateNotSpecified
                        });
                    }
                    if (string.IsNullOrEmpty(to))
                    {
                        result.Errors.Add(new WorkflowValidationError
                        {
                            Element = transition,
                            ErrorCode = WorkflowValidationErrorCodes.TransitionEndStateNotSpecified
                        });
                    }

                    if (from != null && from.EqualsOrdinalIgnoreCase(to))
                    {
                        result.Errors.Add(new WorkflowValidationError
                        {
                            Element = transition,
                            ErrorCode = WorkflowValidationErrorCodes.TransitionFromAndToStatesSame
                        });
                    }

                    if ((!string.IsNullOrEmpty(from) && !stateNames.Contains(from))
                        || (!string.IsNullOrEmpty(to) && !stateNames.Contains(to)))
                    {
                        result.Errors.Add(new WorkflowValidationError
                        {
                            Element = transition,
                            ErrorCode = WorkflowValidationErrorCodes.TransitionStateNotFound
                        });
                    }
                }
            }

            foreach (var stateName in stateTransitions.Keys)
            {
                var transitionNames = stateTransitions[stateName];
                if (!transitionNames.Any())
                {
                    result.Errors.Add(new WorkflowValidationError { Element = stateName, ErrorCode = WorkflowValidationErrorCodes.StateDoesNotHaveAnyTransitions });
                }

                if (transitionNames.Count > 10)
                {
                    result.Errors.Add(new WorkflowValidationError { Element = stateName, ErrorCode = WorkflowValidationErrorCodes.TransitionCountOnStateExceedsLimit10 });
                }

                if (transitionNames.Count != transitionNames.Distinct().Count())
                {
                    result.Errors.Add(new WorkflowValidationError { Element = stateName, ErrorCode = WorkflowValidationErrorCodes.TransitionNameNotUniqueOnState });
                }
            }

            foreach (var project in workflow.Projects.FindAll(p => p != null))
            {
                if (!project.Id.HasValue && !ValidatePropertyNotEmpty(project.Path))
                {
                    result.Errors.Add(new WorkflowValidationError { Element = project, ErrorCode = WorkflowValidationErrorCodes.ProjectNoSpecified });
                }

                if (project.Id.HasValue && project.Id.Value < 1)
                {
                    result.Errors.Add(new WorkflowValidationError { Element = project, ErrorCode = WorkflowValidationErrorCodes.ProjectInvalidId });
                }
            }

            foreach (var artifactType in workflow.ArtifactTypes.FindAll(at => at != null))
            {
                if (!ValidatePropertyNotEmpty(artifactType.Name))
                {
                    result.Errors.Add(new WorkflowValidationError { Element = artifactType, ErrorCode = WorkflowValidationErrorCodes.ArtifactTypeNoSpecified });
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