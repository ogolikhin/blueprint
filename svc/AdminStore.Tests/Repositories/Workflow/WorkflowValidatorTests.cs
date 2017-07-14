using System;
using System.Collections.Generic;
using System.Linq;
using AdminStore.Models.Workflow;
using AdminStore.Services.Workflow;
using Castle.Components.DictionaryAdapter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdminStore.Repositories.Workflow
{
    [TestClass]
    public class WorkflowValidatorTests
    {
        private IeWorkflow _workflow;

        [TestInitialize]
        public void InitValidWorkflow()
        {
            _workflow = new IeWorkflow
            {
                Name = "My Workflow",
                Description = "This is my workflow.",
                States = new List<IeState>(),
                ArtifactTypes = new List<IeArtifactType>(),
                TransitionEvents = new List<IeEvent>(),
                PropertyChangeEvents = new List<IeEvent>(),
                Projects = new List<IeProject>()
            };

            // States
            _workflow.States.Add(new IeState
            {
                Name = "New",
                Description = "Description of New state.",
                IsInitial = true
            });

            _workflow.States.Add(new IeState
            {
                Name = "Active",
                Description = "Description of Active state."
            });

            _workflow.States.Add(new IeState
            {
                Name = "Closed",
                Description = "Description of Closed state."
            });

            // Transitions
            _workflow.TransitionEvents.Add(new IeTransitionEvent
            {
                Name = "From New to Active",
                Description = "Description of From New to Active",
                FromState = "New",
                ToState = "Active",
                PermissionGroups = new List<IeGroup>
                {
                    new IeGroup { Name = "Authors"},
                    new IeGroup { Name = "Group 1" }
                }
            });

            _workflow.TransitionEvents.Add(new IeTransitionEvent
            {
                Name = "From Active to Close",
                Description = "Description of From New to Active",
                FromState = "Active",
                ToState = "Closed",
                PermissionGroups = new List<IeGroup>
                {
                    new IeGroup { Name = "Authors"},
                    new IeGroup { Name = "Group 2" }
                }
                
            });

            // Projects
            _workflow.Projects.Add(new IeProject
            {
                Id = 10
            });

            _workflow.Projects.Add(new IeProject
            {
                Path = @"Blueprint\folder\project1"
            });

            _workflow.Projects.Add(new IeProject
            {
                Id = 20,
                Path = @"Blueprint\folder\project2"
            });

            // Artifact Types
            _workflow.ArtifactTypes.Add(new IeArtifactType
            {
                Name = "Actor"
            });

            _workflow.ArtifactTypes.Add(new IeArtifactType
            {
                Name = "Use Case"
            });

            // Property Change Events
            //TODO this isn't valid - property change triggers can only have an email notification action
            _workflow.PropertyChangeEvents.Add(new IePropertyChangeEvent
            {
                Name = "Text Property Changed",
                Description = "Description of Text Property Changed",
                PropertyName = "My Text Property",
                Triggers = new EditableList<IeTrigger>
                {
                    new IeTrigger { 
                    Action = new IePropertyChangeAction
                    {
                        Name = "Text Property Change Action",
                        Description = "Description of Text Property Change Action",
                        PropertyName = "Text Property",
                        PropertyValue = "New text property value"
                    }},
                    new IeTrigger {
                    Action = new IePropertyChangeAction
                    {
                        Name = "User Property Change Action",
                        Description = "Description of User Property Change Action",
                        PropertyName = "User Property",
                        PropertyValue = "Administrators Group",
                        IsGroup = true
                    }},
                    new IeTrigger {
                    Action = new IeGenerateAction
                    {
                        GenerateActionType = GenerateActionTypes.Children,
                        Name = "Generate Children Action",
                        Description = "Description of Generate Children Action",
                        ArtifactType = "Business Rule",
                        ChildCount = 3
                    }},
                    new IeTrigger { 
                    Action = new IeGenerateAction
                    {
                        GenerateActionType = GenerateActionTypes.UserStories,
                        Name = "Generate User Stories Action",
                        Description = "Description of Generate User Stories Action"
                    }},
                    new IeTrigger {
                    Action = new IeGenerateAction
                    {
                        GenerateActionType = GenerateActionTypes.TestCases,
                        Name = "Generate Test Cases Action",
                        Description = "Description of Generate Test Cases Action"
                    }}
                }
            });

            _workflow.PropertyChangeEvents.Add(new IePropertyChangeEvent
            {
                Name = "Choice Property Changed",
                Description = "Description of Choice Property Changed",
                PropertyName = "My Choice Property"
            });

            _workflow.PropertyChangeEvents.Add(new IePropertyChangeEvent
            {
                Name = "User Property Changed",
                Description = "Description of User Property Changed",
                PropertyName = "My User Property"
            });

            //============= Temp ========================================
            //var xml = SerializationHelper.ToXml(_workflow);
            //var objModel = SerializationHelper.FromXml<IeWorkflow>(xml);
            //===========================================================
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Validate_WorkflowIsNull_ArgumentNullException()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();

            // Act
            workflowValidator.Validate(null);

            // Assert
        }

        [TestMethod]
        public void Validate_ValidWorkflow_Success()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();

            // Act
            var result = workflowValidator.Validate(_workflow);

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
            var result = workflowValidator.Validate(_workflow);

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
            var result = workflowValidator.Validate(_workflow);

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
            var result = workflowValidator.Validate(_workflow);

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
            var result = workflowValidator.Validate(_workflow);

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
            var result = workflowValidator.Validate(_workflow);

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
            var result = workflowValidator.Validate(_workflow);

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

            // Act
            var result = workflowValidator.Validate(_workflow);

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
            var result = workflowValidator.Validate(_workflow);

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
            var result = workflowValidator.Validate(_workflow);

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
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.StatesCountExceedsLimit100, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow, result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_StateNameEmpty_ReturnsStateNameEmptyError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.States.Add(new IeState { Name = "   "});
            _workflow.TransitionEvents.Add(new IeTransitionEvent
            {
                Name = "Transition",
                FromState = _workflow.States[_workflow.States.Count - 2].Name,
                ToState = _workflow.States[_workflow.States.Count - 1].Name
            });

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            var error = result.Errors.First(e => e.ErrorCode == WorkflowXmlValidationErrorCodes.StateNameEmpty);
            Assert.AreSame(_workflow.States.Last(), error.Element);
        }

        [TestMethod]
        public void Validate_StateNameNotUnique_ReturnsStateNameNotUniqueError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.States[1].Name = _workflow.States[0].Name;

            // Act
            var result = workflowValidator.Validate(_workflow);

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
            _workflow.States[0].Name = new string('a', 24);
            ((IeTransitionEvent) _workflow.TransitionEvents[0]).FromState = _workflow.States[0].Name;

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void Validate_StateNameExceedsLimit_ReturnsStateNameExceedsLimit24Error()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.States[0].Name = new string('a', 25);
            ((IeTransitionEvent)_workflow.TransitionEvents[0]).FromState = _workflow.States[0].Name;

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.StateNameExceedsLimit24, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.States[0], result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_StateDescriptionMax_Success()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.States[0].Description = new string('a', 4000);

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void Validate_StateDescriptionExceedsLimit_ReturnsStateDescriptionExceedsLimit4000Error()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.States[0].Description = new string('a', 4001);

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.StateDescriptionExceedsLimit4000, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.States[0], result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_TransitionNameEmpty_ReturnsTriggerNameEmptyError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents.Add(new IeTransitionEvent
            {
                FromState = _workflow.States[0].Name,
                ToState = _workflow.States[2].Name
            });

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.TransitionEventNameEmpty, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.TransitionEvents.Last(), result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_PropertyChangeTriggerNameEmpty_ReturnsTriggerNameEmptyError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.PropertyChangeEvents.Add(new IePropertyChangeEvent
            {
                PropertyName = "a"
            });

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.PropertyChangeEventNameEmpty, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.PropertyChangeEvents.Last(), result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_TransitionNameMax_Success()
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
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void Validate_PropertyChangeTriggerNameMax_Success()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.PropertyChangeEvents.Add(new IePropertyChangeEvent
            {
                Name = new string('a', 24),
                PropertyName = "a"
            });

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void Validate_TransitionNameExceedsLimit_ReturnsTriggerNameExceedsLimit24Error()
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
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.TransitionEventNameExceedsLimit24, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.TransitionEvents.Last(), result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_PropertyChangeTriggerNameExceedsLimit_ReturnsTriggerNameExceedsLimit24Error()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.PropertyChangeEvents.Add(new IePropertyChangeEvent
            {
                Name = new string('a', 25),
                PropertyName = "a"
            });

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.PropertyChangeEventNameExceedsLimit24, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.PropertyChangeEvents.Last(), result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_TransitionDescriptionMax_Success()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents.Add(new IeTransitionEvent
            {
                Name = "Transition",
                Description = new string('a', 4000),
                FromState = _workflow.States[0].Name,
                ToState = _workflow.States[2].Name
            });

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void Validate_PropertyChangeTriggerDescriptionMax_Success()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.PropertyChangeEvents.Add(new IePropertyChangeEvent
            {
                Name = "Transition",
                Description = new string('a', 4000),
                PropertyName = "a"
            });

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void Validate_TransitionDescriptionExceedsLimit_ReturnsTriggerDescriptionExceedsLimit4000Error()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents.Add(new IeTransitionEvent
            {
                Name = "Transition",
                Description = new string('a', 4001),
                FromState = _workflow.States[0].Name,
                ToState = _workflow.States[2].Name
            });

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.TransitionEventDescriptionExceedsLimit4000, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.TransitionEvents.Last(), result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_PropertyChangeTriggerDescriptionExceedsLimit_ReturnsTriggerDescriptionExceedsLimit4000Error()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.PropertyChangeEvents.Add(new IePropertyChangeEvent
            {
                Name = "Transition",
                Description = new string('a', 4001),
                PropertyName = "a"
            });

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.PropertyChangeEventDescriptionExceedsLimit4000, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.PropertyChangeEvents.Last(), result.Errors[0].Element);
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
            var result = workflowValidator.Validate(_workflow);

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
            var result = workflowValidator.Validate(_workflow);

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
            var result = workflowValidator.Validate(_workflow);

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
            var result = workflowValidator.Validate(_workflow);

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
            var result = workflowValidator.Validate(_workflow);

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
            var result = workflowValidator.Validate(_workflow);

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
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.TransitionCountOnStateExceedsLimit10, result.Errors[0].ErrorCode);
            Assert.AreEqual(_workflow.States[0].Name, result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_TransitionNameNotUniqueOnState_ReturnsTransitionNameNotUniqueOnStateError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.TransitionEvents.Add(new IeTransitionEvent
            {
                Name = _workflow.TransitionEvents[0].Name,
                FromState = _workflow.States[0].Name,
                ToState = _workflow.States[2].Name
            });

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.TransitionNameNotUniqueOnState, result.Errors[0].ErrorCode);
            Assert.AreEqual(_workflow.States[0].Name, result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_PropertyChangeTriggerPropertyNotSpecified_ReturnsPropertyChangeTriggerPropertyNotSpecifiedError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.PropertyChangeEvents.Add(new IePropertyChangeEvent
            {
                Name = "a"
            });

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.PropertyChangEventPropertyNotSpecified, result.Errors[0].ErrorCode);
            Assert.AreEqual(_workflow.PropertyChangeEvents.Last(), result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_ActionsCountOnTriggerMax_Success()
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
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsFalse(result.HasErrors);
        }

        [TestMethod]
        public void Validate_ActionsCountOnTriggerExceedsLimit_ReturnsActionsCountOnTriggerExceedsLimit10Error()
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
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.TriggerCountOnEventExceedsLimit10, result.Errors[0].ErrorCode);
            Assert.AreEqual(_workflow.TransitionEvents.Last(), result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_ProjectNoSpecified_ReturnsProjectNoSpecifiedError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.Projects[0].Id = null;

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.ProjectNoSpecified, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.Projects[0], result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_ProjectInvalidId_ReturnsProjectInvalidIdError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.Projects[2].Id = 0;

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.ProjectInvalidId, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.Projects[2], result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_ArtifactTypeNoSpecified_ReturnsArtifactTypeNoSpecifiedError()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.ArtifactTypes[0].Name = null;

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowXmlValidationErrorCodes.ArtifactTypeNoSpecified, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.ArtifactTypes[0], result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_MultipleErrors_ReturnsMultipleErrors()
        {
            // Arrange
            var workflowValidator = new WorkflowXmlValidator();
            _workflow.Name = null;
            _workflow.Projects[0].Id = -1;
            _workflow.ArtifactTypes[0].Name = null;

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(3, result.Errors.Count);
            var error1 = result.Errors.First(e => e.ErrorCode == WorkflowXmlValidationErrorCodes.WorkflowNameEmpty);
            Assert.AreSame(_workflow, error1.Element);
            var error2 = result.Errors.First(e => e.ErrorCode == WorkflowXmlValidationErrorCodes.ProjectInvalidId);
            Assert.AreSame(_workflow.Projects[0], error2.Element);
            var error3 = result.Errors.First(e => e.ErrorCode == WorkflowXmlValidationErrorCodes.ArtifactTypeNoSpecified);
            Assert.AreSame(_workflow.ArtifactTypes[0], error3.Element);
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
            var currentTransitionsCount = workflow.TransitionEvents.OfType<IeTransitionEvent>().Count(t => t.FromState == state.Name || t.ToState == state.Name);
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
            else if (anEvent is IeTransitionEvent)
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
