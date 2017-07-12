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
        WorkflowNameExceedsLimit24,
        WorkflowDescriptionExceedsLimit4000,
        WorkflowDoesNotContainAnyStates,
        StatesCountExceedsLimit100,
        StateNameEmpty,
        StateNameExceedsLimit24,
        StateDescriptionExceedsLimit4000,
        StateNameNotUnique,
        NoInitialState,
        MultipleInitialStates,
        StateDoesNotHaveAnyTransitions,
        TriggerNameEmpty,
        TriggerNameExceedsLimit24,
        TriggerDescriptionExceedsLimit4000,
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