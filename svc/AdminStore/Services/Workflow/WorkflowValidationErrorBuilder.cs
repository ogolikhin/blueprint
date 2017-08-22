using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminStore.Models.Workflow;
using ServiceLibrary.Helpers;

namespace AdminStore.Services.Workflow
{
    public class WorkflowValidationErrorBuilder : IWorkflowValidationErrorBuilder
    {
        private const string TemplateWorkflowImportFailedSingular = "There was an error uploading {0}.{1}";
        private const string TemplateWorkflowImportFailedPlural = "There were errors uploading {0}.{1}";
        private const string ReplacementNotSpecifiedFileName = "the XML";
        //The supplied XML is not valid. Please edit your file and upload again.
        private const string XmlIsNotValid = "The supplied XML is not valid. Please edit your file and upload again.";

        // Messages for the XML validation.
        private const string TemplateXmlWorkflowNameEmpty = "The required field 'Name' of the Workflow is missing.";
        private const string TemplateXmlWorkflowNameExceedsLimit24 = "The field 'Name' of the Workflow is over the character limit of 24.";
        private const string TemplateXmlWorkflowDescriptionExceedsLimit4000 = "The field 'Description' of the Workflow is over the character limit of 4000.";
        private const string TemplateXmlWorkflowDoesNotContainAnyStates = "There are not states defined in the file";
        private const string TemplateXmlStatesCountExceedsLimit100 = "Your file exceeded the limit of permitted States for a Workflow.";
        private const string TemplateXmlStateNameEmpty = "One or more State Names do not have a value.";
        private const string TemplateXmlStateNameExceedsLimit24 = "The field 'Name' of State '{0}' is over the character limit of 24.";
        private const string TemplateXmlStateNameNotUnique = "Two or more States '{0}' are the same. State names in a workflow must be unique.";
        private const string TemplateXmlNoInitialState = "There is no starting State defined. All workflows must have a single start state defined.";
        private const string TemplateXmlInitialStateDoesNotHaveOutgoingTransition = "The initial State '{0}' does not have any outbound transitions. The initial State must have at least one outbound transition.";
        private const string TemplateXmlMultipleInitialStates = "More than one starting States are defined. All workflows must have only one defined starting state.";
        private const string TemplateXmlStateDoesNotHaveAnyTransitions = "State '{0}' is not connected by a transition. A State must have at least one transition.";
        private const string TemplateXmlTransitionEventNameEmpty = "One or more Transition names do not have a value. All transition names must have a value.";
        private const string TemplateXmlTransitionEventNameExceedsLimit24 = "The field 'Name' of Transition '{0}' is over the character limit of 24.";
        private const string TemplateXmlPropertyChangeEventNameEmpty = "One or more Property Change Event names do not have a value. All Property Change Event names must have a value.";
        private const string TemplateXmlPropertyChangeEventNameExceedsLimit24 = "The field 'Name' of Property Change Event '{0}' is over the character limit of 24.";
        private const string TemplateXmlNewArtifactEventNameEmpty = "One or more New Artifact Event names do not have a value. All New Artifact Event names must have a value.";
        private const string TemplateXmlNewArtifactEventNameExceedsLimit24 = "The field 'Name' of New Artifact Event '{0}' is over the character limit of 24.";
        private const string TemplateXmlWorkflowEventNameNotUniqueInWorkflow = "Two or more Events (Transition, Property Change, New Artifact) '{0}' are the same. Event names in a workflow must be unique.";
        private const string TemplateXmlTransitionCountOnStateExceedsLimit10 = "State '{0}' exceeded the limit of permitted Transitions per State of 10.";
        private const string TemplateXmlTransitionStateNotFound = "One of States of Transition '{0}' is not found. A Transition must connect two States located in the Workflow.";
        private const string TemplateXmlTransitionStartStateNotSpecified = "The Start State of Transition '{0}' is not specified.";
        private const string TemplateXmlTransitionEndStateNotSpecified = "The End State of Transition '{0}' is not specified.";
        private const string TemplateXmlTransitionFromAndToStatesSame = "The Start and End States of Transition '{0}' are the same. The Start and End States of a Transition must be different.";
        private const string TemplateXmlTriggerCountOnEventExceedsLimit10 = "Event (Transition, Property Change, New Artifact) '{0}' exceeded the limit of permitted Triggers per Event of 10.";
        private const string TemplateXmlPropertyChangEventPropertyNotSpecified = "Property of Property Change Event '{0}' is not specified.";
        private const string TemplateXmlProjectNoSpecified = "One or more Projects are not specified. A Project must be specified with Id or Path.";
        private const string TemplateXmlAmbiguousProjectReference = "One or more Projects have ambiguous Reference. A Project must be specified either by Id or by Path.";
        private const string TemplateXmlInvalidId = "One or more Ids are invalid. The Id must be greater than zero.";
        private const string TemplateXmlProjectDuplicateId = "One or more Projects have a duplicate Id. Projects in a Workflow must be unique.";
        private const string TemplateXmlProjectInvalidPath = "One or more Projects have a duplicate Project Path. Projects in a Workflow must be unique.";
        private const string TemplateXmlProjectDoesNotHaveAnyArtfactTypes = "One or more Projects do not have Artifact Types. A Project must have at least one Artifact Type.";
        private const string TemplateXmlArtifactTypeNoSpecified = "One or more Artifact Types are not specified. An Artifact Types must be specified.";
        private const string TemplateXmlPropertyChangeEventNoAnyTriggersSpecified = "One or more Property Change Events do not have any triggers. A Property Change Event must have at least one Trigger.";
        private const string TemplateXmlNewArtifactEventNoAnyTriggersSpecified = "One or more New Artifact Events do not have any triggers. A New Artifact Event must have at least one Trigger.";
        private const string TemplateXmlActionTriggerNotSpecified = "One or more Triggers do not have an Action. A Trigger must have an Action.";
        private const string TemplateXmlRecipientsEmailNotificationActionNotSpecitied = "One or more Email Notification Actions do not have specified recipients. Recipients can be specified as a list of emails or a Property that contains recipients.";
        private const string TemplateXmlAmbiguousRecipientsSourcesEmailNotificationAction = "One or more Email Notification Actions have ambiguous recipients. Recipients must be specified either as a list of emails or a Property that contains recipients.";
        private const string TemplateXmlEmailInvalidEmailNotificationAction = "'{0}' is not a valid email.";
        private const string TemplateXmlMessageEmailNotificationActionNotSpecitied = "One or more Email Notification Actions do not have a specified message. An Email Notification Action must have a message.";
        private const string TemplateXmlPropertyNamePropertyChangeActionNotSpecitied = "One or more Property Change Actions do not have a specified Property Name. A Property Change Action must have a Property Name.";
        private const string TemplateXmlPropertyValuePropertyChangeActionNotSpecitied = "One or more Property Change Actions do not have a specified Property Value. A Property Change Action must have one of the following values, a Property Value or Valid Values or Users and Groups.";
        private const string TemplateXmlAmbiguousPropertyValuePropertyChangeAction = "One or more Property Change Actions have ambiguous Property Values. A Property Change Action must have only one of the following values, a Property Value or Valid Values or Users and Groups.";
        private const string TemplateXmlAmbiguousGroupProjectReference = "One or more Property Change Actions have ambiguous Group Project Reference. A Group Project must be specified either by Id or by Path.";
        private const string TemplateXmlArtifactTypeGenerateChildrenActionNotSpecitied = "One or more Generate Children Actions do not have a specified Artifact Type. A Generate Children Action must have an Artifact Type.";
        private const string TemplateXmlChildCountGenerateChildrenActionNotSpecitied = "One or more Generate Children Actions do not have a specified Child Count. A Generate Children Action must have a Child Count.";
        private const string TemplateXmlStateConditionNotOnTriggerOfPropertyChangeEvent = "One or more Triggers of Transitions or New Artifact Events have a State Condition. Only Triggers of Property Change Events can have a State Condition.";
        private const string TemplateXmlStateStateConditionNotSpecified = "One or more States missing on State Conditions of Triggers. The State must be specified on a State Condition.";
        private const string TemplateXmlStateStateConditionNotFound = "State '{0}' of a State Condition is not found. The State of a State Condition must be in the Workflow.";
        private const string TemplateXmlPropertyChangeEventActionNotSupported = "One or more Property Change Events have unsupported Actions. A Property Change Event supports only Email Notification Action";
        private const string TemplateXmlDuplicateArtifactTypesInProject = "One or more Projects contain duplicate Artifact Types. Artifact Types in a Project must be unique.";
        // Workflow Update specific messages
        private const string TemplateXmlWorkflowIdDoesNotMatchIdInUrl = "The Workflow Id in XML does not match the Workflow to update, Id in URL. You probably supplied a wrong Workflow XML file.";
        private const string TemplateXmlDuplicateStateIds = "One or more States have a duplicate Id. A State Id must be unique.";
        private const string TemplateXmlDuplicateWorkflowEventIds = "One or more Workflow Events have a duplicate Id. A Workflow Event Id must be unique.";
        private const string TemplateXmlDuplicateProjectIds = "One or more Projects have a duplicate Id. A Project Id must be unique.";
        private const string TemplateXmlDuplicateArtifactTypeIdsInProject = "One or more ArtifactTypes in a Project have a duplicate Id. A Artifact Type in a Project Id must be unique.";

        // Messages for the Data validation.
        private const string TemplateDataWorkflowNameNotUnique = "A Workflow with Name '{0}' already exists. Workflows in Blueprint must have unique names.";
        private const string TemplateDataProjectByPathNotFound = "Project by Path '{0}' is not found in Blueprint.";
        private const string TemplateDataProjectByIdNotFound = "Project by ID '{0}' is not found in Blueprint.";
        private const string TemplateDataProjectIdDuplicate = "The Workflow contains duplicate projects.";
        private const string TemplateDataInstanceGroupNotFound = "Instance Group '{0}' is not found in Blueprint.";
        private const string TemplateDataStandardArtifactTypeNotFound = "Standard Artifact Type '{0}' is not found.";
        private const string TemplateDataArtifactTypeInProjectAlreadyAssociatedWithWorkflow = "Artifact Type '{0}' in Project '{1}' is already is associated with a Workflow.";
        private const string TemplateDataPropertyNotFound = "Property '{0}' of a Property Change Event is not found in Blueprint.";
        private const string TemplateDataGenerateChildArtifactsActionArtifactTypeNotFound = "Artifact Type '{0}' of a Generate Child Artifacts Action is not found in Blueprint.";
        private const string TemplateDataEmailNotificationActionPropertyTypeNotFound = "Property Type '{0}' of an Email Notification Action is not found in Blueprint.";
        private const string TemplateDataPropertyChangeActionPropertyTypeNotFound = "Property Type '{0}' of a Property Change Action is not found in Blueprint.";
        private const string TemplateDataPropertyChangeActionRequiredPropertyValueEmpty = "The Value of required Property '{0}' in a Property Change Action is empty.";
        private const string TemplateDataPropertyChangeActionUserOrGroupNotSpecified = "One or more Users or Groups in Value of User Property '{0}' in a Property Change Action are not specified.";
        private const string TemplateDataPropertyChangeActionUserNotFound = "One or more Users in Value of User Property '{0}' in a Property Change Action are not found.";
        private const string TemplateDataPropertyChangeActionGroupNotFound = "One or more Groups in Value of User Property '{0}' in a Property Change Action are not found.";
        private const string TemplateDataPropertyChangeActionChoiceValueSpecifiedAsNotValidated = "The Value of Validated Choice Property '{0}' in a Property Change Action is specified as not validated.";
        private const string TemplateDataPropertyChangeActionValidValueNotSpecified = "One or more Valid Values in Value of Choice Property '{0}' in a Property Change Action are not specified.";
        private const string TemplateDataPropertyChangeActionValidValueNotFound = "One or more Valid Values in Value of Choice Property '{0}' in a Property Change Action are not found.";
        private const string TemplateDataPropertyChangeActionInvalidNumberFormat = "The Value of Number Property '{0}' in a Property Change Action has an invalid number format.";
        private const string TemplateDataPropertyChangeActionInvalidNumberDecimalPlaces = "The Value of Number Property '{0}' in a Property Change Action has an invalid decimal places.";
        private const string TemplateDataPropertyChangeActionNumberOutOfRange = "The Value of Number Property '{0}' in a Property Change Action is out of the range.";
        private const string TemplateDataPropertyChangeActionInvalidDateFormat = "The Value of Date Property '{0}' in a Property Change Action has an invalid date format.";
        private const string TemplateDataPropertyChangeActionDateOutOfRange = "The Value of Date Property '{0}' in a Property Change Action is out of the range.";
        // Workflow Update specific messages
        private const string TemplateDataPWorkflowActive = "The Workflow '{0}' [ID = {1}] is Active. An Active Workflow cannot be updated.";
        private const string TemplateDataStateNotFoundByIdInCurrent = "The State '{0}' [ID = {1}] is not found by ID in the current workflow.";
        private const string TemplateDataTransitionEventNotFoundByIdInCurrent = "The Transition Event '{0}' [ID = {1}] is not found by ID in the current workflow.";
        private const string TemplateDataPropertyChangeEventNotFoundBuIdInCurrent = "The Property Change Event '{0}' [ID = {1}] is not found by ID in the current workflow.";
        private const string TemplateDataNewArtifactEventNotFoundByIdInCurrent = "The New Artifact Event '{0}' [ID = {1}] is not found by ID in the current workflow.";
        private const string TemplateDataProjectArtifactTypeNotFoundByIdInCurrent = "The Standard Artifact Type '{1}' [ID = {2}] in Project [ID = {0}] is not found by ID in the current workflow.";
        private const string TemplateDataWorkflowNothingToUpdate = "The provided workflow does not contain any updates.";

        #region Interface Implementation

        public string BuildTextXmlErrors(IEnumerable<WorkflowXmlValidationError> errors, string fileName, bool isEditFileMessage = true)
        {
            var errorList = errors.ToList();
            var sb = new StringBuilder();
            AppendLine(sb, errorList.Count > 1 ? TemplateWorkflowImportFailedPlural : TemplateWorkflowImportFailedSingular,
                string.IsNullOrWhiteSpace(fileName) ? ReplacementNotSpecifiedFileName : fileName,
                isEditFileMessage ? " " + XmlIsNotValid : string.Empty);

            foreach (var error in errorList)
            {
                string template;
                var errParams = GetXmlErrorMessageTemaplateAndParams(error, out template);
                Append(sb, "\t- ");
                AppendLine(sb, template, errParams);
            }

            return sb.ToString();
        }

        public string BuildTextDataErrors(IEnumerable<WorkflowDataValidationError> errors, string fileName, bool isEditFileMessage = true)
        {
            var errorList = errors.ToList();
            var sb = new StringBuilder();

            AppendLine(sb, errorList.Count > 1 ? TemplateWorkflowImportFailedPlural : TemplateWorkflowImportFailedSingular,
                string.IsNullOrWhiteSpace(fileName) ? ReplacementNotSpecifiedFileName : fileName,
                isEditFileMessage ? " " + XmlIsNotValid : string.Empty);

            foreach (var error in errorList)
            {
                string template;
                var errParams = GetDataErrorMessageTemaplateAndParams(error, out template);
                Append(sb, "\t- ");
                AppendLine(sb, template, errParams);
            }

            return sb.ToString();
        }

        #endregion


        private static void AppendLine(StringBuilder sb, string template, params object[] args)
        {
            sb.AppendLine(I18NHelper.FormatInvariant(template, args));
        }

        private static void Append(StringBuilder sb, string template, params object[] args)
        {
            sb.Append(I18NHelper.FormatInvariant(template, args));
        }

        #region Private Methods

        private static object[] GetXmlErrorMessageTemaplateAndParams(WorkflowXmlValidationError error, out string template)
        {
            object[] errParams;

            switch (error.ErrorCode)
            {
                case WorkflowXmlValidationErrorCodes.WorkflowNameEmpty:
                    template = TemplateXmlWorkflowNameEmpty;
                    errParams = new object[] {};
                    break;
                case WorkflowXmlValidationErrorCodes.WorkflowNameExceedsLimit24:
                    template = TemplateXmlWorkflowNameExceedsLimit24;
                    errParams = new object[] {};
                    break;
                case WorkflowXmlValidationErrorCodes.WorkflowDescriptionExceedsLimit4000:
                    template = TemplateXmlWorkflowDescriptionExceedsLimit4000;
                    errParams = new object[] {};
                    break;
                case WorkflowXmlValidationErrorCodes.WorkflowDoesNotContainAnyStates:
                    template = TemplateXmlWorkflowDoesNotContainAnyStates;
                    errParams = new object[] {};
                    break;
                case WorkflowXmlValidationErrorCodes.StatesCountExceedsLimit100:
                    template = TemplateXmlStatesCountExceedsLimit100;
                    errParams = new object[] {};
                    break;
                case WorkflowXmlValidationErrorCodes.StateNameEmpty:
                    template = TemplateXmlStateNameEmpty;
                    errParams = new object[] {};
                    break;
                case WorkflowXmlValidationErrorCodes.StateNameExceedsLimit24:
                    template = TemplateXmlStateNameExceedsLimit24;
                    errParams = new object[] {((IeState) error.Element).Name};
                    break;
                case WorkflowXmlValidationErrorCodes.StateNameNotUnique:
                    template = TemplateXmlStateNameNotUnique;
                    errParams = new object[] {((IeState) error.Element).Name};
                    break;
                case WorkflowXmlValidationErrorCodes.NoInitialState:
                    template = TemplateXmlNoInitialState;
                    errParams = new object[] {};
                    break;
                case WorkflowXmlValidationErrorCodes.InitialStateDoesNotHaveOutgoingTransition:
                    template = TemplateXmlInitialStateDoesNotHaveOutgoingTransition;
                    errParams = new object[] {((IeState) error.Element).Name};
                    break;
                case WorkflowXmlValidationErrorCodes.MultipleInitialStates:
                    template = TemplateXmlMultipleInitialStates;
                    errParams = new object[] {};
                    break;
                case WorkflowXmlValidationErrorCodes.StateDoesNotHaveAnyTransitions:
                    template = TemplateXmlStateDoesNotHaveAnyTransitions;
                    errParams = new object[] {(string) error.Element};
                    break;
                case WorkflowXmlValidationErrorCodes.TransitionEventNameEmpty:
                    template = TemplateXmlTransitionEventNameEmpty;
                    errParams = new object[] {};
                    break;
                case WorkflowXmlValidationErrorCodes.TransitionEventNameExceedsLimit24:
                    template = TemplateXmlTransitionEventNameExceedsLimit24;
                    errParams = new object[] {((IeTransitionEvent) error.Element).Name};
                    break;
                case WorkflowXmlValidationErrorCodes.PropertyChangeEventNameEmpty:
                    template = TemplateXmlPropertyChangeEventNameEmpty;
                    errParams = new object[] {};
                    break;
                case WorkflowXmlValidationErrorCodes.PropertyChangeEventNameExceedsLimit24:
                    template = TemplateXmlPropertyChangeEventNameExceedsLimit24;
                    errParams = new object[] {((IePropertyChangeEvent) error.Element).Name};
                    break;
                case WorkflowXmlValidationErrorCodes.NewArtifactEventNameEmpty:
                    template = TemplateXmlNewArtifactEventNameEmpty;
                    errParams = new object[] {};
                    break;
                case WorkflowXmlValidationErrorCodes.NewArtifactEventNameExceedsLimit24:
                    template = TemplateXmlNewArtifactEventNameExceedsLimit24;
                    errParams = new object[] {((IeNewArtifactEvent) error.Element).Name};
                    break;
                case WorkflowXmlValidationErrorCodes.WorkflowEventNameNotUniqueInWorkflow:
                    template = TemplateXmlWorkflowEventNameNotUniqueInWorkflow;
                    errParams = new object[] {((IeEvent) error.Element).Name};
                    break;
                case WorkflowXmlValidationErrorCodes.TransitionCountOnStateExceedsLimit10:
                    template = TemplateXmlTransitionCountOnStateExceedsLimit10;
                    errParams = new object[] {(string) error.Element};
                    break;
                case WorkflowXmlValidationErrorCodes.TransitionStateNotFound:
                    template = TemplateXmlTransitionStateNotFound;
                    errParams = new object[] {((IeTransitionEvent) error.Element).Name};
                    break;
                case WorkflowXmlValidationErrorCodes.TransitionStartStateNotSpecified:
                    template = TemplateXmlTransitionStartStateNotSpecified;
                    errParams = new object[] {((IeTransitionEvent) error.Element).Name};
                    break;
                case WorkflowXmlValidationErrorCodes.TransitionEndStateNotSpecified:
                    template = TemplateXmlTransitionEndStateNotSpecified;
                    errParams = new object[] {((IeTransitionEvent) error.Element).Name};
                    break;
                case WorkflowXmlValidationErrorCodes.TransitionFromAndToStatesSame:
                    template = TemplateXmlTransitionFromAndToStatesSame;
                    errParams = new object[] {((IeTransitionEvent) error.Element).Name};
                    break;
                case WorkflowXmlValidationErrorCodes.TriggerCountOnEventExceedsLimit10:
                    template = TemplateXmlTriggerCountOnEventExceedsLimit10;
                    errParams = new object[] {((IeEvent) error.Element).Name};
                    break;
                case WorkflowXmlValidationErrorCodes.PropertyChangeEventPropertyNotSpecified:
                    template = TemplateXmlPropertyChangEventPropertyNotSpecified;
                    errParams = new object[] {((IePropertyChangeEvent) error.Element).Name};
                    break;
                case WorkflowXmlValidationErrorCodes.ProjectNoSpecified:
                    template = TemplateXmlProjectNoSpecified;
                    errParams = new object[] {};
                    break;
                case WorkflowXmlValidationErrorCodes.AmbiguousProjectReference:
                    template = TemplateXmlAmbiguousProjectReference;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.InvalidId:
                    template = TemplateXmlInvalidId;
                    errParams = new object[] {};
                    break;
                case WorkflowXmlValidationErrorCodes.ProjectDuplicateId:
                    template = TemplateXmlProjectDuplicateId;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.ProjectDuplicatePath:
                    template = TemplateXmlProjectInvalidPath;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.ProjectDoesNotHaveAnyArtfactTypes:
                    template = TemplateXmlProjectDoesNotHaveAnyArtfactTypes;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.ArtifactTypeNoSpecified:
                    template = TemplateXmlArtifactTypeNoSpecified;
                    errParams = new object[] {};
                    break;
                case WorkflowXmlValidationErrorCodes.PropertyChangeEventNoAnyTriggersNotSpecified:
                    template = TemplateXmlPropertyChangeEventNoAnyTriggersSpecified;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.NewArtifactEventNoAnyTriggersNotSpecified:
                    template = TemplateXmlNewArtifactEventNoAnyTriggersSpecified;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.ActionTriggerNotSpecified:
                    template = TemplateXmlActionTriggerNotSpecified;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.RecipientsEmailNotificationActionNotSpecitied:
                    template = TemplateXmlRecipientsEmailNotificationActionNotSpecitied;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.AmbiguousRecipientsSourcesEmailNotificationAction:
                    template = TemplateXmlAmbiguousRecipientsSourcesEmailNotificationAction;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.EmailInvalidEmailNotificationAction:
                    template = TemplateXmlEmailInvalidEmailNotificationAction;
                    errParams = new object[] { (string)error.Element };
                    break;
                case WorkflowXmlValidationErrorCodes.MessageEmailNotificationActionNotSpecitied:
                    template = TemplateXmlMessageEmailNotificationActionNotSpecitied;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.PropertyNamePropertyChangeActionNotSpecitied:
                    template = TemplateXmlPropertyNamePropertyChangeActionNotSpecitied;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.PropertyValuePropertyChangeActionNotSpecitied:
                    template = TemplateXmlPropertyValuePropertyChangeActionNotSpecitied;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.AmbiguousPropertyValuePropertyChangeAction:
                    template = TemplateXmlAmbiguousPropertyValuePropertyChangeAction;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.AmbiguousGroupProjectReference:
                    template = TemplateXmlAmbiguousGroupProjectReference;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.ArtifactTypeGenerateChildrenActionNotSpecitied:
                    template = TemplateXmlArtifactTypeGenerateChildrenActionNotSpecitied;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.ChildCountGenerateChildrenActionNotSpecitied:
                    template = TemplateXmlChildCountGenerateChildrenActionNotSpecitied;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.StateConditionNotOnTriggerOfPropertyChangeEvent:
                    template = TemplateXmlStateConditionNotOnTriggerOfPropertyChangeEvent;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.StateStateConditionNotSpecified:
                    template = TemplateXmlStateStateConditionNotSpecified;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.StateStateConditionNotFound:
                    template = TemplateXmlStateStateConditionNotFound;
                    errParams = new object[] { (string)error.Element };
                    break;
                case WorkflowXmlValidationErrorCodes.PropertyChangeEventActionNotSupported:
                    template = TemplateXmlPropertyChangeEventActionNotSupported;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.DuplicateArtifactTypesInProject:
                    template = TemplateXmlDuplicateArtifactTypesInProject;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.WorkflowIdDoesNotMatchIdInUrl:
                    template = TemplateXmlWorkflowIdDoesNotMatchIdInUrl;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.DuplicateStateIds:
                    template = TemplateXmlDuplicateStateIds;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.DuplicateWorkflowEventIds:
                    template = TemplateXmlDuplicateWorkflowEventIds;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.DuplicateProjectIds:
                    template = TemplateXmlDuplicateProjectIds;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.DuplicateArtifactTypeIdsInProject:
                    template = TemplateXmlDuplicateArtifactTypeIdsInProject;
                    errParams = new object[] { };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(error.ErrorCode));
            }

            return errParams;
        }

        private static object[] GetDataErrorMessageTemaplateAndParams(WorkflowDataValidationError error, out string template)
        {
            object[] errParams;

            switch (error.ErrorCode)
            {
                case WorkflowDataValidationErrorCodes.WorkflowNameNotUnique:
                    template = TemplateDataWorkflowNameNotUnique;
                    errParams = new object[] { ((IeWorkflow)error.Element).Name };
                    break;
                case WorkflowDataValidationErrorCodes.ProjectByPathNotFound:
                    template = TemplateDataProjectByPathNotFound;
                    errParams = new object[] { (string) error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.ProjectByIdNotFound:
                    template = TemplateDataProjectByIdNotFound;
                    errParams = new object[] { (int)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.ProjectDuplicate:
                    template = TemplateDataProjectIdDuplicate;
                    errParams = new object[] {};
                    break;
                case WorkflowDataValidationErrorCodes.InstanceGroupNotFound:
                    template = TemplateDataInstanceGroupNotFound;
                    errParams = new object[] { (string) error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.StandardArtifactTypeNotFound:
                    template = TemplateDataStandardArtifactTypeNotFound;
                    errParams = new object[] { (string) error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.ArtifactTypeInProjectAlreadyAssociatedWithWorkflow:
                    template = TemplateDataArtifactTypeInProjectAlreadyAssociatedWithWorkflow;
                    var t = (Tuple<int, string>) error.Element;
                    errParams = new object[] { t?.Item2, t?.Item1 };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyNotFound:
                    template = TemplateDataPropertyNotFound;
                    errParams = new object[] { (string) error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.GenerateChildArtifactsActionArtifactTypeNotFound:
                    template = TemplateDataGenerateChildArtifactsActionArtifactTypeNotFound;
                    errParams = new object[] { (string) error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.EmailNotificationActionPropertyTypeNotFound:
                    template = TemplateDataEmailNotificationActionPropertyTypeNotFound;
                    errParams = new object[] { (string) error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyChangeActionPropertyTypeNotFound:
                    template = TemplateDataPropertyChangeActionPropertyTypeNotFound;
                    errParams = new object[] { (string) error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredPropertyValueEmpty:
                    template = TemplateDataPropertyChangeActionRequiredPropertyValueEmpty;
                    errParams = new object[] { (string)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyChangeActionUserOrGroupNotSpecified:
                    template = TemplateDataPropertyChangeActionUserOrGroupNotSpecified;
                    errParams = new object[] { (string)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyChangeActionUserNotFound:
                    template = TemplateDataPropertyChangeActionUserNotFound;
                    errParams = new object[] { (string)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyChangeActionGroupNotFound:
                    template = TemplateDataPropertyChangeActionGroupNotFound;
                    errParams = new object[] { (string)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyChangeActionChoiceValueSpecifiedAsNotValidated:
                    template = TemplateDataPropertyChangeActionChoiceValueSpecifiedAsNotValidated;
                    errParams = new object[] { (string)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyChangeActionValidValueNotSpecified:
                    template = TemplateDataPropertyChangeActionValidValueNotSpecified;
                    errParams = new object[] { (string)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyChangeActionValidValueNotFound:
                    template = TemplateDataPropertyChangeActionValidValueNotFound;
                    errParams = new object[] { (string)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyChangeActionInvalidNumberFormat:
                    template = TemplateDataPropertyChangeActionInvalidNumberFormat;
                    errParams = new object[] { (string)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyChangeActionInvalidNumberDecimalPlaces:
                    template = TemplateDataPropertyChangeActionInvalidNumberDecimalPlaces;
                    errParams = new object[] { (string)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyChangeActionNumberOutOfRange:
                    template = TemplateDataPropertyChangeActionNumberOutOfRange;
                    errParams = new object[] { (string)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyChangeActionInvalidDateFormat:
                    template = TemplateDataPropertyChangeActionInvalidDateFormat;
                    errParams = new object[] { (string)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyChangeActionDateOutOfRange:
                    template = TemplateDataPropertyChangeActionDateOutOfRange;
                    errParams = new object[] { (string)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.WorkflowActive:
                    template = TemplateDataPWorkflowActive;
                    var workflow = (IeWorkflow) error.Element;
                    errParams = new object[] { workflow.Name, workflow.Id };
                    break;
                case WorkflowDataValidationErrorCodes.StateNotFoundByIdInCurrent:
                    template = TemplateDataStateNotFoundByIdInCurrent;
                    var state = (IeState) error.Element;
                    errParams = new object[] { state.Name, state.Id };
                    break;
                case WorkflowDataValidationErrorCodes.TransitionEventNotFoundByIdInCurrent:
                    template = TemplateDataTransitionEventNotFoundByIdInCurrent;
                    var te = (IeTransitionEvent) error.Element;
                    errParams = new object[] { te.Name, te.Id };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyChangeEventNotFoundBuIdInCurrent:
                    template = TemplateDataPropertyChangeEventNotFoundBuIdInCurrent;
                    var pce = (IePropertyChangeEvent )error.Element;
                    errParams = new object[] { pce.Name, pce.Id };
                    break;
                case WorkflowDataValidationErrorCodes.NewArtifactEventNotFoundByIdInCurrent:
                    template = TemplateDataNewArtifactEventNotFoundByIdInCurrent;
                    var nae = (IeNewArtifactEvent) error.Element;
                    errParams = new object[] { nae.Name, nae.Id };
                    break;
                case WorkflowDataValidationErrorCodes.ProjectArtifactTypeNotFoundByIdInCurrent:
                    template = TemplateDataProjectArtifactTypeNotFoundByIdInCurrent;
                    var tuple  = (Tuple<IeProject, IeArtifactType>) error.Element;
                    errParams = new object[] { tuple.Item1.Id, tuple.Item2.Name, tuple.Item2.Id };
                    break;
                case WorkflowDataValidationErrorCodes.WorkflowNothingToUpdate:
                    template = TemplateDataWorkflowNothingToUpdate;
                    errParams = new object[] { };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return errParams;
        }

        #endregion
    }
}