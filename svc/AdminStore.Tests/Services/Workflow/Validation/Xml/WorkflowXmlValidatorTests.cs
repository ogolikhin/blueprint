﻿using System;
using System.Collections.Generic;
using System.Linq;
using AdminStore.Models.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Services.Workflow.Validation.Xml
{
    [TestClass]
    public class WorkflowXmlValidatorTests
    {
        private IeWorkflow _workflow;

        [TestInitialize]
        public void InitValidWorkflow()
        {
            _workflow = WorkflowTestHelper.TestWorkflow;

            // ============= Temp ========================================
            // var xml = SerializationHelper.ToXml(_workflow);
            // var objModel = SerializationHelper.FromXml<IeWorkflow>(xml);

            // var xmlTriggers = new XmlWorkflowEventTriggers();
            // xmlTriggers.Triggers.Add(new XmlWorkflowEventTrigger());
            // xmlTriggers.Triggers.Add(new XmlWorkflowEventTrigger());

            // var xml = SerializationHelper.ToXml(xmlTriggers);
            // var objModel = SerializationHelper.FromXml<XmlWorkflowEventTriggers>(xml);
            // ===========================================================
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Validate_WorkflowIsNull_ArgumentNullException()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();

            // Act
            workflowValidator.ValidateXml(null);

            // Assert
        }

        [TestMethod]
        public void Validate_ValidWorkflow_Success()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void Validate_WorkflowNameIsNull_ReturnsWorkflowNameMissingOrInvalidError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.Name = null;

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.WorkflowNameMissingOrInvalid, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow, result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_WorkflowNameEmpty_ReturnsWorkflowNameMissingOrInvalidError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.Name = "   ";

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.WorkflowNameMissingOrInvalid, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow, result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_WorkflowNameLessThanLimit_ReturnsWorkflowNameMissingOrInvalidError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.Name = new string('a', WorkflowXmlValidator.MinWorkflowNameLength - 1);

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.WorkflowNameMissingOrInvalid, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow, result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_WorkflowNameExceedsLimit_ReturnsWorkflowNameMissingOrInvalidError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.Name = new string('a', WorkflowXmlValidator.MaxWorkflowNameLength + 1);

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.WorkflowNameMissingOrInvalid, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow, result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_WorkflowNameMin_Success()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.Name = new string('a', WorkflowXmlValidator.MinWorkflowNameLength);

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void Validate_WorkflowNameMax_Success()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.Name = new string('a', WorkflowXmlValidator.MaxWorkflowNameLength);

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void Validate_WorkflowDescriptionMax_Success()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.Description = new string('a', 4000);

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void Validate_WorkflowDescriptionExceedsLimit_ReturnsWorkflowDescriptionExceedsLimit4000Error()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.Description = new string('a', 4001);

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.WorkflowDescriptionExceedsLimit4000, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow, result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_NoStates_ReturnsWorkflowDoesNotContainAnyStatesError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.States.Clear();
            _workflow.PropertyChangeEvents.Clear();
            _workflow.TransitionEvents.Clear();

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.WorkflowDoesNotContainAnyStates, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow, result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_NoInitialState_ReturnsNoInitialStateError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.States[0].IsInitial = false;
            _workflow.TransitionEvents.Add(new IeTransitionEvent
            {
                Name = "From Active to New",
                FromState = "Active",
                ToState = "New"
            });

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.NoInitialState, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow, result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_MultipleInitialStates_ReturnsMultipleInitialStatesError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.States[1].IsInitial = true;

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.MultipleInitialStates, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow, result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_WorkflowStatesMax_Success()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            AddStates(_workflow, 100);

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void Validate_WorkflowStatesExceedLimit_ReturnsStatesCountExceedsLimit100Error()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            AddStates(_workflow, 101);

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.StatesCountExceedsLimit100, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow, result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_StateNameEmpty_ReturnsOnlyOneStateNameEmptyError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.States.Add(new IeState { Name = "   " });
            _workflow.TransitionEvents.Add(new IeTransitionEvent
            {
                Name = "Transition 1",
                FromState = _workflow.States[_workflow.States.Count - 2].Name,
                ToState = _workflow.States[_workflow.States.Count - 1].Name
            });

            _workflow.States.Add(new IeState { Name = "   " });
            _workflow.TransitionEvents.Add(new IeTransitionEvent
            {
                Name = "Transition 2",
                FromState = _workflow.States[_workflow.States.Count - 2].Name,
                ToState = _workflow.States[_workflow.States.Count - 1].Name
            });

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            var error = result.Errors.First(e => e.ErrorCode == WorkflowXmlValidationErrorCodes.StateNameEmpty);
            Assert.AreSame(_workflow.States.ElementAt(_workflow.States.Count - 2), error.Element);
        }

        [TestMethod]
        public void Validate_StateNameNotUnique_ReturnsStateNameNotUniqueError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.States[1].Name = _workflow.States[0].Name;

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            var error = result.Errors.First(e => e.ErrorCode == WorkflowXmlValidationErrorCodes.StateNameNotUnique);
            Assert.AreSame(_workflow.States[1], error.Element);
        }

        [TestMethod]
        public void Validate_StateNameMax_Success()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents.RemoveAt(_workflow.TransitionEvents.Count - 1);
            _workflow.States[0].Name = new string('a', 24);
            _workflow.TransitionEvents[0].FromState = _workflow.States[0].Name;

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void Validate_StateNameExceedsLimit_ReturnsStateNameExceedsLimit24Error()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents.RemoveAt(_workflow.TransitionEvents.Count - 1);
            _workflow.States[0].Name = new string('a', 25);
            _workflow.TransitionEvents[0].FromState = _workflow.States[0].Name;

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.StateNameExceedsLimit24, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.States[0], result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_TransitionEventNameMax_Success()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents.Add(new IeTransitionEvent
            {
                Name = new string('a', 24),
                FromState = _workflow.States[0].Name,
                ToState = _workflow.States[2].Name
            });

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void Validate_PropertyChangeEventNameMax_Success()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.PropertyChangeEvents.Add(new IePropertyChangeEvent
            {
                Name = new string('a', 24),
                PropertyName = "a",
                Triggers = new List<IeTrigger>
                {
                    new IeTrigger
                    {
                        Action = new IeEmailNotificationAction
                        {
                            PropertyName = "Assignee",
                            Message = "some message"
                        }
                    }
                }
            });

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void Validate_NewArtifactEventNameMax_Success()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.NewArtifactEvents.Add(new IeNewArtifactEvent
            {
                Name = new string('a', 24),
                Triggers = new List<IeTrigger>
                {
                    new IeTrigger
                    {
                        Action = new IeGenerateAction
                        {
                            GenerateActionType = GenerateActionTypes.UserStories
                        }
                    }
                }
            });

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void Validate_TransitionEventNameExceedsLimit_ReturnsTransitionEventNameExceedsLimit24Error()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents.Add(new IeTransitionEvent
            {
                Name = new string('a', 25),
                FromState = _workflow.States[0].Name,
                ToState = _workflow.States[2].Name
            });

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.TransitionEventNameExceedsLimit24, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.TransitionEvents.Last(), result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_PropertyChangeEventNameExceedsLimit_ReturnsPropertyChangeEventNameExceedsLimit24Error()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.PropertyChangeEvents.Add(new IePropertyChangeEvent
            {
                Name = new string('a', 25),
                PropertyName = "a",
                Triggers = new List<IeTrigger>
                {
                    new IeTrigger
                    {
                        Action = new IeEmailNotificationAction
                        {
                            PropertyName = "Assignee",
                            Message = "some message"
                        }
                    }
                }
            });

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.PropertyChangeEventNameExceedsLimit24, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.PropertyChangeEvents.Last(), result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_NewArtifactEventNameExceedsLimit_ReturnsNewArtifactEventNameExceedsLimit24Error()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.NewArtifactEvents.Add(new IeNewArtifactEvent
            {
                Name = new string('a', 25),
                Triggers = new List<IeTrigger>
                {
                    new IeTrigger
                    {
                        Action = new IeGenerateAction
                        {
                            GenerateActionType = GenerateActionTypes.UserStories
                        }
                    }
                }
            });

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.NewArtifactEventNameExceedsLimit24, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.NewArtifactEvents.Last(), result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_TransitionStartStateNotSpecified_ReturnsTransitionStartStateNotSpecifiedError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents.Add(new IeTransitionEvent
            {
                Name = "Transition",
                ToState = _workflow.States[2].Name
            });

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.TransitionStartStateNotSpecified, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.TransitionEvents.Last(), result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_TransitionEndStateNotSpecified_ReturnsTransitionEndStateNotSpecifiedError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents.Add(new IeTransitionEvent
            {
                Name = "Transition",
                FromState = _workflow.States[0].Name
            });

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.TransitionEndStateNotSpecified, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.TransitionEvents.Last(), result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_TransitionFromAndToStatesSame_ReturnsTransitionFromAndToStatesSameError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents.Add(new IeTransitionEvent
            {
                Name = "Transition",
                FromState = _workflow.States[0].Name,
                ToState = _workflow.States[0].Name
            });

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            var error = result.Errors.First(e => e.ErrorCode == WorkflowXmlValidationErrorCodes.TransitionFromAndToStatesSame);
            Assert.AreSame(_workflow.TransitionEvents.Last(), error.Element);
        }

        [TestMethod]
        public void Validate_TransitionStateNotFound_ReturnsTransitionStateNotFoundError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents.Add(new IeTransitionEvent
            {
                Name = "Transition",
                FromState = _workflow.States[0].Name,
                ToState = "State Not Found"
            });

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.TransitionStateNotFound, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.TransitionEvents.Last(), result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_StateDoesNotHaveAnyTransitions_ReturnsStateDoesNotHaveAnyTransitionsError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.States.Add(new IeState
            {
                Name = "State"
            });

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.StateDoesNotHaveAnyTransitions, result.Errors[0].ErrorCode);
            Assert.AreEqual(_workflow.States.Last().Name, result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_TransitionEventNameEmpty_ReturnsOnlyOneTransitionEventNameEmptyError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents.Add(new IeTransitionEvent
            {
                FromState = _workflow.States[0].Name,
                ToState = _workflow.States[2].Name
            });

            _workflow.TransitionEvents.Add(new IeTransitionEvent
            {
                FromState = _workflow.States[2].Name,
                ToState = _workflow.States[0].Name
            });

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.TransitionEventNameEmpty, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_TransitionCountOnStateMax_Success()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            AddTransitionsToState(_workflow, _workflow.States[0], 10);

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void Validate_TransitionCountOnStateExceedsLimit_ReturnsTransitionCountOnStateExceedsLimit10Error()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            AddTransitionsToState(_workflow, _workflow.States[0], 11);

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.TransitionCountOnStateExceedsLimit10, result.Errors[0].ErrorCode);
            Assert.AreEqual(_workflow.States[0].Name, result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_StateWithDuplicateOutgoingTransitions_ReturnsStateWithDuplicateOutgoingTransitionsError()
        {
            // Arrange
            const string duplicateName = "duplicate";
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents[0].Name = duplicateName;
            _workflow.TransitionEvents.Add(new IeTransitionEvent
            {
                Name = duplicateName,
                FromState = _workflow.States[0].Name,
                ToState = _workflow.States[2].Name
            });

            _workflow.States.Add(new IeState { Name = "state" });
            _workflow.TransitionEvents.Add(new IeTransitionEvent
            {
                Name = duplicateName,
                FromState = _workflow.States[0].Name,
                ToState = _workflow.States[3].Name
            });

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.StateWithDuplicateOutgoingTransitions, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_PropertyChangeTriggerPropertyNotSpecified_ReturnsPropertyChangeTriggerPropertyNotSpecifiedError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.PropertyChangeEvents.Add(new IePropertyChangeEvent
            {
                Name = "a",
                Triggers = new List<IeTrigger>
                {
                    new IeTrigger
                    {
                        Action = new IeEmailNotificationAction
                        {
                            PropertyName = "Assignee",
                            Message = "some message"
                        }
                    }
                }
            });

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.PropertyChangeEventPropertyNotSpecified, result.Errors[0].ErrorCode);
            Assert.AreEqual(_workflow.PropertyChangeEvents.Last(), result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_PropertyChangeEventDuplicateProperties_ReturnsPropertyChangeEventDuplicatePropertiesError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.PropertyChangeEvents[1].PropertyName = _workflow.PropertyChangeEvents[0].PropertyName;
            _workflow.PropertyChangeEvents.Add(_workflow.PropertyChangeEvents[0]);

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.PropertyChangeEventDuplicateProperties, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_TriggerCountOnEventMax_Success()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents.Add(new IeTransitionEvent
            {
                Name = "t",
                FromState = _workflow.States[0].Name,
                ToState = _workflow.States[2].Name
            });
            AddActionsToEvent(_workflow.TransitionEvents.Last(), 10);

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsFalse(result.HasErrors);
        }

        [TestMethod]
        public void Validate_TriggerCountOnEventExceedsLimit10_ReturnsTriggerCountOnEventExceedsLimit10Error()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents.Add(new IeTransitionEvent
            {
                Name = "t",
                FromState = _workflow.States[0].Name,
                ToState = _workflow.States[2].Name
            });
            AddActionsToEvent(_workflow.TransitionEvents.Last(), 11);

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.TriggerCountOnEventExceedsLimit10, result.Errors[0].ErrorCode);
            Assert.AreEqual(_workflow.TransitionEvents.Last(), result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_PropertyChangeEventNoAnyTriggersSpecified_ReturnsPropertyChangeEventNoAnyTriggersNotSpecifiedError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.PropertyChangeEvents[0].Triggers.Clear();

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.PropertyChangeEventNoAnyTriggersNotSpecified, result.Errors[0].ErrorCode);
            Assert.AreEqual(_workflow.PropertyChangeEvents[0], result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_NewArtifactEventNoAnyTriggersSpecified_NewArtifactEventNoAnyTriggersNotSpecifiedError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.NewArtifactEvents[0].Triggers = null;

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.NewArtifactEventNoAnyTriggersNotSpecified, result.Errors[0].ErrorCode);
            Assert.AreEqual(_workflow.NewArtifactEvents[0], result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_PropertyChangeActionDuplicatePropertiesOnEvent_Transition_PropertyChangeActionDuplicatePropertiesOnEventError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents[1].Triggers.Add(_workflow.TransitionEvents[1].Triggers[0]);

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.PropertyChangeActionDuplicatePropertiesOnEvent, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_PropertyChangeActionDuplicatePropertiesOnEvent_NewArtifact_PropertyChangeActionDuplicatePropertiesOnEventError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.NewArtifactEvents[1].Triggers.Add(_workflow.NewArtifactEvents[1].Triggers[0]);

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.PropertyChangeActionDuplicatePropertiesOnEvent, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_ActionTriggerNotSpecified_ActionTriggerNotSpecifiedError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents[0].Triggers[0].Action = null;
            _workflow.PropertyChangeEvents[0].Triggers[0].Action = null;
            _workflow.NewArtifactEvents[0].Triggers[0].Action = null;

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.ActionTriggerNotSpecified, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_ProjectNoSpecified_ReturnsProjectNoSpecifiedError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.Projects[0].Id = null;

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.ProjectNoSpecified, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.Projects[0], result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_AmbiguousProjectReference_ReturnsAmbiguousProjectReferenceError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.Projects[0].Id = 11;
            _workflow.Projects[1].Id = 22;

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.AmbiguousProjectReference, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_ProjectInvalidId_ReturnsProjectInvalidIdError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.Projects[0].Id = 0;

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.InvalidId, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_ProjectInvalidId_ReturnsGroupProjectInvalidIdError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            ((IePropertyChangeAction)_workflow.NewArtifactEvents[1].Triggers[4].Action).UsersGroups.UsersGroups[2].GroupProjectId = 0;

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.InvalidId, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_ProjectInvalidId_ReturnsProjectDuplicateIdError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.Projects[1].Id = _workflow.Projects[0].Id;
            _workflow.Projects[1].Path = null;

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.ProjectDuplicateId, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_ProjectInvalidId_ReturnsProjectDuplicatePathError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.Projects[0].Id = null;
            _workflow.Projects[0].Path = "path";
            _workflow.Projects[1].Id = null;
            _workflow.Projects[1].Path = "path";

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.ProjectDuplicatePath, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_ProjectDoesNotHaveAnyArtfactTypes_ReturnsProjectDoesNotHaveAnyArtfactTypesError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.Projects[0].ArtifactTypes.Clear();

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.ProjectDoesNotHaveAnyArtfactTypes, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_ArtifactTypeNoSpecified_ReturnsArtifactTypeNoSpecifiedError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.Projects[0].ArtifactTypes[0].Name = null;

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.ArtifactTypeNoSpecified, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.Projects[0].ArtifactTypes[0], result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_MultipleErrors_ReturnsMultipleErrors()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.Name = null;
            _workflow.Projects[0].Id = -1;
            _workflow.Projects[0].ArtifactTypes[0].Name = null;

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(3, result.Errors.Count);
            var error1 = result.Errors.First(e => e.ErrorCode == WorkflowXmlValidationErrorCodes.WorkflowNameMissingOrInvalid);
            Assert.AreSame(_workflow, error1.Element);
            var error3 = result.Errors.First(e => e.ErrorCode == WorkflowXmlValidationErrorCodes.ArtifactTypeNoSpecified);
            Assert.AreSame(_workflow.Projects[0].ArtifactTypes[0], error3.Element);
        }

        [TestMethod]
        public void Validate_InitialStateDoesNotHaveOutgoingTransition_ReturnsInitialStateDoesNotHaveOutgoingTransitionError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents.RemoveAll(e => e.FromState == "New" && e.ToState == "Active");
            _workflow.TransitionEvents.Add(new IeTransitionEvent
            {
                Name = "From Active to New",
                FromState = "Active",
                ToState = "New",
            });
            _workflow.TransitionEvents.Add(new IeTransitionEvent
            {
                Name = "From Closed to Active",
                FromState = "Closed",
                ToState = "Active",
            });
            _workflow.Projects.Clear();

            // Act
            var result = workflowValidator.ValidateXml(_workflow);


            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.InitialStateDoesNotHaveOutgoingTransition, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.States[0], result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_RecipientsEmailNotificationActionNotSpecified_ReturnsRecipientsEmailNotificationActionNotSpecifiedError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents?.ForEach(e => e.Triggers?.ForEach(t =>
            {
                if (t?.Action?.ActionType == ActionTypes.EmailNotification)
                {
                    ((IeEmailNotificationAction)t.Action).Emails = null;
                    ((IeEmailNotificationAction)t.Action).PropertyName = null;
                }
            }));
            _workflow.PropertyChangeEvents?.ForEach(e => e.Triggers?.ForEach(t =>
            {
                if (t?.Action?.ActionType == ActionTypes.EmailNotification)
                {
                    ((IeEmailNotificationAction)t.Action).Emails = null;
                    ((IeEmailNotificationAction)t.Action).PropertyName = null;
                }
            }));
            _workflow.NewArtifactEvents?.ForEach(e => e.Triggers?.ForEach(t =>
            {
                if (t?.Action?.ActionType == ActionTypes.EmailNotification)
                {
                    ((IeEmailNotificationAction)t.Action).Emails = null;
                    ((IeEmailNotificationAction)t.Action).PropertyName = null;
                }
            }));

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.RecipientsEmailNotificationActionNotSpecified, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_AmbiguousRecipientsSourcesEmailNotificationAction_ReturnsAmbiguousRecipientsSourcesEmailNotificationActionError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents?.ForEach(e => e.Triggers?.ForEach(t =>
            {
                if (t?.Action?.ActionType == ActionTypes.EmailNotification)
                {
                    ((IeEmailNotificationAction)t.Action).Emails = new List<string> { "user@comapany.com" };
                    ((IeEmailNotificationAction)t.Action).PropertyName = "a";
                }
            }));
            _workflow.PropertyChangeEvents?.ForEach(e => e.Triggers?.ForEach(t =>
            {
                if (t?.Action?.ActionType == ActionTypes.EmailNotification)
                {
                    ((IeEmailNotificationAction)t.Action).Emails = new List<string> { "user@comapany.com" };
                    ((IeEmailNotificationAction)t.Action).PropertyName = "a";
                }
            }));
            _workflow.NewArtifactEvents?.ForEach(e => e.Triggers?.ForEach(t =>
            {
                if (t?.Action?.ActionType == ActionTypes.EmailNotification)
                {
                    ((IeEmailNotificationAction)t.Action).Emails = new List<string> { "user@comapany.com" };
                    ((IeEmailNotificationAction)t.Action).PropertyName = "a";
                }
            }));

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.AmbiguousRecipientsSourcesEmailNotificationAction, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_EmailInvalidEmailNotificationAction_ReturnsEmailInvalidEmailNotificationActionError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            var action = (IeEmailNotificationAction)_workflow.PropertyChangeEvents[0].Triggers[0].Action;
            action.Emails[0] = "a";

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.EmailInvalidEmailNotificationAction, result.Errors[0].ErrorCode);
            Assert.AreSame(action.Emails[0], result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_MessageEmailNotificationActionNotSpecified_ReturnsMessageEmailNotificationActionNotSpecifiedError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            var action = (IeEmailNotificationAction)_workflow.PropertyChangeEvents[0].Triggers[0].Action;
            action.Message = " ";

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.MessageEmailNotificationActionNotSpecified, result.Errors[0].ErrorCode);
            Assert.AreSame(action, result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_PropertyNamePropertyChangeActionNotSpecified_ReturnsPropertyNamePropertyChangeActionNotSpecifiedError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            ((IePropertyChangeAction)_workflow.TransitionEvents[1].Triggers[0].Action).PropertyName = null;
            ((IePropertyChangeAction)_workflow.TransitionEvents[1].Triggers[1].Action).PropertyName = "";

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.PropertyNamePropertyChangeActionNotSpecified, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_AmbiguousGroupProjectReference_ReturnsAmbiguousGroupProjectReferenceError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            ((IePropertyChangeAction)_workflow.NewArtifactEvents[1].Triggers[4].Action).UsersGroups.UsersGroups[3].GroupProjectId = 66;
            ((IePropertyChangeAction)_workflow.NewArtifactEvents[1].Triggers[4].Action).UsersGroups.UsersGroups[4].GroupProjectId = 77;

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.AmbiguousGroupProjectReference, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_PropertyChangeActionUserOrGroupNameNotSpecified_ReturnsPropertyChangeActionUserOrGroupNameNotSpecifiedError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            ((IePropertyChangeAction)_workflow.NewArtifactEvents[1].Triggers[4].Action).UsersGroups.UsersGroups[3].Name = "";
            ((IePropertyChangeAction)_workflow.NewArtifactEvents[1].Triggers[4].Action).UsersGroups.UsersGroups[4].Name = null;

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.PropertyChangeActionUserOrGroupNameNotSpecified, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_PropertyValuePropertyChangeActionNotSpecified_ReturnsPropertyValuePropertyChangeActionNotSpecifiedError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            ((IePropertyChangeAction)_workflow.TransitionEvents[1].Triggers[0].Action).PropertyValue = null;
            ((IePropertyChangeAction)_workflow.TransitionEvents[1].Triggers[1].Action).PropertyValue = null;

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.PropertyValuePropertyChangeActionNotSpecified, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_AmbiguousPropertyValuePropertyChangeAction_ReturnsAmbiguousPropertyValuePropertyChangeActionError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            ((IePropertyChangeAction)_workflow.TransitionEvents[1].Triggers[0].Action).ValidValues = new List<IeValidValue>
            {
                new IeValidValue { Value = "a" }
            };
            ((IePropertyChangeAction)_workflow.TransitionEvents[1].Triggers[1].Action).UsersGroups = new IeUsersGroups
            {
                UsersGroups = new List<IeUserGroup>()
            };

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.AmbiguousPropertyValuePropertyChangeAction, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_PropertyValuePropertyChangeAction_IncludeCurrentUser_Success()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            var pcAction = (IePropertyChangeAction)_workflow.TransitionEvents[1].Triggers[1].Action;
            pcAction.PropertyValue = null;
            pcAction.ValidValues = null;
            pcAction.UsersGroups = new IeUsersGroups { IncludeCurrentUser = true };

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsFalse(result.HasErrors);
        }

        [TestMethod]
        public void Validate_PropertyValuePropertyChangeAction_UsersGroupsAndIncludeCurrentUser_Success()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            var pcAction = (IePropertyChangeAction)_workflow.TransitionEvents[1].Triggers[1].Action;
            pcAction.PropertyValue = null;
            pcAction.ValidValues = null;
            pcAction.UsersGroups = new IeUsersGroups
            {
                UsersGroups = new List<IeUserGroup>
                {
                    new IeUserGroup { Name = "user1" }
                },
                IncludeCurrentUser = true
            };

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsFalse(result.HasErrors);
        }

        [TestMethod]
        public void Validate_ArtifactTypeGenerateChildrenActionNotSpecified_ReturnsArtifactTypeGenerateChildrenActionNotSpecifiedError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents?.ForEach(e => e.Triggers?.ForEach(t =>
            {
                if (t?.Action?.ActionType == ActionTypes.Generate)
                {
                    var action = (IeGenerateAction)t.Action;
                    if (action.GenerateActionType == GenerateActionTypes.Children)
                    {
                        action.ArtifactType = null;
                        action.ChildCount = 1;
                    }
                }
            }));
            _workflow.NewArtifactEvents?.ForEach(e => e.Triggers?.ForEach(t =>
            {
                if (t?.Action?.ActionType == ActionTypes.Generate)
                {
                    var action = (IeGenerateAction)t.Action;
                    if (action.GenerateActionType == GenerateActionTypes.Children)
                    {
                        action.ArtifactType = " ";
                        action.ChildCount = 2;
                    }
                }
            }));

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.ArtifactTypeGenerateChildrenActionNotSpecified, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_ChildCountGenerateChildrenActionNotSpecified_ReturnsChildCountGenerateChildrenActionNotSpecifiedError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents.ForEach(e => e.Triggers?.ForEach(t =>
            {
                if (t?.Action?.ActionType == ActionTypes.Generate)
                {
                    var action = (IeGenerateAction)t.Action;
                    if (action.GenerateActionType == GenerateActionTypes.Children)
                    {
                        action.ArtifactType = "a";
                        action.ChildCount = null;
                    }
                }
            }));
            _workflow.NewArtifactEvents?.ForEach(e => e.Triggers?.ForEach(t =>
            {
                if (t?.Action?.ActionType == ActionTypes.Generate)
                {
                    var action = (IeGenerateAction)t.Action;
                    if (action.GenerateActionType == GenerateActionTypes.Children)
                    {
                        action.ArtifactType = "a";
                        action.ChildCount = null;
                    }
                }
            }));

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.ChildCountGenerateChildrenActionNotSpecified, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_ChildCountGenerateChildrenActionNotValid_0_ReturnsChildCountGenerateChildrenActionNotValidError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents.ForEach(e => e.Triggers?.ForEach(t =>
            {
                if (t?.Action?.ActionType == ActionTypes.Generate)
                {
                    var action = (IeGenerateAction)t.Action;
                    if (action.GenerateActionType == GenerateActionTypes.Children)
                    {
                        action.ArtifactType = "a";
                        action.ChildCount = 0;
                    }
                }
            }));
            _workflow.NewArtifactEvents?.ForEach(e => e.Triggers?.ForEach(t =>
            {
                if (t?.Action?.ActionType == ActionTypes.Generate)
                {
                    var action = (IeGenerateAction)t.Action;
                    if (action.GenerateActionType == GenerateActionTypes.Children)
                    {
                        action.ArtifactType = "a";
                        action.ChildCount = 0;
                    }
                }
            }));

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.ChildCountGenerateChildrenActionNotValid, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_ChildCountGenerateChildrenActionNotValid_11_ReturnsChildCountGenerateChildrenActionNotValidError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents.ForEach(e => e.Triggers?.ForEach(t =>
            {
                if (t?.Action?.ActionType == ActionTypes.Generate)
                {
                    var action = (IeGenerateAction)t.Action;
                    if (action.GenerateActionType == GenerateActionTypes.Children)
                    {
                        action.ArtifactType = "a";
                        action.ChildCount = 11;
                    }
                }
            }));
            _workflow.NewArtifactEvents?.ForEach(e => e.Triggers?.ForEach(t =>
            {
                if (t?.Action?.ActionType == ActionTypes.Generate)
                {
                    var action = (IeGenerateAction)t.Action;
                    if (action.GenerateActionType == GenerateActionTypes.Children)
                    {
                        action.ArtifactType = "a";
                        action.ChildCount = 11;
                    }
                }
            }));

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.ChildCountGenerateChildrenActionNotValid, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_ArtifactTypeApplicableOnlyToGenerateChildArtifactAction_GenerateUserStories_ReturnsArtifactTypeApplicableOnlyToGenerateChildArtifactActionError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents?.ForEach(e => e.Triggers?.ForEach(t =>
            {
                if (t?.Action?.ActionType == ActionTypes.Generate)
                {
                    var action = (IeGenerateAction)t.Action;
                    if (action.GenerateActionType == GenerateActionTypes.UserStories)
                    {
                        action.ArtifactType = "a";
                    }
                }
            }));
            _workflow.NewArtifactEvents?.ForEach(e => e.Triggers?.ForEach(t =>
            {
                if (t?.Action?.ActionType == ActionTypes.Generate)
                {
                    var action = (IeGenerateAction)t.Action;
                    if (action.GenerateActionType == GenerateActionTypes.UserStories)
                    {
                        action.ArtifactType = "a";
                    }
                }
            }));

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.ArtifactTypeApplicableOnlyToGenerateChildArtifactAction, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_ArtifactTypeApplicableOnlyToGenerateChildArtifactAction_GenerateTestCases_ReturnsArtifactTypeApplicableOnlyToGenerateChildArtifactActionError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents?.ForEach(e => e.Triggers?.ForEach(t =>
            {
                if (t?.Action?.ActionType == ActionTypes.Generate)
                {
                    var action = (IeGenerateAction)t.Action;
                    if (action.GenerateActionType == GenerateActionTypes.TestCases)
                    {
                        action.ArtifactType = "a";
                    }
                }
            }));
            _workflow.NewArtifactEvents?.ForEach(e => e.Triggers?.ForEach(t =>
            {
                if (t?.Action?.ActionType == ActionTypes.Generate)
                {
                    var action = (IeGenerateAction)t.Action;
                    if (action.GenerateActionType == GenerateActionTypes.TestCases)
                    {
                        action.ArtifactType = "a";
                    }
                }
            }));

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.ArtifactTypeApplicableOnlyToGenerateChildArtifactAction, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_ChildCountApplicableOnlyToGenerateChildArtifactAction_GenerateChildCountApplicableOnlyToGenerateChildArtifactActionError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents?.ForEach(e => e.Triggers?.ForEach(t =>
            {
                if (t?.Action?.ActionType == ActionTypes.Generate)
                {
                    var action = (IeGenerateAction)t.Action;
                    if (action.GenerateActionType == GenerateActionTypes.UserStories)
                    {
                        action.ChildCount = 2;
                    }
                }
            }));
            _workflow.NewArtifactEvents?.ForEach(e => e.Triggers?.ForEach(t =>
            {
                if (t?.Action?.ActionType == ActionTypes.Generate)
                {
                    var action = (IeGenerateAction)t.Action;
                    if (action.GenerateActionType == GenerateActionTypes.UserStories)
                    {
                        action.ChildCount = 1;
                    }
                }
            }));

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.ChildCountApplicableOnlyToGenerateChildArtifactAction, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_ChildCountApplicableOnlyToGenerateChildArtifactAction_GenerateTestCases_ReturnsChildCountApplicableOnlyToGenerateChildArtifactActionError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents?.ForEach(e => e.Triggers?.ForEach(t =>
            {
                if (t?.Action?.ActionType == ActionTypes.Generate)
                {
                    var action = (IeGenerateAction)t.Action;
                    if (action.GenerateActionType == GenerateActionTypes.TestCases)
                    {
                        action.ChildCount = 2;
                    }
                }
            }));
            _workflow.NewArtifactEvents?.ForEach(e => e.Triggers?.ForEach(t =>
            {
                if (t?.Action?.ActionType == ActionTypes.Generate)
                {
                    var action = (IeGenerateAction)t.Action;
                    if (action.GenerateActionType == GenerateActionTypes.TestCases)
                    {
                        action.ChildCount = 1;
                    }
                }
            }));

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.ChildCountApplicableOnlyToGenerateChildArtifactAction, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_StateConditionNotOnTriggerOfPropertyChangeEvent_ReturnsStateConditionNotOnTriggerOfPropertyChangeEventError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents.ForEach(e => e.Triggers?.ForEach(t =>
            {
                t.Condition = new IeStateCondition { State = "Active" };
            }));
            _workflow.NewArtifactEvents.ForEach(e => e.Triggers?.ForEach(t =>
            {
                t.Condition = new IeStateCondition { State = "Active" };
            }));

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.StateConditionNotOnTriggerOfPropertyChangeEvent, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_StateStateConditionNotSpecified_ReturnsStateStateConditionNotSpecifiedError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.PropertyChangeEvents.ForEach(e => e?.Triggers?.ForEach(t =>
            {
                if (t?.Condition?.ConditionType == ConditionTypes.State)
                {
                    ((IeStateCondition)t.Condition).State = string.Empty;
                }
            }));

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.StateStateConditionNotSpecified, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_StateStateConditionNotFound_ReturnsStateStateConditionNotFoundError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            var stateCondition = (IeStateCondition)_workflow.PropertyChangeEvents[0].Triggers[0].Condition;
            stateCondition.State = "Missing State";

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.StateStateConditionNotFound, result.Errors[0].ErrorCode);
            Assert.AreSame(stateCondition.State, result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_PropertyChangeEventActionNotSupported_ReturnsPropertyChangeEventActionNotSupportedError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.PropertyChangeEvents[0].Triggers.Add(new IeTrigger
            {
                Action = new IeGenerateAction
                {
                    GenerateActionType = GenerateActionTypes.TestCases

                }
            });

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.PropertyChangeEventActionNotSupported, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.PropertyChangeEvents[0], result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_SDuplicateArtifactTypesInProject_ReturnsDuplicateArtifactTypesInProjectError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.Projects[0].ArtifactTypes.Add(new IeArtifactType
            {
                Name = _workflow.Projects[0].ArtifactTypes[0].Name
            });
            _workflow.Projects[1].ArtifactTypes.Add(new IeArtifactType
            {
                Name = _workflow.Projects[1].ArtifactTypes[0].Name
            });

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.DuplicateArtifactTypesInProject, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_DisconnectedWorkflow_ReturnsStatesNotConnectedToInitialStateError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.States.Add(new IeState { Name = "Investigate" });
            _workflow.States.Add(new IeState { Name = "On Hold" });
            _workflow.TransitionEvents.Add(new IeTransitionEvent { Name = "Investigate to On Hold", FromState = "Investigate", ToState = "On Hold" });
            _workflow.TransitionEvents.Add(new IeTransitionEvent { Name = "On Hold to Investigate", FromState = "On Hold", ToState = "Investigate" });

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.StatesNotConnectedToInitialState, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_WebhooksNoUrl_ReturnsWebhookActionUrlNotSpecifiedError()
        {
            // Arrange
            var webhookAction = new IeWebhookAction
            {
                BasicAuth = new IeBasicAuth
                {
                    Username = "user",
                    Password = "pass"
                },
                HttpHeaders = new List<string>
                {
                    "key: value"
                },
                IgnoreInvalidSSLCertificate = true,
                Name = "name",
                Signature = new IeSignature
                {
                    SecretToken = "jkafas241jalf"
                }
            };
            var triggers = new List<IeTrigger>
            {
                new IeTrigger {
                    Action = webhookAction
                }
            };
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.NewArtifactEvents.Add(new IeNewArtifactEvent
            {
                Name = "new event",
                Triggers = triggers
            });

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.WebhookActionUrlNotSpecified, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_WebhooksInvalidUrl_ReturnsWebhookActionUrlInvalidError()
        {
            // Arrange
            var webhookAction = new IeWebhookAction
            {
                 Url = "http:/www,example.com",
                BasicAuth = new IeBasicAuth
                {
                    Username = "user",
                    Password = "pass"
                },
                HttpHeaders = new List<string>
                {
                    "key: value"
                },
                IgnoreInvalidSSLCertificate = true,
                Name = "name",
                Signature = new IeSignature
                {
                    SecretToken = "jkafas241jalf"
                }
            };
            var triggers = new List<IeTrigger>
            {
                new IeTrigger {
                    Action = webhookAction
                }
            };
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.NewArtifactEvents.Add(new IeNewArtifactEvent
            {
                Name = "new event",
                Triggers = triggers
            });

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.WebhookActionUrlInvalid, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_WebhooksInvalidHeader_ReturnsWebhookActionHttpHeaderInvalidError()
        {
            // Arrange
            var webhookAction = new IeWebhookAction
            {
                Url = "http://www.example.com",
                BasicAuth = new IeBasicAuth
                {
                    Username = "user",
                    Password = "pass"
                },
                HttpHeaders = new List<string>
                {
                    "key: value",
                    "key; value"
                },
                IgnoreInvalidSSLCertificate = true,
                Name = "name",
                Signature = new IeSignature
                {
                    SecretToken = "jkafas241jalf"
                }
            };
            var triggers = new List<IeTrigger>
            {
                new IeTrigger {
                    Action = webhookAction
                }
            };
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.NewArtifactEvents.Add(new IeNewArtifactEvent
            {
                Name = "new event",
                Triggers = triggers
            });

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.WebhookActionHttpHeaderInvalid, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_WebhooksInvalidBasicAuth_ReturnsWebhookActionBasicAuthInvalidError()
        {
            // Arrange
            var webhookAction = new IeWebhookAction
            {
                Url = "http://www.example.com",
                BasicAuth = new IeBasicAuth
                {
                    Username = "user",
                    Password = ""
                },
                HttpHeaders = new List<string>
                {
                    "key: value"
                },
                IgnoreInvalidSSLCertificate = true,
                Name = "name",
                Signature = new IeSignature
                {
                    SecretToken = "jkafas241jalf"
                }
            };
            var triggers = new List<IeTrigger>
            {
                new IeTrigger {
                    Action = webhookAction
                }
            };
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.NewArtifactEvents.Add(new IeNewArtifactEvent
            {
                Name = "new event",
                Triggers = triggers
            });

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.WebhookActionBasicAuthInvalid, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_WebhooksMissingSecretToken_ReturnsWebhookActionSignatureSecretTokenEmptyError()
        {
            // Arrange
            var webhookAction = new IeWebhookAction
            {
                Url = "http://www.example.com",
                BasicAuth = new IeBasicAuth
                {
                    Username = "user",
                    Password = "pass"
                },
                HttpHeaders = new List<string>
                {
                    "key: value"
                },
                IgnoreInvalidSSLCertificate = true,
                Name = "name",
                Signature = new IeSignature
                {
                    SecretToken = ""
                }
            };
            var triggers = new List<IeTrigger>
            {
                new IeTrigger {
                    Action = webhookAction
                }
            };
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.NewArtifactEvents.Add(new IeNewArtifactEvent
            {
                Name = "new event",
                Triggers = triggers
            });

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.WebhookActionSignatureSecretTokenEmpty, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_WebhooksInvalidSignatureAlgorithm_ReturnsWebhookActionSignatureAlgorithmInvalidError()
        {
            // Arrange
            var webhookAction = new IeWebhookAction
            {
                Url = "http://www.example.com",
                BasicAuth = new IeBasicAuth
                {
                    Username = "user",
                    Password = "pass"
                },
                HttpHeaders = new List<string>
                {
                    "key: value"
                },
                IgnoreInvalidSSLCertificate = true,
                Name = "name",
                Signature = new IeSignature
                {
                    Algorithm = "gsdfg",
                    SecretToken = "jkafas241jalf"
                }
            };
            var triggers = new List<IeTrigger>
            {
                new IeTrigger {
                    Action = webhookAction
                }
            };
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.NewArtifactEvents.Add(new IeNewArtifactEvent
            {
                Name = "new event",
                Triggers = triggers
            });

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.WebhookActionSignatureAlgorithmInvalid, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_WebhooksNoAuthMethod_ReturnsWebhookActionNoAuthenticationMethodProvidedError()
        {
            // Arrange
            var webhookAction = new IeWebhookAction
            {
                Url = "http://www.example.com",
                IgnoreInvalidSSLCertificate = true,
                Name = "name"
            };
            var triggers = new List<IeTrigger>
            {
                new IeTrigger {
                    Action = webhookAction
                }
            };
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.NewArtifactEvents.Add(new IeNewArtifactEvent
            {
                Name = "new event",
                Triggers = triggers
            });

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.WebhookActionNoAuthenticationMethodProvided, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_WebhooksInPropertyChangeTrigger_ReturnsPropertyChangeEventActionNotSupportedError()
        {
            // Arrange
            var webhookAction = new IeWebhookAction
            {
                Url = "http://www.example.com",
                BasicAuth = new IeBasicAuth
                {
                    Username = "user",
                    Password = "pass"
                },
                HttpHeaders = new List<string>
                {
                    "key: value"
                },
                IgnoreInvalidSSLCertificate = true,
                Name = "name",
                Signature = new IeSignature
                {
                    SecretToken = "jkafas241jalf"
                }
            };
            var triggers = new List<IeTrigger>
            {
                new IeTrigger {
                    Action = webhookAction
                }
            };
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.PropertyChangeEvents.Add(new IePropertyChangeEvent
            {
                Name = "new event",
                Triggers = triggers,
                PropertyName = "property name"
            });

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.PropertyChangeEventActionNotSupported, result.Errors[0].ErrorCode);
        }

        #region Private methods

        private static void AddStates(IeWorkflow workflow, int statesCount)
        {
            while (workflow.States.Count < statesCount)
            {
                var count = workflow.States.Count;
                workflow.States.Add(new IeState { Name = "State " + count });
                workflow.TransitionEvents.Add(new IeTransitionEvent
                {
                    Name = "Transition " + count,
                    FromState = workflow.States[count - 1].Name,
                    ToState = workflow.States[count].Name
                });
            }
        }

        private static void AddTransitionsToState(IeWorkflow workflow, IeState state, int transitionsCount)
        {
            var currentTransitionsCount = workflow.TransitionEvents.Count(t => t.FromState == state.Name || t.ToState == state.Name);
            var toAddCount = transitionsCount - currentTransitionsCount;
            if (toAddCount < 1)
            {
                return;
            }

            for (var i = 0; i < toAddCount; i++)
            {
                workflow.States.Add(new IeState { Name = "State " + i });
                workflow.TransitionEvents.Add(new IeTransitionEvent
                {
                    Name = "Transition " + i,
                    FromState = state.Name,
                    ToState = workflow.States.Last().Name
                });
            }
        }

        private static void AddActionsToEvent(IeEvent anEvent, int actionsCount)
        {
            if (anEvent is IePropertyChangeEvent)
            {
                if (anEvent.Triggers == null)
                {
                    anEvent.Triggers = new List<IeTrigger>();
                    for (var i = 0; i < actionsCount; i++)
                    {
                        anEvent.Triggers.Add(new IeTrigger
                        {
                            Action = new IeEmailNotificationAction
                            {
                                Name = "Action " + i
                            }
                        });
                    }
                }
            }
            else if (anEvent is IeTransitionEvent || anEvent is IeNewArtifactEvent)
            {
                if (anEvent.Triggers == null)
                {
                    anEvent.Triggers = new List<IeTrigger>();
                    for (var i = 0; i < actionsCount; i++)
                    {
                        anEvent.Triggers.Add(new IeTrigger
                        {
                            Action = new IeGenerateAction
                            {
                                Name = "Action " + i,
                                GenerateActionType = GenerateActionTypes.UserStories
                            }
                        });
                    }
                }
            }
        }

        #endregion
    }
}
