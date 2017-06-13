using System.Collections.Generic;
using System.Linq;

namespace AdminStore.Repositories.Workflow
{
    public class WorkflowValidationResult
    {
        public bool HasErrors => Errors.Any();

        public List<WorkflowValidationError> Errors => new List<WorkflowValidationError>();
    }

    public class WorkflowValidationError
    {
        public object Element { get; set; }
        public WorkflowValidationErrorCode ErrorCode { get; set; }

    }


    public enum WorkflowValidationErrorCode
    {
        WorkflowNameEmpty,
        WorkflowNameExceedsLimit75,
        WorkflowDescriptionExceedsLimit250,
        WorkflowDoesNotContainAnyStates,
        StatesCountExceedsLimit100,
        StateNameEmpty,
        StateNameExceedsLimit26,
        StateDescriptionExceedsLimit250,
        StateNameNotUnique,
        NoInitialState,
        MultipleInitialStates,
        StateDoesNotHaveAnyTransitions,
        TransitionNameExceedsLimit26,
        TransitionNameNotUniqueOnState,
        TransitionCountOnStateExceedsLimit10,
        TransitionStateNotFound
    }
}