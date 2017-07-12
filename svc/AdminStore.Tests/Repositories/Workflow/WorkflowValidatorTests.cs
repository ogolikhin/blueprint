using System;
using System.Collections.Generic;
using System.Linq;
using AdminStore.Models.Workflow;
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
                Transitions = new List<IeTransitionTrigger>(),
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
            _workflow.Transitions.Add(new IeTransitionTrigger
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

            _workflow.Transitions.Add(new IeTransitionTrigger
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
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Validate_WorkflowIsNull_ArgumentNullException()
        {
            // Arrange
            var workflowValidator = new WorkflowValidator();

            // Act
            workflowValidator.Validate(null);

            // Assert
        }

        [TestMethod]
        public void Validate_ValidWorkflow_Success()
        {
            // Arrange
            var workflowValidator = new WorkflowValidator();

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
            var workflowValidator = new WorkflowValidator();
            _workflow.Name = "   ";

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowValidationErrorCodes.WorkflowNameEmpty, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow, result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_WorkflowNameMax_Success()
        {
            // Arrange
            var workflowValidator = new WorkflowValidator();
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
            var workflowValidator = new WorkflowValidator();
            _workflow.Name = new string('a', 25);

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowValidationErrorCodes.WorkflowNameExceedsLimit24, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow, result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_WorkflowDescriptionMax_Success()
        {
            // Arrange
            var workflowValidator = new WorkflowValidator();
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
            var workflowValidator = new WorkflowValidator();
            _workflow.Description = new string('a', 4001);

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowValidationErrorCodes.WorkflowDescriptionExceedsLimit4000, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow, result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_NoStates_ReturnsWorkflowDoesNotContainAnyStatesError()
        {
            // Arrange
            var workflowValidator = new WorkflowValidator();
            _workflow.States.Clear();
            _workflow.Transitions.Clear();

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowValidationErrorCodes.WorkflowDoesNotContainAnyStates, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow, result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_NoInitialState_ReturnsNoInitialStateError()
        {
            // Arrange
            var workflowValidator = new WorkflowValidator();
            _workflow.States[0].IsInitial = false;

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowValidationErrorCodes.NoInitialState, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow, result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_MultipleInitialStates_ReturnsMultipleInitialStatesError()
        {
            // Arrange
            var workflowValidator = new WorkflowValidator();
            _workflow.States[1].IsInitial = true;

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowValidationErrorCodes.MultipleInitialStates, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow, result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_WorkflowStatesMax_Success()
        {
            // Arrange
            var workflowValidator = new WorkflowValidator();
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
            var workflowValidator = new WorkflowValidator();
            AddStates(_workflow, 101);

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowValidationErrorCodes.StatesCountExceedsLimit100, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow, result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_StateNameEmpty_ReturnsStateNameEmptyError()
        {
            // Arrange
            var workflowValidator = new WorkflowValidator();
            _workflow.States.Add(new IeState { Name = "   "});
            _workflow.Transitions.Add(new IeTransitionTrigger
            {
                Name = "Transition",
                FromState = _workflow.States[_workflow.States.Count - 2].Name,
                ToState = _workflow.States[_workflow.States.Count - 1].Name
            });

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            var error = result.Errors.First(e => e.ErrorCode == WorkflowValidationErrorCodes.StateNameEmpty);
            Assert.AreSame(_workflow.States.Last(), error.Element);
        }

        [TestMethod]
        public void Validate_StateNameNotUnique_ReturnsStateNameNotUniqueError()
        {
            // Arrange
            var workflowValidator = new WorkflowValidator();
            _workflow.States[1].Name = _workflow.States[0].Name;

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            var error = result.Errors.First(e => e.ErrorCode == WorkflowValidationErrorCodes.StateNameNotUnique);
            Assert.AreSame(_workflow.States[1], error.Element);
        }

        [TestMethod]
        public void Validate_StateNameMax_Success()
        {
            // Arrange
            var workflowValidator = new WorkflowValidator();
            _workflow.States[0].Name = new string('a', 24);
            _workflow.Transitions[0].FromState = _workflow.States[0].Name;

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
            var workflowValidator = new WorkflowValidator();
            _workflow.States[0].Name = new string('a', 25);
            _workflow.Transitions[0].FromState = _workflow.States[0].Name;

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowValidationErrorCodes.StateNameExceedsLimit24, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.States[0], result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_StateDescriptionMax_Success()
        {
            // Arrange
            var workflowValidator = new WorkflowValidator();
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
            var workflowValidator = new WorkflowValidator();
            _workflow.States[0].Description = new string('a', 4001);

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowValidationErrorCodes.StateDescriptionExceedsLimit4000, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.States[0], result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_TransitionNameEmpty_ReturnsTransitionNameEmptyError()
        {
            // Arrange
            var workflowValidator = new WorkflowValidator();
            _workflow.Transitions.Add(new IeTransitionTrigger
            {
                FromState = _workflow.States[0].Name,
                ToState = _workflow.States[2].Name
            });

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowValidationErrorCodes.TransitionNameEmpty, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.Transitions.Last(), result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_TransitionNameMax_Success()
        {
            // Arrange
            var workflowValidator = new WorkflowValidator();
            _workflow.Transitions.Add(new IeTransitionTrigger
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
        public void Validate_TransitionNameExceedsLimit_ReturnsTransitionNameExceedsLimit24Error()
        {
            // Arrange
            var workflowValidator = new WorkflowValidator();
            _workflow.Transitions.Add(new IeTransitionTrigger
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
            Assert.AreEqual(WorkflowValidationErrorCodes.TransitionNameExceedsLimit24, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.Transitions.Last(), result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_TransitionDescriptionMax_Success()
        {
            // Arrange
            var workflowValidator = new WorkflowValidator();
            _workflow.Transitions.Add(new IeTransitionTrigger
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
        public void Validate_TransitionDescriptionExceedsLimit_ReturnsTransitionDescriptionExceedsLimit4000Error()
        {
            // Arrange
            var workflowValidator = new WorkflowValidator();
            _workflow.Transitions.Add(new IeTransitionTrigger
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
            Assert.AreEqual(WorkflowValidationErrorCodes.TransitionDescriptionExceedsLimit4000, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.Transitions.Last(), result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_TransitionStartStateNotSpecified_ReturnsTransitionStartStateNotSpecifiedError()
        {
            // Arrange
            var workflowValidator = new WorkflowValidator();
            _workflow.Transitions.Add(new IeTransitionTrigger
            {
                Name = "Transition",
                ToState = _workflow.States[2].Name
            });

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowValidationErrorCodes.TransitionStartStateNotSpecified, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.Transitions.Last(), result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_TransitionEndStateNotSpecified_ReturnsTransitionEndStateNotSpecifiedError()
        {
            // Arrange
            var workflowValidator = new WorkflowValidator();
            _workflow.Transitions.Add(new IeTransitionTrigger
            {
                Name = "Transition",
                FromState = _workflow.States[0].Name
            });

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowValidationErrorCodes.TransitionEndStateNotSpecified, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.Transitions.Last(), result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_TransitionFromAndToStatesSame_ReturnsTransitionFromAndToStatesSameError()
        {
            // Arrange
            var workflowValidator = new WorkflowValidator();
            _workflow.Transitions.Add(new IeTransitionTrigger
            {
                Name = "Transition",
                FromState = _workflow.States[0].Name,
                ToState = _workflow.States[0].Name
            });

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            var error = result.Errors.First(e => e.ErrorCode == WorkflowValidationErrorCodes.TransitionFromAndToStatesSame);
            Assert.AreSame(_workflow.Transitions.Last(), error.Element);
        }

        [TestMethod]
        public void Validate_TransitionStateNotFound_ReturnsTransitionStateNotFoundError()
        {
            // Arrange
            var workflowValidator = new WorkflowValidator();
            _workflow.Transitions.Add(new IeTransitionTrigger
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
            Assert.AreEqual(WorkflowValidationErrorCodes.TransitionStateNotFound, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.Transitions.Last(), result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_StateDoesNotHaveAnyTransitions_ReturnsStateDoesNotHaveAnyTransitionsError()
        {
            // Arrange
            var workflowValidator = new WorkflowValidator();
            _workflow.States.Add(new IeState
            {
                Name = "State"
            });

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowValidationErrorCodes.StateDoesNotHaveAnyTransitions, result.Errors[0].ErrorCode);
            Assert.AreEqual(_workflow.States.Last().Name, result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_TransitionCountOnStateMax_Success()
        {
            // Arrange
            var workflowValidator = new WorkflowValidator();
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
            var workflowValidator = new WorkflowValidator();
            AddTransitionsToState(_workflow, _workflow.States[0], 11);

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowValidationErrorCodes.TransitionCountOnStateExceedsLimit10, result.Errors[0].ErrorCode);
            Assert.AreEqual(_workflow.States[0].Name, result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_TransitionNameNotUniqueOnState_ReturnsTransitionNameNotUniqueOnStateError()
        {
            // Arrange
            var workflowValidator = new WorkflowValidator();
            _workflow.Transitions.Add(new IeTransitionTrigger
            {
                Name = _workflow.Transitions[0].Name,
                FromState = _workflow.States[0].Name,
                ToState = _workflow.States[2].Name
            });

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowValidationErrorCodes.TransitionNameNotUniqueOnState, result.Errors[0].ErrorCode);
            Assert.AreEqual(_workflow.States[0].Name, result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_ProjectNoSpecified_ReturnsProjectNoSpecifiedError()
        {
            // Arrange
            var workflowValidator = new WorkflowValidator();
            _workflow.Projects[0].Id = null;

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowValidationErrorCodes.ProjectNoSpecified, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.Projects[0], result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_ProjectInvalidId_ReturnsProjectInvalidIdError()
        {
            // Arrange
            var workflowValidator = new WorkflowValidator();
            _workflow.Projects[2].Id = 0;

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowValidationErrorCodes.ProjectInvalidId, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.Projects[2], result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_ArtifactTypeNoSpecified_ReturnsArtifactTypeNoSpecifiedError()
        {
            // Arrange
            var workflowValidator = new WorkflowValidator();
            _workflow.ArtifactTypes[0].Name = null;

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowValidationErrorCodes.ArtifactTypeNoSpecified, result.Errors[0].ErrorCode);
            Assert.AreSame(_workflow.ArtifactTypes[0], result.Errors[0].Element);
        }

        [TestMethod]
        public void Validate_MultipleErrors_ReturnsMultipleErrors()
        {
            // Arrange
            var workflowValidator = new WorkflowValidator();
            _workflow.Name = null;
            _workflow.Projects[0].Id = -1;
            _workflow.ArtifactTypes[0].Name = null;

            // Act
            var result = workflowValidator.Validate(_workflow);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(3, result.Errors.Count);
            var error1 = result.Errors.First(e => e.ErrorCode == WorkflowValidationErrorCodes.WorkflowNameEmpty);
            Assert.AreSame(_workflow, error1.Element);
            var error2 = result.Errors.First(e => e.ErrorCode == WorkflowValidationErrorCodes.ProjectInvalidId);
            Assert.AreSame(_workflow.Projects[0], error2.Element);
            var error3 = result.Errors.First(e => e.ErrorCode == WorkflowValidationErrorCodes.ArtifactTypeNoSpecified);
            Assert.AreSame(_workflow.ArtifactTypes[0], error3.Element);
        }

        #region Private methods

        private static void AddStates(IeWorkflow workflow, int statesCount)
        {
            while (workflow.States.Count < statesCount)
            {
                var count = workflow.States.Count;
                workflow.States.Add(new IeState { Name = "State " + count });
                workflow.Transitions.Add(new IeTransitionTrigger
                {
                    Name = "Transition " + count,
                    FromState = workflow.States[count - 1].Name,
                    ToState = workflow.States[count].Name
                });
            }
        }

        private static void AddTransitionsToState(IeWorkflow workflow, IeState state, int transitionsCount)
        {
            var currentTransitionsCount = workflow.Transitions.Count(t => t.FromState == state.Name || t.ToState == state.Name);
            var toAddCount = transitionsCount - currentTransitionsCount;
            if (toAddCount < 1)
            {
                return;
            }

            for(var i = 0; i < toAddCount; i++)
            {
                workflow.States.Add(new IeState { Name = "State " + i });
                workflow.Transitions.Add(new IeTransitionTrigger
                {
                    Name = "Transition " + i,
                    FromState = state.Name,
                    ToState = workflow.States.Last().Name
                });
            }
        }

        #endregion
    }
}
