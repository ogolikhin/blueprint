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
        private const string XmlIsNotValid = "The supplied XML is not valid. Please edit your file and upload again.";

        // Messages for the XML validation.
        private const string TemplateXmlWorkflowXmlSerializationError = "{0}";
        private const string TemplateXmlWorkflowNameEmpty = "The Workflow's <Name> element is missing, or has no value.";
        private const string TemplateXmlWorkflowNameExceedsLimit24 = "The Workflow's <Name> element exceeds 24 characters.";
        private const string TemplateXmlWorkflowDescriptionExceedsLimit4000 = "The Workflow's <Description> element exceeds 4000 characters.";
        private const string TemplateXmlWorkflowDoesNotContainAnyStates = "There are no States defined in this Workflow. Please ensure the XML definition includes a <States> element, and two or more <State> child elements.";
        private const string TemplateXmlStatesCountExceedsLimit100 = "The maximum 100 States allowed in a Workflow has been exceeded.";
        private const string TemplateXmlStateNameEmpty = "<State> element: One or more <Name> child elements are missing, or do not have a value.";
        private const string TemplateXmlStateNameExceedsLimit24 = "State element '{0}': The value of the <Name> child element exceeds 24 characters.";
        private const string TemplateXmlStateNameNotUnique = "Two or more <State> elements have the same name ({0}). State names in a Workflow must be unique.";
        private const string TemplateXmlNoInitialState = "No initial State has been defined. Please ensure one of your <State> elements includes an attribute and value of IsInitial='true'.";
        private const string TemplateXmlInitialStateDoesNotHaveOutgoingTransition = "There is no Transition that originates from the initial State. Please ensure the <State> element that is set as the initial State is an outgoing component of at least one Transition--that is, the State name is a value for the Transition <FromState>. (The State ID and Transition <FromStateId> element could also be used.)";
        private const string TemplateXmlMultipleInitialStates = "More than one initial state has been defined; Workflows can have only one initial state. Please ensure only one of your <State> elements has the value 'true' for the IsInitial attribute.";
        private const string TemplateXmlStateDoesNotHaveAnyTransitions = "The State '{0}' is not connected to any other States by a Transition. Please ensure the State is an incoming or outgoing component of at least one Transition--that is, the State name is a value for the Transition <FromState>. (The State ID and Transition <FromStateId> could also be used.)";
        private const string TemplateXmlTransitionEventNameEmpty = "<Transition> element: One or more <Name> child elements are missing, or do not have a value.";
        private const string TemplateXmlTransitionEventNameExceedsLimit24 = "Transition element '{0}': The value of the <Name> child element exceeds 24 characters.";
        private const string TemplateXmlPropertyChangeEventNameExceedsLimit24 = "Property Change element '{0}': The value of the <Name> child element exceeds 24 characters.";
        private const string TemplateXmlNewArtifactEventNameExceedsLimit24 = "New Artifact element '{0}': The value of the <Name> child element exceeds 24 characters.";
        private const string TemplateXmlStateWithDuplicateOutgoingTransitions = "Some duplicate Transition names conflict because they have the same originating State. Please ensure that any <Transition> elements with the same value for <FromState> have unique values for <Name>.";
        private const string TemplateXmlTransitionCountOnStateExceedsLimit10 = "The State '{0}' has exceeded the number of Transitions it can be connected to. Please ensure the State is an incoming or outgoing component of 10 or fewer Transitions.";
        private const string TemplateXmlTransitionStateNotFound = "One of the States referenced in the Transition '{0}' was not found. Please ensure the State names in the Transition <FromState> and <ToState> child elements correspond to States described earlier in the Workflow definition. (The <FromStateId> and <ToStateId> child elements could also be used.)";
        private const string TemplateXmlTransitionStartStateNotSpecified = "Transition element '{0}': The starting State has not been defined. Please ensure the <FromState> or <FromStateId> child elements have correct values.";
        private const string TemplateXmlTransitionEndStateNotSpecified = "Transition element '{0}': The end State has not been defined. Please ensure the <ToState> or <ToStateId> child elements have correct values.";
        private const string TemplateXmlTransitionFromAndToStatesSame = "Transition element '{0}': The starting and end States are the same. Please ensure the <FromState> and <ToState> have correct values. (The , <FromStateId> and <ToStateId> child elements could also be used.)";
        private const string TemplateXmlTriggerCountOnEventExceedsLimit10 = "An Event (Transition, Property Change, New Artifact) has exceeded the number of Triggers it can include. Please ensure '{0}' includes 10 or fewer Triggers.";
        private const string TemplateXmlPropertyChangEventPropertyNotSpecified = "The required property for a Property Change event ‘{0}’ has not been specified.";
        private const string TemplateXmlPropertyChangeEventDuplicateProperties = "<PropertyChange> elements: There are one or more duplicate properties. Please ensure all <PropertyName> values are unique.";
        private const string TemplateXmlProjectNoSpecified = "There are no Projects defined in this Workflow. If the XML definition includes a <Projects> element, ensure that there are one or more <Project> child elements.";
        private const string TemplateXmlAmbiguousProjectReference = "One or more Projects are specified using both ID and path. Please ensure all <Project> elements use one or the other, but not both.";
        private const string TemplateXmlInvalidId = "An element has an invalid Id attribute value. IDs must be greater than 0.";
        private const string TemplateXmlProjectDuplicateId = "Two or more <Project> elements have the same ID. Project IDs are unique.";
        private const string TemplateXmlProjectInvalidPath = "Two or more <Project> elements have the same Path. Project paths are unique.";
        private const string TemplateXmlProjectDoesNotHaveAnyArtfactTypes = "<Project> elements: One or more Projects do not include Artifact Types. Please ensure each <Project> element has an <ArtifactTypes> child element.";
        private const string TemplateXmlArtifactTypeNoSpecified = "<Project> elements: One or more Projects do not include Artifact Types. Please ensure each Project's <ArtifactType> child element exists and has a value.";
        private const string TemplateXmlPropertyChangeEventNoAnyTriggersSpecified = "<PropertyChanges> element: One or more Property Change events do not have a trigger. Please ensure each <PropertyChange> child element has a <Triggers> child element, itself with at least one <Trigger> child element.";
        private const string TemplateXmlNewArtifactEventNoAnyTriggersSpecified = "<NewArtifacts> element: One or more New Artifact events do not have a trigger. Please ensure each <NewArtifact> child element has a <Triggers> child element, itself with at least one <Trigger> child element.";
        private const string TemplateXmlPropertyChangeActionDuplicatePropertiesOnEvent = "An Event (Transition, New Artifact) has more than one Property Change Action that changes the same property. Please ensure all <PropertyChangeAction> elements reference unique properties.";
        private const string TemplateXmlActionTriggerNotSpecified = "One or more Triggers do not have a corresponding Action. Please ensure all <Trigger> elements include an Action-type child element (for example, <EmailNotificationAction>).";
        private const string TemplateXmlRecipientsEmailNotificationActionNotSpecified = "One or more email notification Actions do not specify recipients, either as a list of emails, or through a property representing recipients. Please ensure all <EmailNotificationAction> elements either include an <Emails> child element with one or more <Email> child elements, or include <PropertyName> or <PropertyId> child elements.";
        private const string TemplateXmlAmbiguousRecipientsSourcesEmailNotificationAction = "One or more email notification Actions specify recipients using both a list of emails, and a property representing recipients. Please ensure all <EmailNotificationAction> elements use one or the other, but not both.";
        private const string TemplateXmlEmailInvalidEmailNotificationAction = "The email address '{0}' provided for an <EmailNotificationAction> element is not valid.";
        private const string TemplateXmlMessageEmailNotificationActionNotSpecified = "One or more email notification Actions do not include a message. Please ensure all <EmailNotificationAction> elements include a <Message> child element.";
        private const string TemplateXmlPropertyNamePropertyChangeActionNotSpecified = "<PropertyChangeAction> elements: One or more <PropertyName> child elements do not have a value.";
        private const string TemplateXmlPropertyValuePropertyChangeActionNotSpecified = "One or more Property Change Actions are missing information. Please ensure all <PropertyChangeActions> elements include one of the following defined child elements: <PropertyValue>, <ValidValues>, or <UsersGroups>.";
        private const string TemplateXmlAmbiguousPropertyValuePropertyChangeAction = "One or more Property Change Actions specify too many property-value elements. Please ensure all <PropertyChangeAction> elements define only one of the following child elements: <PropertyValue>, <ValidValues>, or <UsersGroups>.";
        private const string TemplateXmlPropertyChangeActionUserOrGroupNameNotSpecified = "One or more Property Change Actions is missing information. One or more Users or Groups do not have a specified Name.";
        private const string TemplateXmlAmbiguousGroupProjectReference = "One or more Property Change Actions specify conflicting group project information. Please ensure all <UsersGroups> child elements define only one of the following child elements: <GroupProjectId> or <GroupProjectPath>.";
        private const string TemplateXmlArtifactTypeGenerateChildrenActionNotSpecified = "One or more 'generate child artifact' Actions do not specify the child artifact type. Please ensure all <GenerateActionType> elements with the value 'Children' are accompanied by an <ArtifactTypeId> element.";
        private const string TemplateXmlChildCountGenerateChildrenActionNotSpecified = "One or more 'generate child artifact' Actions do not specify the number of child artifacts to create. Please ensure all <GenerateActionType> elements with the value 'Children' are accompanied by a <ChildCount> element.";
        private const string TemplateXmlChildCountGenerateChildrenActionNotValid = "One or more actions that result in the creation of child artifacts are invalid. For all <GenerateAction> elements where the <GenerateActionType> value is Children, please ensure all <ChildCount> values are between 1 and 10 inclusive.";
        private const string TemplateXmlArtifactTypeApplicableOnlyToGenerateChildArtifactAction = "One or more actions that result in the generation of test cases or user stories are incorrectly defined. For all <GenerateAction> elements where the <GenerateActionType> value is UserStories or TestCases, please ensure there is no <ArtifactType> child element.";
        private const string TemplateXmlChildCountApplicableOnlyToGenerateChildArtifactAction = "One or more actions that result in the generation of test cases or user stories are incorrectly defined. For all <GenerateAction> elements where the <GenerateActionType> value is UserStories or TestCases, please ensure there is no <ChildCount> child element.";
        private const string TemplateXmlStateConditionNotOnTriggerOfPropertyChangeEvent = "An event Trigger includes a State condition when it should not have one. Only property change events can have triggers that use a State condition. Please ensure the <Trigger> elements for other event types (<NewArtifact> or <Transition>) do not have a <StateCondition> child element.";
        private const string TemplateXmlStateStateConditionNotSpecified = "One or more State conditions that triggers an event is missing the State. Please ensure all <StateCondition> elements have <State> or <StateId> child elements.";
        private const string TemplateXmlStateStateConditionNotFound = "The State '{0}' defined as part of a State condition was not found. Please ensure the State names or IDs in all <StateCondition> elements correspond to States described earlier in the Workflow definition.";
        private const string TemplateXmlPropertyChangeEventActionNotSupported = "One or more property-change events includes an incompatible action; a property change triggers an email notification. For all <PropertyChange> elements, please ensure all <Trigger> child elements have only <EmailNotificationAction> as the triggered action.";
        private const string TemplateXmlDuplicateArtifactTypesInProject = "One or more Projects contain duplicate artifact types. Please ensure for each <Project> element, there are no duplicate <ArtifactType> definitions.";
        // Workflow Update specific messages
        private const string TemplateXmlWorkflowIdDoesNotMatchIdInUrl = "The Workflow Id attribute in the file does not match the Workflow have you chosen to update. Please ensure you are uploading the correct Workflow XML file.";
        private const string TemplateXmlDuplicateStateIds = "One or more States have a duplicate ID. A State ID must be unique.";
        private const string TemplateXmlDuplicateWorkflowEventIds = "One or more Workflow Events have a duplicate ID. A Workflow Event ID must be unique.";
        private const string TemplateXmlDuplicateProjectIds = "One or more Projects have a duplicate ID. A Project ID must be unique.";
        private const string TemplateXmlDuplicateArtifactTypeIdsInProject = "One or more Projects contain duplicate artifact type IDs. Please ensure for each <Project> element, its <ArtifactType> child elements do not have identical 'Id' attribute values. If you are modifying an existing Workflow, try removing the 'Id' attribute from any newly added or changed <ArtifactType> elements.";

        // Messages for the Data validation.
        private const string TemplateDataWorkflowNameNotUnique = "Main <Workflow> element: The value of the <Name> child element already exists. Workflows must have unique names.";
        private const string TemplateDataProjectByPathNotFound = "<Project> elements: The project path '{0}' was not found.";
        private const string TemplateDataProjectByIdNotFound = "<Project> elements: The project ID '{0}' was not found.";
        private const string TemplateDataProjectIdDuplicate = "<Project> elements: There are two or more duplicate projects. Please ensure all <Project> elements have a unique 'Id' attribute value.";
        private const string TemplateDataInstanceGroupNotFoundByName = "No instance group named '{0}' was found.";
        private const string TemplateDataStandardArtifactTypeNotFoundByName = "No standard artifact type named '{0}' was found.";
        private const string TemplateDataArtifactTypeInProjectAlreadyAssociatedWithWorkflow = "Standard artifact type '{0}' is already assigned to a Workflow in project '{1}'.";
        private const string TemplateDataPropertyNotFoundByName = "<PropertyChange> elements: The Property '{0}' was not found.";
        private const string TemplateDataGenerateChildArtifactsActionArtifactTypeNotFoundByName = "<GenerateAction> elements: The name for artifact type '{0}' was not found.";
        private const string TemplateDataEmailNotificationActionPropertyTypeNotFoundByName = "<PropertyChange> elements: For name of property '{0}' that was defined as part of an <EmailNotificationAction> element was not found.";
        private const string TemplateDataEmailNotificationActionUnacceptablePropertyType = "Property Type '{0}' of an Email Notification Action is of an unacceptable Type. Only Text and User Properties can be used in an Email Notification Action.";
        private const string TemplateDataPropertyChangeActionPropertyTypeNotFoundByName = "<Transition> elements: The name of property '{0}' that was defined as part of a <PropertyChangeAction> element was not found.";
        private const string TemplateDataPropertyChangeActionRequiredPropertyValueEmpty = "<PropertyChangeAction> elements: There are no values defined for property '{0}.' ";
        private const string TemplateDataPropertyChangeActionUserNotFoundByName = "<PropertyChangeAction> elements: The name of one or more users defined for property '{0}' were not found.";
        private const string TemplateDataPropertyChangeActionGroupNotFoundByName = "<PropertyChangeAction> elements: The name of one or more groups defined for property '{0}' were not found.";
        private const string TemplateDataPropertyChangeActionChoiceValueSpecifiedAsNotValidated = "<PropertyChangeAction> element: The value of choice property '{0}' is not valid.";
        private const string TemplateDataPropertyChangeActionValidValueNotFoundByValue = "<PropertyChangeAction> elements: One or more values of choice property ‘{0}’ were not found.";
        private const string TemplateDataPropertyChangeActionInvalidNumberFormat = "<PropertyChangeAction> element: The value for number property '{0}' is of an invalid number format.";
        private const string TemplateDataPropertyChangeActionInvalidNumberDecimalPlaces = "<PropertyChangeAction> element: The value for number property '{0}' has an invalid number of decimal places.";
        private const string TemplateDataPropertyChangeActionNumberOutOfRange = "<PropertyChangeAction> element: The value for number property '{0}' is not within an accepted range.";
        private const string TemplateDataPropertyChangeActionInvalidDateFormat = "<PropertyChangeAction> element: The value for date property '{0}' uses an invalid date format.";
        private const string TemplateDataPropertyChangeActionDateOutOfRange = "<PropertyChangeAction> element: The value for date property '{0}' is not within an accepted date range.";
        // Workflow Update specific messages
        private const string TemplateDataPWorkflowActive = "The Workflow '{0}' [ID = {1}] is Active. An Active Workflow cannot be updated.";
        private const string TemplateDataStateNotFoundByIdInCurrent = "The State '{0}' [ID = {1}] is not found by ID in the current Workflow.";
        private const string TemplateDataTransitionEventNotFoundByIdInCurrent = "The Transition Event '{0}' [ID = {1}] is not found by ID in the current Workflow.";
        private const string TemplateDataPropertyChangeEventNotFoundByIdInCurrent = "The Property Change Event '{0}' [ID = {1}] is not found by ID in the current Workflow.";
        private const string TemplateDataNewArtifactEventNotFoundByIdInCurrent = "The New Artifact Event '{0}' [ID ={1}] is not found by ID in the current Workflow.";
        private const string TemplateDataProjectArtifactTypeNotFoundByIdInCurrent = "The Standard Artifact Type '{1}' [ID = {2}] in Project [ID = {0}] is not found by ID in the current Workflow.";
        private const string TemplateDataWorkflowNothingToUpdate = "The provided Workflow does not contain any updates.";
        private const string TemplateDataStandardArtifactTypeNotFoundById = "Standard Artifact Type '{0}' is not found by Id in Blueprint.";
        private const string TemplateDataPropertyNotFoundById = "Property '{0}' of a Property Change Event is not found by Id in Blueprint.";
        private const string TemplateDataInstanceGroupNotFoundById = "Instance Group '{0}' is not found by Id in Blueprint.";
        private const string TemplateDataEmailNotificationActionPropertyTypeNotFoundById = "Property Type '{0}' of an Email Notification Action is not found by Id in Blueprint.";
        private const string TemplateDataPropertyChangeActionPropertyTypeNotFoundById = "Property Type '{0}' of a Property Change Action is not found by Id in Blueprint.";
        private const string TemplateDataGenerateChildArtifactsActionArtifactTypeNotFoundById = "Artifact Type '{0}' of a Generate Child Artifacts Action is not found by Id in Blueprint.";
        private const string TemplateDataPropertyChangeActionValidValueNotFoundById = "One or more Valid Values in Value of Choice Property '{0}' in a Property Change Action are not found by Id.";
        private const string TemplateDataPropertyChangeActionUserNotFoundById = "One or more Users in Value of User Property '{0}' in a Property Change Action are not found by Id.";
        private const string TemplateDataPropertyChangeActionGroupNotFoundById = "One or more Groups in Value of User Property '{0}' in a Property Change Action are not found by Id.";
        private const string TemplateDataPropertyChangeActionNotChoicePropertyValidValuesNotApplicable = "<PropertyChangeAction> elements: The property and value types do not match. Please ensure <ValidValues> elements are used only with choice-type properties.";
        private const string TemplateDataPropertyChangeActionNotUserPropertyUsersGroupsNotApplicable = "<PropertyChangeAction> elements: The property and value types do not match. Please ensure <UsersGroups> elements are used only with user-type properties.";
        private const string TemplateDataPropertyChangeActionRequiredUserPropertyPropertyValueNotApplicable = "<PropertyChangeAction> elements: The property and value types do not match. Please ensure <PropertyValue> elements are not used with user-type properties.";
        private const string TemplateDataPropertyChangeActionChoicePropertyMultipleValidValuesNotAllowed = "<PropertyChangeAction> elements: Multiple <ValidValue> child elements are provided for a choice-type property that allows only one.";

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
                case WorkflowXmlValidationErrorCodes.WorkflowXmlSerializationError:
                    template = TemplateXmlWorkflowXmlSerializationError;
                    errParams = new object[] { (string)error.Element };
                    break;
                case WorkflowXmlValidationErrorCodes.WorkflowNameEmpty:
                    template = TemplateXmlWorkflowNameEmpty;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.WorkflowNameExceedsLimit24:
                    template = TemplateXmlWorkflowNameExceedsLimit24;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.WorkflowDescriptionExceedsLimit4000:
                    template = TemplateXmlWorkflowDescriptionExceedsLimit4000;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.WorkflowDoesNotContainAnyStates:
                    template = TemplateXmlWorkflowDoesNotContainAnyStates;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.StatesCountExceedsLimit100:
                    template = TemplateXmlStatesCountExceedsLimit100;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.StateNameEmpty:
                    template = TemplateXmlStateNameEmpty;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.StateNameExceedsLimit24:
                    template = TemplateXmlStateNameExceedsLimit24;
                    errParams = new object[] { ((IeState)error.Element).Name };
                    break;
                case WorkflowXmlValidationErrorCodes.StateNameNotUnique:
                    template = TemplateXmlStateNameNotUnique;
                    errParams = new object[] { ((IeState)error.Element).Name };
                    break;
                case WorkflowXmlValidationErrorCodes.NoInitialState:
                    template = TemplateXmlNoInitialState;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.InitialStateDoesNotHaveOutgoingTransition:
                    template = TemplateXmlInitialStateDoesNotHaveOutgoingTransition;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.MultipleInitialStates:
                    template = TemplateXmlMultipleInitialStates;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.StateDoesNotHaveAnyTransitions:
                    template = TemplateXmlStateDoesNotHaveAnyTransitions;
                    errParams = new object[] { (string)error.Element };
                    break;
                case WorkflowXmlValidationErrorCodes.TransitionEventNameEmpty:
                    template = TemplateXmlTransitionEventNameEmpty;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.TransitionEventNameExceedsLimit24:
                    template = TemplateXmlTransitionEventNameExceedsLimit24;
                    errParams = new object[] { ((IeTransitionEvent)error.Element).Name };
                    break;
                case WorkflowXmlValidationErrorCodes.PropertyChangeEventNameExceedsLimit24:
                    template = TemplateXmlPropertyChangeEventNameExceedsLimit24;
                    errParams = new object[] { ((IePropertyChangeEvent)error.Element).Name };
                    break;
                case WorkflowXmlValidationErrorCodes.NewArtifactEventNameExceedsLimit24:
                    template = TemplateXmlNewArtifactEventNameExceedsLimit24;
                    errParams = new object[] { ((IeNewArtifactEvent)error.Element).Name };
                    break;
                case WorkflowXmlValidationErrorCodes.StateWithDuplicateOutgoingTransitions:
                    template = TemplateXmlStateWithDuplicateOutgoingTransitions;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.TransitionCountOnStateExceedsLimit10:
                    template = TemplateXmlTransitionCountOnStateExceedsLimit10;
                    errParams = new object[] { (string)error.Element };
                    break;
                case WorkflowXmlValidationErrorCodes.TransitionStateNotFound:
                    template = TemplateXmlTransitionStateNotFound;
                    errParams = new object[] { ((IeTransitionEvent)error.Element).Name };
                    break;
                case WorkflowXmlValidationErrorCodes.TransitionStartStateNotSpecified:
                    template = TemplateXmlTransitionStartStateNotSpecified;
                    errParams = new object[] { ((IeTransitionEvent)error.Element).Name };
                    break;
                case WorkflowXmlValidationErrorCodes.TransitionEndStateNotSpecified:
                    template = TemplateXmlTransitionEndStateNotSpecified;
                    errParams = new object[] { ((IeTransitionEvent)error.Element).Name };
                    break;
                case WorkflowXmlValidationErrorCodes.TransitionFromAndToStatesSame:
                    template = TemplateXmlTransitionFromAndToStatesSame;
                    errParams = new object[] { ((IeTransitionEvent)error.Element).Name };
                    break;
                case WorkflowXmlValidationErrorCodes.TriggerCountOnEventExceedsLimit10:
                    template = TemplateXmlTriggerCountOnEventExceedsLimit10;
                    errParams = new object[] { ((IeEvent)error.Element).Name };
                    break;
                case WorkflowXmlValidationErrorCodes.PropertyChangeEventPropertyNotSpecified:
                    template = TemplateXmlPropertyChangEventPropertyNotSpecified;
                    errParams = new object[] { ((IePropertyChangeEvent)error.Element).Name };
                    break;
                case WorkflowXmlValidationErrorCodes.PropertyChangeEventDuplicateProperties:
                    template = TemplateXmlPropertyChangeEventDuplicateProperties;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.ProjectNoSpecified:
                    template = TemplateXmlProjectNoSpecified;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.AmbiguousProjectReference:
                    template = TemplateXmlAmbiguousProjectReference;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.InvalidId:
                    template = TemplateXmlInvalidId;
                    errParams = new object[] { };
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
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.PropertyChangeEventNoAnyTriggersNotSpecified:
                    template = TemplateXmlPropertyChangeEventNoAnyTriggersSpecified;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.NewArtifactEventNoAnyTriggersNotSpecified:
                    template = TemplateXmlNewArtifactEventNoAnyTriggersSpecified;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.PropertyChangeActionDuplicatePropertiesOnEvent:
                    template = TemplateXmlPropertyChangeActionDuplicatePropertiesOnEvent;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.ActionTriggerNotSpecified:
                    template = TemplateXmlActionTriggerNotSpecified;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.RecipientsEmailNotificationActionNotSpecified:
                    template = TemplateXmlRecipientsEmailNotificationActionNotSpecified;
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
                case WorkflowXmlValidationErrorCodes.MessageEmailNotificationActionNotSpecified:
                    template = TemplateXmlMessageEmailNotificationActionNotSpecified;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.PropertyNamePropertyChangeActionNotSpecified:
                    template = TemplateXmlPropertyNamePropertyChangeActionNotSpecified;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.PropertyValuePropertyChangeActionNotSpecified:
                    template = TemplateXmlPropertyValuePropertyChangeActionNotSpecified;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.AmbiguousPropertyValuePropertyChangeAction:
                    template = TemplateXmlAmbiguousPropertyValuePropertyChangeAction;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.PropertyChangeActionUserOrGroupNameNotSpecified:
                    template = TemplateXmlPropertyChangeActionUserOrGroupNameNotSpecified;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.AmbiguousGroupProjectReference:
                    template = TemplateXmlAmbiguousGroupProjectReference;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.ArtifactTypeGenerateChildrenActionNotSpecified:
                    template = TemplateXmlArtifactTypeGenerateChildrenActionNotSpecified;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.ChildCountGenerateChildrenActionNotSpecified:
                    template = TemplateXmlChildCountGenerateChildrenActionNotSpecified;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.ChildCountGenerateChildrenActionNotValid:
                    template = TemplateXmlChildCountGenerateChildrenActionNotValid;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.ArtifactTypeApplicableOnlyToGenerateChildArtifactAction:
                    template = TemplateXmlArtifactTypeApplicableOnlyToGenerateChildArtifactAction;
                    errParams = new object[] { };
                    break;
                case WorkflowXmlValidationErrorCodes.ChildCountApplicableOnlyToGenerateChildArtifactAction:
                    template = TemplateXmlChildCountApplicableOnlyToGenerateChildArtifactAction;
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
                    errParams = new object[] { };
                    break;
                case WorkflowDataValidationErrorCodes.ProjectByPathNotFound:
                    template = TemplateDataProjectByPathNotFound;
                    errParams = new object[] { (string)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.ProjectByIdNotFound:
                    template = TemplateDataProjectByIdNotFound;
                    errParams = new object[] { (int)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.ProjectDuplicate:
                    template = TemplateDataProjectIdDuplicate;
                    errParams = new object[] { };
                    break;
                case WorkflowDataValidationErrorCodes.InstanceGroupNotFoundByName:
                    template = TemplateDataInstanceGroupNotFoundByName;
                    errParams = new object[] { (string)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.StandardArtifactTypeNotFoundByName:
                    template = TemplateDataStandardArtifactTypeNotFoundByName;
                    errParams = new object[] { (string)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyNotFoundByName:
                    template = TemplateDataPropertyNotFoundByName;
                    errParams = new object[] { (string)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.GenerateChildArtifactsActionArtifactTypeNotFoundByName:
                    template = TemplateDataGenerateChildArtifactsActionArtifactTypeNotFoundByName;
                    errParams = new object[] { (string)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.EmailNotificationActionPropertyTypeNotFoundByName:
                    template = TemplateDataEmailNotificationActionPropertyTypeNotFoundByName;
                    errParams = new object[] { (string)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.EmailNotificationActionUnacceptablePropertyType:
                    template = TemplateDataEmailNotificationActionUnacceptablePropertyType;
                    errParams = new object[] { (string)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyChangeActionPropertyTypeNotFoundByName:
                    template = TemplateDataPropertyChangeActionPropertyTypeNotFoundByName;
                    errParams = new object[] { (string)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredPropertyValueEmpty:
                    template = TemplateDataPropertyChangeActionRequiredPropertyValueEmpty;
                    errParams = new object[] { (string)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyChangeActionUserNotFoundByName:
                    template = TemplateDataPropertyChangeActionUserNotFoundByName;
                    errParams = new object[] { (string)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyChangeActionGroupNotFoundByName:
                    template = TemplateDataPropertyChangeActionGroupNotFoundByName;
                    errParams = new object[] { (string)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyChangeActionChoiceValueSpecifiedAsNotValidated:
                    template = TemplateDataPropertyChangeActionChoiceValueSpecifiedAsNotValidated;
                    errParams = new object[] { (string)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyChangeActionValidValueNotFoundByValue:
                    template = TemplateDataPropertyChangeActionValidValueNotFoundByValue;
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
                    var workflow = (IeWorkflow)error.Element;
                    errParams = new object[] { workflow.Name, workflow.Id };
                    break;
                case WorkflowDataValidationErrorCodes.StateNotFoundByIdInCurrent:
                    template = TemplateDataStateNotFoundByIdInCurrent;
                    var state = (IeState)error.Element;
                    errParams = new object[] { state.Name, state.Id };
                    break;
                case WorkflowDataValidationErrorCodes.TransitionEventNotFoundByIdInCurrent:
                    template = TemplateDataTransitionEventNotFoundByIdInCurrent;
                    var te = (IeTransitionEvent)error.Element;
                    errParams = new object[] { te.Name, te.Id };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyChangeEventNotFoundByIdInCurrent:
                    template = TemplateDataPropertyChangeEventNotFoundByIdInCurrent;
                    var pce = (IePropertyChangeEvent)error.Element;
                    errParams = new object[] { pce.Name, pce.Id };
                    break;
                case WorkflowDataValidationErrorCodes.NewArtifactEventNotFoundByIdInCurrent:
                    template = TemplateDataNewArtifactEventNotFoundByIdInCurrent;
                    var nae = (IeNewArtifactEvent)error.Element;
                    errParams = new object[] { nae.Name, nae.Id };
                    break;
                case WorkflowDataValidationErrorCodes.ProjectArtifactTypeNotFoundByIdInCurrent:
                    template = TemplateDataProjectArtifactTypeNotFoundByIdInCurrent;
                    var tuple = (Tuple<IeProject, IeArtifactType>)error.Element;
                    errParams = new object[] { tuple.Item1.Id, tuple.Item2.Name, tuple.Item2.Id };
                    break;
                case WorkflowDataValidationErrorCodes.WorkflowNothingToUpdate:
                    template = TemplateDataWorkflowNothingToUpdate;
                    errParams = new object[] { };
                    break;
                case WorkflowDataValidationErrorCodes.StandardArtifactTypeNotFoundById:
                    template = TemplateDataStandardArtifactTypeNotFoundById;
                    errParams = new object[] { (int)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyNotFoundById:
                    template = TemplateDataPropertyNotFoundById;
                    errParams = new object[] { (int)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.InstanceGroupNotFoundById:
                    template = TemplateDataInstanceGroupNotFoundById;
                    errParams = new object[] { (int)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.EmailNotificationActionPropertyTypeNotFoundById:
                    template = TemplateDataEmailNotificationActionPropertyTypeNotFoundById;
                    errParams = new object[] { (int)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyChangeActionPropertyTypeNotFoundById:
                    template = TemplateDataPropertyChangeActionPropertyTypeNotFoundById;
                    errParams = new object[] { (int)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.GenerateChildArtifactsActionArtifactTypeNotFoundById:
                    template = TemplateDataGenerateChildArtifactsActionArtifactTypeNotFoundById;
                    errParams = new object[] { (int)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyChangeActionValidValueNotFoundById:
                    template = TemplateDataPropertyChangeActionValidValueNotFoundById;
                    errParams = new object[] { (int)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyChangeActionGroupNotFoundById:
                    template = TemplateDataPropertyChangeActionGroupNotFoundById;
                    errParams = new object[] { (int)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyChangeActionUserNotFoundById:
                    template = TemplateDataPropertyChangeActionUserNotFoundById;
                    errParams = new object[] { (int)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyChangeActionNotChoicePropertyValidValuesNotApplicable:
                    template = TemplateDataPropertyChangeActionNotChoicePropertyValidValuesNotApplicable;
                    errParams = new object[] { };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyChangeActionNotUserPropertyUsersGroupsNotApplicable:
                    template = TemplateDataPropertyChangeActionNotUserPropertyUsersGroupsNotApplicable;
                    errParams = new object[] { };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredUserPropertyPropertyValueNotApplicable:
                    template = TemplateDataPropertyChangeActionRequiredUserPropertyPropertyValueNotApplicable;
                    errParams = new object[] { };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyChangeActionChoicePropertyMultipleValidValuesNotAllowed:
                    template = TemplateDataPropertyChangeActionChoicePropertyMultipleValidValuesNotAllowed;
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