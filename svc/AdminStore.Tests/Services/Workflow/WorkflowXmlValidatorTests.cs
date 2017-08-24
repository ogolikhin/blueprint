using System;
using System.Collections.Generic;
using System.Linq;
using AdminStore.Models.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Services.Workflow
{
    [TestClass]
    public class WorkflowXmlValidatorTests
    {
        private IeWorkflow _workflow;

        [TestInitialize]
        public void InitValidWorkflow()
        {
            _workflow = WorkflowTestHelper.TestWorkflow;

            //============= Temp ========================================
            //var xml = SerializationHelper.ToXml(_workflow);
            //var objModel = SerializationHelper.FromXml<IeWorkflow>(xml);

            //var xmlTriggers = new XmlWorkflowEventTriggers();
            //xmlTriggers.Triggers.Add(new XmlWorkflowEventTrigger());
            //xmlTriggers.Triggers.Add(new XmlWorkflowEventTrigger());

            //var xml = SerializationHelper.ToXml(xmlTriggers);
            //var objModel = SerializationHelper.FromXml<XmlWorkflowEventTriggers>(xml);
            //===========================================================
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
        public void Validate_WorkflowNameEmpty_ReturnsWorkflowNameEmptyError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.Name = "   ";

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.WorkflowNameEmpty, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow, result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_WorkflowNameMax_Success()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.Name = new string('a', 24);

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void Validate_WorkflowNameExceedsLimit_ReturnsWorkflowNameExceedsLimit24Error()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.Name = new string('a', 25);

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.WorkflowNameExceedsLimit24, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow, result.Errors[0].Element);
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
            _workflow.States.Add(new IeState { Name = "   "});
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
                            GenerateActionType =GenerateActionTypes.UserStories
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
                            GenerateActionType =GenerateActionTypes.UserStories
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
        public void Validate_WorkflowEventNameNotUniqueInWorkflow_ReturnsWorkflowEventNameNotUniqueInWorkflowError()
        {
            // Arrange
            var duplicateName = "duplicate";
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents.Add(new IeTransitionEvent
            {
                Name = duplicateName,
                FromState = _workflow.States[0].Name,
                ToState = _workflow.States[2].Name
            });
            _workflow.TransitionEvents.Add(new IeTransitionEvent
            {
                Name = duplicateName,
                FromState = _workflow.States[2].Name,
                ToState = _workflow.States[0].Name
            });
            _workflow.PropertyChangeEvents.Add(new IePropertyChangeEvent
            {
                Name = duplicateName,
                PropertyName = "prop",
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
            _workflow.NewArtifactEvents.Add(new IeNewArtifactEvent
            {
                Name = duplicateName,
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
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.WorkflowEventNameNotUniqueInWorkflow, result.Errors[0].ErrorCode);
            Assert.AreEqual(_workflow.TransitionEvents.Last(), result.Errors[0].Element);
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
            ((IePropertyChangeAction) _workflow.NewArtifactEvents[1].Triggers[4].Action).UsersGroups[2].GroupProjectId = 0;

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
            var error1 = result.Errors.First(e => e.ErrorCode == WorkflowXmlValidationErrorCodes.WorkflowNameEmpty);
            Assert.AreSame(_workflow, error1.Element);
            var error2 = result.Errors.First(e => e.ErrorCode == WorkflowXmlValidationErrorCodes.InvalidId);
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
        public void Validate_RecipientsEmailNotificationActionNotSpecitied_ReturnsRecipientsEmailNotificationActionNotSpecitiedError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents?.ForEach(e => e.Triggers?.ForEach(t =>
            {
                if (t?.Action?.ActionType == ActionTypes.EmailNotification)
                {
                    ((IeEmailNotificationAction) t.Action).Emails = null;
                    ((IeEmailNotificationAction) t.Action).PropertyName = null;
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
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.RecipientsEmailNotificationActionNotSpecitied, result.Errors[0].ErrorCode);
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
                    ((IeEmailNotificationAction)t.Action).Emails = new List<string> {"user@comapany.com"};
                    ((IeEmailNotificationAction)t.Action).PropertyName = "a";
                }
            }));
            _workflow.PropertyChangeEvents?.ForEach(e => e.Triggers?.ForEach(t =>
            {
                if (t?.Action?.ActionType == ActionTypes.EmailNotification)
                {
                    ((IeEmailNotificationAction)t.Action).Emails = new List<string> { "user@comapany.com" }; ;
                    ((IeEmailNotificationAction)t.Action).PropertyName = "a";
                }
            }));
            _workflow.NewArtifactEvents?.ForEach(e => e.Triggers?.ForEach(t =>
            {
                if (t?.Action?.ActionType == ActionTypes.EmailNotification)
                {
                    ((IeEmailNotificationAction)t.Action).Emails = new List<string> { "user@comapany.com" };;
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
            var action = (IeEmailNotificationAction) _workflow.PropertyChangeEvents[0].Triggers[0].Action;
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
        public void Validate_MessageEmailNotificationActionNotSpecitied_ReturnsMessageEmailNotificationActionNotSpecitiedError()
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
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.MessageEmailNotificationActionNotSpecitied, result.Errors[0].ErrorCode);
            Assert.AreSame(action, result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_PropertyNamePropertyChangeActionNotSpecitied_ReturnsPropertyNamePropertyChangeActionNotSpecitiedError()
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
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.PropertyNamePropertyChangeActionNotSpecitied, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_AmbiguousGroupProjectReference_ReturnsAmbiguousGroupProjectReferenceError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            ((IePropertyChangeAction)_workflow.NewArtifactEvents[1].Triggers[4].Action).UsersGroups[3].GroupProjectId = 66;
            ((IePropertyChangeAction)_workflow.NewArtifactEvents[1].Triggers[4].Action).UsersGroups[4].GroupProjectId = 77;

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.AmbiguousGroupProjectReference, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_PropertyChangeActionValidValueValueNotSpecitied_ReturnsPropertyChangeActionValidValueValueNotSpecitiedError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            ((IePropertyChangeAction)_workflow.TransitionEvents[1].Triggers[3].Action).ValidValues[0].Value = "";
            ((IePropertyChangeAction)_workflow.TransitionEvents[1].Triggers[3].Action).ValidValues[1].Value = null;

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.PropertyChangeActionValidValueValueNotSpecitied, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_PropertyChangeActionUserOrGroupNameNotSpecitied_ReturnsPropertyChangeActionUserOrGroupNameNotSpecitiedError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            ((IePropertyChangeAction)_workflow.NewArtifactEvents[1].Triggers[4].Action).UsersGroups[3].Name = "";
            ((IePropertyChangeAction)_workflow.NewArtifactEvents[1].Triggers[4].Action).UsersGroups[4].Name = null;

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.PropertyChangeActionUserOrGroupNameNotSpecitied, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_PropertyValuePropertyChangeActionNotSpecitied_ReturnsPropertyValuePropertyChangeActionNotSpecitiedError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            ((IePropertyChangeAction) _workflow.TransitionEvents[1].Triggers[0].Action).PropertyValue = null;
            ((IePropertyChangeAction)_workflow.TransitionEvents[1].Triggers[1].Action).PropertyValue = null;

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.PropertyValuePropertyChangeActionNotSpecitied, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_AmbiguousPropertyValuePropertyChangeAction_ReturnsAmbiguousPropertyValuePropertyChangeActionError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            ((IePropertyChangeAction)_workflow.TransitionEvents[1].Triggers[0].Action).ValidValues = new List<IeValidValue>
            {
                new IeValidValue { Value = "a"}
            };
            ((IePropertyChangeAction)_workflow.TransitionEvents[1].Triggers[1].Action).UsersGroups = new List<IeUserGroup>
            {
                new IeUserGroup { Name = "user1"}
            };

            // Act
            var result = workflowValidator.ValidateXml(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.AmbiguousPropertyValuePropertyChangeAction, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_ArtifactTypeGenerateChildrenActionNotSpecitied_ReturnsArtifactTypeGenerateChildrenActionNotSpecitiedError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents?.ForEach(e => e.Triggers?.ForEach(t =>
            {
                if (t?.Action?.ActionType == ActionTypes.Generate)
                {
                    var action = (IeGenerateAction) t.Action;
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
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.ArtifactTypeGenerateChildrenActionNotSpecitied, result.Errors[0].ErrorCode);
        }

        [TestMethod]
        public void Validate_ChildCountGenerateChildrenActionNotSpecitied_ReturnsChildCountGenerateChildrenActionNotSpecitiedError()
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
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.ChildCountGenerateChildrenActionNotSpecitied, result.Errors[0].ErrorCode);
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
                    ((IeStateCondition) t.Condition).State = string.Empty;
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
            var stateCondition = (IeStateCondition) _workflow.PropertyChangeEvents[0].Triggers[0].Condition;
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

            for(var i = 0; i < toAddCount; i++)
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
                                ArtifactType = "Process",
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
