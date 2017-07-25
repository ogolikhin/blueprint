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
        
        private const string TemplateWorkflowImportFailedSingular = "There was an error uploading {0}. The supplied XML is not valid. Please edit your file and upload again.";
        private const string TemplateWorkflowImportFailedPlural = "There were errors uploading {0}. The supplied XML is not valid. Please edit your file and upload again.";

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
        private const string TemplateXmlProjectInvalidId = "The Project Id '{0}' is invalid. The Project Id must be greater than zero.";
        private const string TemplateXmlArtifactTypeNoSpecified = "One or more Artifact Types are not specified. A Artifact Types must be specified.";
        private const string TemplateXmlProjectsProvidedWithoutArifactTypes = "The Project or Projects are specified without any Artifact Types in the Workflow.";
        private const string TemplateXmlArtifactTypesProvidedWithoutProjects = "The Artifact Type or Artifact Types are specified without any Projects in the Workflow.";

        // Messages for the Data validation.
        private const string TemplateXmlWorkflowNameNotUnique = "A Workflow with Name '{0}' already exists. Workflows in Blueprint must have unique names.";
        private const string TemplateXmlProjectNotFound = "Project '{0}' is not found in Blueprint.";// TODO: Id vs. Path
        private const string TemplateXmlGroupsNotFound = "Group '{0}' is not found in Blueprint.";
        private const string TemplateXmlArtifactTypeNotFoundInProject = "Artifact Type '{0}' is not found in Project '{1}'."; // TODO: Test for Standard Types
        private const string TemplateXmlArtifactTypeAlreadyAssociatedWithWorkflow = "Artifact Type '{0}' is already is associated with a Workflow."; // TODO: Artifact Type in a Project
        private const string TemplateXmlPropertyNotFound = "Property '{0}' of a Property Change Event is not found in Blueprint.";

        #region Interface Implementation

        public string BuildTextXmlErrors(IEnumerable<WorkflowXmlValidationError> errors, string fileName)
        {
            var errorList = errors.ToList();
            var sb = new StringBuilder();
            AppendLine(sb, errorList.Count > 1 ? TemplateWorkflowImportFailedPlural : TemplateWorkflowImportFailedSingular, fileName);

            foreach (var error in errorList)
            {
                string template;
                var errParams = GetXmlErrorMessageTemaplateAndParams(error, out template);
                Append(sb, "\t- ");
                AppendLine(sb, template, errParams);
            }

            return sb.ToString();
        }

        public string BuildTextDataErrors(IEnumerable<WorkflowDataValidationError> errors, string fileName)
        {
            var errorList = errors.ToList();
            var sb = new StringBuilder();

            AppendLine(sb, errorList.Count > 1 ? TemplateWorkflowImportFailedPlural : TemplateWorkflowImportFailedSingular, fileName);

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

        private object[] GetXmlErrorMessageTemaplateAndParams(WorkflowXmlValidationError error, out string template)
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
                case WorkflowXmlValidationErrorCodes.PropertyChangEventPropertyNotSpecified:
                    template = TemplateXmlPropertyChangEventPropertyNotSpecified;
                    errParams = new object[] {((IePropertyChangeEvent) error.Element).Name};
                    break;
                case WorkflowXmlValidationErrorCodes.ProjectNoSpecified:
                    template = TemplateXmlProjectNoSpecified;
                    errParams = new object[] {};
                    break;
                case WorkflowXmlValidationErrorCodes.ProjectInvalidId:
                    template = TemplateXmlProjectInvalidId;
                    errParams = new object[] {((IeProject) error.Element).Id};
                    break;
                case WorkflowXmlValidationErrorCodes.ArtifactTypeNoSpecified:
                    template = TemplateXmlArtifactTypeNoSpecified;
                    errParams = new object[] {};
                    break;
                case WorkflowXmlValidationErrorCodes.ProjectsProvidedWithoutArifactTypes:
                    template = TemplateXmlProjectsProvidedWithoutArifactTypes;
                    errParams = new object[] {};
                    break;
                case WorkflowXmlValidationErrorCodes.ArtifactTypesProvidedWithoutProjects:
                    template = TemplateXmlArtifactTypesProvidedWithoutProjects;
                    errParams = new object[] {};
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(error.ErrorCode));
            }

            return errParams;
        }

        private object[] GetDataErrorMessageTemaplateAndParams(WorkflowDataValidationError error, out string template)
        {
            object[] errParams = null;
            template = null; // TODO: Remove

            switch (error.ErrorCode)
            {
                case WorkflowDataValidationErrorCodes.WorkflowNameNotUnique:
                    template = TemplateXmlWorkflowNameNotUnique;
                    errParams = new object[] { ((IeWorkflow)error.Element).Name };
                    break;
                case WorkflowDataValidationErrorCodes.ProjectNotFound:
                    template = TemplateXmlProjectNotFound;
                    errParams = new object[] { (string) error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.GroupsNotFound:
                    template = TemplateXmlGroupsNotFound;
                    errParams = new object[] { (string) error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.ArtifactTypeNotFoundInProject:
                    template = TemplateXmlArtifactTypeNotFoundInProject;
                    var t = (Tuple<string, int>)error.Element;
                    errParams = new object[] { t.Item1, t.Item2 };
                    break;
                case WorkflowDataValidationErrorCodes.ArtifactTypeAlreadyAssociatedWithWorkflow:
                    template = TemplateXmlArtifactTypeAlreadyAssociatedWithWorkflow;
                    errParams = new object[] { (string)error.Element };
                    break;
                case WorkflowDataValidationErrorCodes.PropertyNotFound:
                    template = TemplateXmlPropertyNotFound;
                    errParams = new object[] { (string)error.Element };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return errParams;
        }

        #endregion
    }
}