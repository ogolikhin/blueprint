using System;
using System.Collections.Generic;
using System.Linq;
using AdminStore.Models.Workflow;

namespace AdminStore.Repositories.Workflow
{
    public class WorkflowValidator : IWorkflowValidator
    {
        public WorkflowValidationResult Validate(IeWorkflow workflow)
        {
            if (workflow == null)
            {
                throw new ArgumentNullException(nameof(workflow));
            }

            var result = new WorkflowValidationResult();

            if (!ValidateWorkflowNameNotEmpty(workflow.Name))
            {
                result.Errors.Add(new WorkflowValidationError { Element = workflow, ErrorCode = WorkflowValidationErrorCode.WorkflowNameEmpty});
            }

            if (!ValidateWorkflowNameLimit(workflow.Name))
            {
                result.Errors.Add(new WorkflowValidationError { Element = workflow, ErrorCode = WorkflowValidationErrorCode.WorkflowNameExceedsLimit75 });
            }

            if (!ValidateWorkflowDescriptionLimit(workflow.Description))
            {
                result.Errors.Add(new WorkflowValidationError { Element = workflow, ErrorCode = WorkflowValidationErrorCode.WorkflowDescriptionExceedsLimit250 });
            }

            if (!ValidateWorkflowContainsStates(workflow.States))
            {
                result.Errors.Add(new WorkflowValidationError { Element = workflow, ErrorCode = WorkflowValidationErrorCode.WorkflowDoesNotContainAnyStates });
            }

            if (!ValidateStatesCount(workflow.States))
            {
                result.Errors.Add(new WorkflowValidationError { Element = workflow, ErrorCode = WorkflowValidationErrorCode.StatesCountExceedsLimit100 });
            }


            var stateNames = new HashSet<string>();
            foreach (var state in workflow.States.FindAll(s => s != null))
            {

                if (!ValidateStateNameNotEmpty(state.Name))
                {
                    result.Errors.Add(new WorkflowValidationError { Element = state, ErrorCode = WorkflowValidationErrorCode.StateNameEmpty });
                }
                else
                {
                    if (stateNames.Contains(state.Name))
                    {
                        result.Errors.Add(new WorkflowValidationError { Element = state, ErrorCode = WorkflowValidationErrorCode.StateNameNotUnique });
                    }
                    stateNames.Add(state.Name);
                }

                if (!ValidateStateNameLimit(state.Name))
                {
                    result.Errors.Add(new WorkflowValidationError { Element = state, ErrorCode = WorkflowValidationErrorCode.StateNameExceedsLimit26 });
                }

                if (!ValidateStateDescriptionLimit(state.Description))
                {
                    result.Errors.Add(new WorkflowValidationError { Element = state, ErrorCode = WorkflowValidationErrorCode.StateDescriptionExceedsLimit250 });
                }
            }


            //TODO:
            return result;
        }

        private static bool ValidateWorkflowNameNotEmpty(string name)
        {
            return !string.IsNullOrWhiteSpace(name);
        }

        private static bool ValidateWorkflowNameLimit(string name)
        {
            return !(name?.Length > 75);
        }

        private static bool ValidateWorkflowDescriptionLimit(string description)
        {
            return !(description?.Length > 250);
        }

        private static bool ValidateWorkflowContainsStates(IEnumerable<IeState> states)
        {
            return (states?.Any()).GetValueOrDefault();
        }

        private static bool ValidateStatesCount(IEnumerable<IeState> states)
        {
            return !((states?.Count()).GetValueOrDefault() > 100);
        }

        private static bool ValidateStateNameNotEmpty(string name)
        {
            return !string.IsNullOrWhiteSpace(name);
        }

        private static bool ValidateStateNameLimit(string name)
        {
            return !(name?.Length > 26);
        }

        private static bool ValidateStateDescriptionLimit(string description)
        {
            return !(description?.Length > 250);
        }
    }
}