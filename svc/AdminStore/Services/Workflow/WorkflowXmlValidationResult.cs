using System.Collections.Generic;
using System.Linq;

namespace AdminStore.Services.Workflow
{
    public class WorkflowXmlValidationResult
    {
        public bool HasErrors => Errors.Any();

        private List<WorkflowXmlValidationError> _errors;
        public List<WorkflowXmlValidationError> Errors => _errors ?? (_errors = new List<WorkflowXmlValidationError>());
    }

    public class WorkflowXmlValidationError
    {
        public object Element { get; set; }
        public WorkflowXmlValidationErrorCodes ErrorCode { get; set; }

    }


    public enum WorkflowXmlValidationErrorCodes
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
        ActionsCountOnTriggerExceedsLimit10,
        PropertyChangeTriggerPropertyNotSpecified,
        ProjectNoSpecified,
        ProjectInvalidId,
        ArtifactTypeNoSpecified
    }
}