﻿using System.Collections.Generic;
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
        WorkflowNameMissingOrInvalid,
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
        StatesNotConnectedToInitialState,
        TransitionEventNameEmpty,
        TransitionEventNameExceedsLimit24,
        PropertyChangeEventNameExceedsLimit24,
        NewArtifactEventNameExceedsLimit24,
        StateWithDuplicateOutgoingTransitions,
        TransitionCountOnStateExceedsLimit10,
        TransitionStateNotFound,
        TransitionStartStateNotSpecified,
        TransitionEndStateNotSpecified,
        TransitionFromAndToStatesSame,
        TriggerCountOnEventExceedsLimit10,
        PropertyChangeEventPropertyNotSpecified,
        PropertyChangeEventDuplicateProperties,
        PropertyChangeEventNoAnyTriggersNotSpecified,
        NewArtifactEventNoAnyTriggersNotSpecified,
        PropertyChangeActionDuplicatePropertiesOnEvent,
        ActionTriggerNotSpecified,
        RecipientsEmailNotificationActionNotSpecified,
        AmbiguousRecipientsSourcesEmailNotificationAction,
        EmailInvalidEmailNotificationAction,
        MessageEmailNotificationActionNotSpecified,
        PropertyNamePropertyChangeActionNotSpecified,
        PropertyValuePropertyChangeActionNotSpecified,
        AmbiguousPropertyValuePropertyChangeAction,
        PropertyChangeActionUserOrGroupNameNotSpecified,
        AmbiguousGroupProjectReference,
        ArtifactTypeGenerateChildrenActionNotSpecified,
        ChildCountGenerateChildrenActionNotSpecified,
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