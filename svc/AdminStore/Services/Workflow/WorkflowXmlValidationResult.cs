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
        StateNameNotUnique,
        NoInitialState,
        InitialStateDoesNotHaveOutgoingTransition,
        MultipleInitialStates,
        StateDoesNotHaveAnyTransitions,
        TransitionEventNameEmpty,
        TransitionEventNameExceedsLimit24,
        PropertyChangeEventNameEmpty,
        PropertyChangeEventNameExceedsLimit24,
        NewArtifactEventNameEmpty,
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
        ArtifactTypeGenerateChildrenActionNotSpecitied,
        ChildCountGenerateChildrenActionNotSpecitied,
        ProjectNoSpecified,
        ProjectInvalidId,
        ArtifactTypeNoSpecified,
        ProjectsProvidedWithoutArifactTypes,
        ArtifactTypesProvidedWithoutProjects
    }
}