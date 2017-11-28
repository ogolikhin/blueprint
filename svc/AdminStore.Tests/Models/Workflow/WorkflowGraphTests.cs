using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdminStore.Models.Workflow
{
    [TestClass]
    public class WorkflowGraphTests
    {
        [TestMethod]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "AdminStore.Models.Workflow.WorkflowGraphTests")]
        public void Construction_NullWorkflow_ThrowsArgumentNullException()
        {
            // Arrange
            IeWorkflow workflow = null;

            try
            {
                // Act
                var graph = new WorkflowGraph(workflow);
            }
            catch (ArgumentNullException ex)
            {
                // Assert
                Assert.AreEqual(nameof(workflow), ex.ParamName);
                return;
            }

            Assert.Fail("Expected ArgumentNullException to have been thrown.");
        }

        [TestMethod]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "AdminStore.Models.Workflow.WorkflowGraphTests")]
        public void Construction_WorkflowWithoutStates_ThrowsArgumentNullException()
        {
            // Arrange
            var workflow = new IeWorkflow();

            try
            {
                // Act
                var graph = new WorkflowGraph(workflow);
            }
            catch (ArgumentNullException ex)
            {
                // Assert
                Assert.AreEqual(nameof(workflow.States), ex.ParamName);
                return;
            }

            Assert.Fail("Expected ArgumentNullException to have been thrown.");
        }

        [TestMethod]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "AdminStore.Models.Workflow.WorkflowGraphTests")]
        public void Construction_WorkflowWithoutTransitions_ThrowsArgumentNullException()
        {
            // Arrange
            var workflow = new IeWorkflow
            {
                States = new List<IeState>()
            };

            try
            {
                // Act
                var graph = new WorkflowGraph(workflow);
            }
            catch (ArgumentNullException ex)
            {
                // Assert
                Assert.AreEqual(nameof(workflow.TransitionEvents), ex.ParamName);
                return;
            }

            Assert.Fail("Expected ArgumentNullException to have been thrown.");
        }

        [TestMethod]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "AdminStore.Models.Workflow.WorkflowGraphTests")]
        public void Construction_WorkflowStateWithoutName_ThrowsArgumentException()
        {
            // Arrange
            var workflow = new IeWorkflow
            {
                States = new List<IeState> { new IeState() }
            };

            try
            {
                // Act
                var graph = new WorkflowGraph(workflow);
            }
            catch (ArgumentException ex)
            {
                // Assert
                Assert.IsNotNull(ex);
                return;
            }

            Assert.Fail("Expected ArgumentException to have been thrown.");
        }

        [TestMethod]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "AdminStore.Models.Workflow.WorkflowGraphTests")]
        public void Construction_WorkflowTransitionWithoutFromState_ThrowsArgumentException()
        {
            // Arrange
            var workflow = new IeWorkflow
            {
                States = new List<IeState>(),
                TransitionEvents = new List<IeTransitionEvent> { new IeTransitionEvent() }
            };

            try
            {
                // Act
                var graph = new WorkflowGraph(workflow);
            }
            catch (ArgumentException ex)
            {
                // Assert
                Assert.IsNotNull(ex);
                return;
            }

            Assert.Fail("Expected ArgumentException to have been thrown.");
        }

        [TestMethod]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "AdminStore.Models.Workflow.WorkflowGraphTests")]
        public void Construction_WorkflowTransitionWithoutToState_ThrowsArgumentException()
        {
            // Arrange
            var workflow = new IeWorkflow
            {
                States = new List<IeState> { new IeState { Name = "New" } },
                TransitionEvents = new List<IeTransitionEvent> { new IeTransitionEvent { FromState = "New" } }
            };

            try
            {
                // Act
                var graph = new WorkflowGraph(workflow);
            }
            catch (ArgumentException ex)
            {
                // Assert
                Assert.IsNotNull(ex);
                return;
            }

            Assert.Fail("Expected ArgumentException to have been thrown.");
        }

        [TestMethod]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "AdminStore.Models.Workflow.WorkflowGraphTests")]
        public void Construction_WorkflowWithoutInitialState_ThrowsArgumentException()
        {
            // Arrange
            var workflow = new IeWorkflow
            {
                States = new List<IeState>(),
                TransitionEvents = new List<IeTransitionEvent>()
            };

            try
            {
                // Act
                var graph = new WorkflowGraph(workflow);
            }
            catch (ArgumentException ex)
            {
                // Assert
                Assert.IsNotNull(ex);
                return;
            }

            Assert.Fail("Expected ArgumentException to have been thrown.");
        }

        [TestMethod]
        public void IsConnected_WorkflowWithInitialState_ReturnTrue()
        {
            // Arrange
            var workflow = new IeWorkflow
            {
                States = new List<IeState> { new IeState { Name = "New", IsInitial = true } },
                TransitionEvents = new List<IeTransitionEvent>()
            };
            var graph = new WorkflowGraph(workflow);

            // Assert
            Assert.AreEqual(true, graph.IsConnected());
        }

        [TestMethod]
        public void IsConnected_InitialWorkflowStateWithTransitionToSelf_ReturnTrue()
        {
            // Arrange
            var workflow = new IeWorkflow
            {
                States = new List<IeState> { new IeState { Name = "New", IsInitial = true } },
                TransitionEvents = new List<IeTransitionEvent> { new IeTransitionEvent { FromState = "New", ToState = "New" } }
            };
            var graph = new WorkflowGraph(workflow);

            // Assert
            Assert.AreEqual(true, graph.IsConnected());
        }

        [TestMethod]
        public void IsConnected_WorkflowWithTwoConnectedStates_ReturnTrue()
        {
            // Arrange
            var workflow = new IeWorkflow
            {
                States = new List<IeState>
                {
                    new IeState { Name = "New", IsInitial = true },
                    new IeState { Name = "Done" }
                },
                TransitionEvents = new List<IeTransitionEvent>
                {
                    new IeTransitionEvent { FromState = "New", ToState = "Done" }
                }
            };
            var graph = new WorkflowGraph(workflow);

            // Assert
            Assert.AreEqual(true, graph.IsConnected());
        }

        [TestMethod]
        public void IsConnected_WorkflowWithTwoDoublyConnectedStates_ReturnTrue()
        {
            // Arrange
            var workflow = new IeWorkflow
            {
                States = new List<IeState>
                {
                    new IeState { Name = "New", IsInitial = true },
                    new IeState { Name = "Done" }
                },
                TransitionEvents = new List<IeTransitionEvent>
                {
                    new IeTransitionEvent { FromState = "New", ToState = "Done" },
                    new IeTransitionEvent { FromState = "Done", ToState = "New" }
                }
            };
            var graph = new WorkflowGraph(workflow);

            // Assert
            Assert.AreEqual(true, graph.IsConnected());
        }

        [TestMethod]
        public void IsConnected_WorkflowWithTwoDisconnectedStates_ReturnFalse()
        {
            // Arrange
            var workflow = new IeWorkflow
            {
                States = new List<IeState>
                {
                    new IeState { Name = "New", IsInitial = true },
                    new IeState { Name = "Done" }
                },
                TransitionEvents = new List<IeTransitionEvent>()
            };
            var graph = new WorkflowGraph(workflow);

            // Assert
            Assert.AreEqual(false, graph.IsConnected());
        }

        [TestMethod]
        public void IsConnected_ComplexConnectedWorkflow_ReturnsTrue()
        {
            // Arrange
            var workflow = new IeWorkflow
            {
                States = new List<IeState>
                {
                    new IeState { Name = "New", IsInitial = true },
                    new IeState { Name = "Ready" },
                    new IeState { Name = "In Progress" },
                    new IeState { Name = "On Hold" },
                    new IeState { Name = "Done" }
                },
                TransitionEvents = new List<IeTransitionEvent>
                {
                    new IeTransitionEvent { FromState = "New", ToState = "Ready" },
                    new IeTransitionEvent { FromState = "Ready", ToState = "In Progress" },
                    new IeTransitionEvent { FromState = "In Progress", ToState = "On Hold" },
                    new IeTransitionEvent { FromState = "On Hold", ToState = "In Progress" },
                    new IeTransitionEvent { FromState = "In Progress", ToState = "Done" }
                }
            };
            var graph = new WorkflowGraph(workflow);

            // Assert
            Assert.AreEqual(true, graph.IsConnected());
        }

        [TestMethod]
        public void IsConnected_ComplexDisconnectedWorkflow_ReturnsFalse()
        {
            // Arrange
            var workflow = new IeWorkflow
            {
                States = new List<IeState>
                {
                    new IeState { Name = "New", IsInitial = true },
                    new IeState { Name = "Ready" },
                    new IeState { Name = "In Progress" },
                    new IeState { Name = "On Hold" },
                    new IeState { Name = "Done" }
                },
                TransitionEvents = new List<IeTransitionEvent>
                {
                    new IeTransitionEvent { FromState = "New", ToState = "Ready" },
                    new IeTransitionEvent { FromState = "In Progress", ToState = "On Hold" },
                    new IeTransitionEvent { FromState = "On Hold", ToState = "In Progress" },
                    new IeTransitionEvent { FromState = "In Progress", ToState = "Done" }
                }
            };
            var graph = new WorkflowGraph(workflow);

            // Assert
            Assert.AreEqual(false, graph.IsConnected());
        }
    }
}
