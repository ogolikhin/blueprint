using System.Collections.Generic;
using System.Linq;

namespace AdminStore.Repositories.Workflow
{
    public class WorkflowValidationResult
    {
        public bool HasErrors => Errors.Any();

        private List<WorkflowValidationError> _errors;
        public List<WorkflowValidationError> Errors => _errors ?? (_errors = new List<WorkflowValidationError>());
    }

    public class WorkflowValidationError
    {
        public object Element { get; set; }
        public WorkflowValidationErrorCodes ErrorCode { get; set; }

    }


    public enum WorkflowValidationErrorCodes
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
        TransitionNameEmpty,
        TransitionNameExceedsLimit26,
        TransitionNameNotUniqueOnState,
        TransitionCountOnStateExceedsLimit10,
        TransitionStateNotFound,
        TransitionStartStateNotSpecified,
        TransitionEndStateNotSpecified,
        TransitionFromAndToStatesSame,
        ProjectNoSpecified,
        ProjectInvalidId,
        ArtifactTypeNoSpecified
    }
}