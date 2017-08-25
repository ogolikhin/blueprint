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
        WorkflowXmlSerializationError,
        WorkflowNameEmpty,
        WorkflowNameExceedsLimit24,
        WorkflowDescriptionExceedsLimit4000,
        WorkflowDoesNotContainAnyStates,
        StatesCountExceedsLimit100,
        StateNameEmpty,
        StateNameExceedsLimit24,
        StateNameNotUnique,
        NoInitialState,
        InitialStateDoesNotHaveOutgoingTransition,
        MultipleInitialStates,
        StateDoesNotHaveAnyTransitions,
        TransitionEventNameExceedsLimit24,
        PropertyChangeEventNameExceedsLimit24,
        NewArtifactEventNameExceedsLimit24,
        WorkflowEventNameNotUniqueInWorkflow,
        TransitionCountOnStateExceedsLimit10,
        TransitionStateNotFound,
        TransitionStartStateNotSpecified,
        TransitionEndStateNotSpecified,
        TransitionFromAndToStatesSame,
        TriggerCountOnEventExceedsLimit10,
        PropertyChangeEventPropertyNotSpecified,
        PropertyChangeEventNoAnyTriggersNotSpecified,
        NewArtifactEventNoAnyTriggersNotSpecified,
        ActionTriggerNotSpecified,
        RecipientsEmailNotificationActionNotSpecitied,
        AmbiguousRecipientsSourcesEmailNotificationAction,
        EmailInvalidEmailNotificationAction,
        MessageEmailNotificationActionNotSpecitied,
        PropertyNamePropertyChangeActionNotSpecitied,
        PropertyValuePropertyChangeActionNotSpecitied,
        AmbiguousPropertyValuePropertyChangeAction,
        PropertyChangeActionValidValueValueNotSpecitied,
        PropertyChangeActionUserOrGroupNameNotSpecitied,
        AmbiguousGroupProjectReference,
        ArtifactTypeGenerateChildrenActionNotSpecitied,
        ChildCountGenerateChildrenActionNotSpecitied,
        ChildCountGenerateChildrenActionNotValid,
        ArtifactTypeApplicableOnlyToGenerateChildArtifactAction,
        ChildCountApplicableOnlyToGenerateChildArtifactAction,
        StateConditionNotOnTriggerOfPropertyChangeEvent,
        StateStateConditionNotSpecified,
        StateStateConditionNotFound,
        PropertyChangeEventActionNotSupported,
        ProjectNoSpecified,
        AmbiguousProjectReference,
        InvalidId,
        ProjectDuplicateId,
        ProjectDuplicatePath,
        ProjectDoesNotHaveAnyArtfactTypes,
        ArtifactTypeNoSpecified,
        DuplicateArtifactTypesInProject,

        // Update specific errors
        WorkflowIdDoesNotMatchIdInUrl,
        DuplicateStateIds,
        DuplicateWorkflowEventIds,
        DuplicateProjectIds,
        DuplicateArtifactTypeIdsInProject
    }
}