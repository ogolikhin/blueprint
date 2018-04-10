namespace AdminStore.Services.Workflow.Validation.Data
{
    public enum WorkflowDataValidationErrorCodes
    {
        WorkflowNameNotUnique,
        ProjectByPathNotFound,
        ProjectByIdNotFound,
        ProjectDuplicate,
        InstanceGroupNotFoundByName,
        StandardArtifactTypeNotFoundByName,
        ArtifactTypeInProjectAlreadyAssociatedWithWorkflow,
        PropertyNotFoundByName,
        PropertyNotAssociated,
        GenerateChildArtifactsActionArtifactTypeNotFoundByName,
        EmailNotificationActionPropertyTypeNotFoundByName,
        EmailNotificationActionPropertyTypeNotAssociated,
        EmailNotificationActionUnacceptablePropertyType,
        PropertyChangeActionPropertyTypeNotFoundByName,
        PropertyChangeActionPropertyTypeNotAssociated,
        // Property Value Validation error codes
        PropertyChangeActionRequiredPropertyValueEmpty,
        PropertyChangeActionUserNotFoundByName,
        PropertyChangeActionGroupNotFoundByName,
        PropertyChangeActionChoiceValueSpecifiedAsNotValidated,
        PropertyChangeActionDuplicateValidValueFound,
        PropertyChangeActionDuplicateUserOrGroupFound,
        PropertyChangeActionValidValueNotFoundByValue,
        PropertyChangeActionInvalidNumberFormat,
        PropertyChangeActionInvalidNumberDecimalPlaces,
        PropertyChangeActionNumberOutOfRange,
        PropertyChangeActionInvalidDateFormat,
        PropertyChangeActionDateOutOfRange,

        //
        PropertyChangeActionNotChoicePropertyValidValuesNotApplicable,
        PropertyChangeActionNotUserPropertyUsersGroupsNotApplicable,
        PropertyChangeActionRequiredUserPropertyPropertyValueNotApplicable,
        PropertyChangeActionChoicePropertyMultipleValidValuesNotAllowed, // Move up later

        // Update specific errors
        WorkflowActive,
        StateNotFoundByIdInCurrent,
        TransitionEventNotFoundByIdInCurrent,
        PropertyChangeEventNotFoundByIdInCurrent,
        NewArtifactEventNotFoundByIdInCurrent,
        ProjectArtifactTypeNotFoundByIdInCurrent,
        WorkflowNothingToUpdate,
        StandardArtifactTypeNotFoundById,
        PropertyNotFoundById,
        InstanceGroupNotFoundById,
        EmailNotificationActionPropertyTypeNotFoundById,
        PropertyChangeActionPropertyTypeNotFoundById,
        GenerateChildArtifactsActionArtifactTypeNotFoundById,
        PropertyChangeActionValidValueNotFoundById,
        PropertyChangeActionGroupNotFoundById,
        PropertyChangeActionUserNotFoundById,
        WebhookActionNotFoundById
    }
}
