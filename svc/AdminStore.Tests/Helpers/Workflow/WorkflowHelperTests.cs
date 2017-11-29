using System;
using System.Collections.Generic;
using System.Linq;
using AdminStore.Models.DiagramWorkflow;
using AdminStore.Models.Enums;
using AdminStore.Models.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Helpers.Workflow
{
    [TestClass]
    public class WorkflowHelperTests
    {
        [TestMethod]
        public void CollectionEquals_NullNull_True()
        {
            // Assert
            Assert.IsTrue(WorkflowHelper.CollectionEquals<int>(null, null));
        }

        [TestMethod]
        public void CollectionEquals_FirstNull_False()
        {
            // Assert
            Assert.IsFalse(WorkflowHelper.CollectionEquals(null, new List<int> { 1 }));
        }

        [TestMethod]
        public void CollectionEquals_SecondNull_False()
        {
            // Assert
            Assert.IsFalse(WorkflowHelper.CollectionEquals(new List<int> { 1 }, null));
        }

        [TestMethod]
        public void CollectionEquals_CountsNotEqual_False()
        {
            // Assert
            Assert.IsFalse(WorkflowHelper.CollectionEquals(new List<int> { 1, 2 }, new List<int> { 1 }));
        }

        [TestMethod]
        public void CollectionEquals_Equals_True()
        {
            // Assert
            Assert.IsTrue(WorkflowHelper.CollectionEquals(new List<int> { 1, 2 }, new List<int> { 1, 2 }));
        }

        #region MapIeWorkflowToDWorkflow

        [TestMethod]
        public void MapIeWorkflowToDWorkflow_Map_Successfully()
        {
            // Arrange
            IeWorkflow sourceWF = new IeWorkflow();
            sourceWF.Name = "TestName";
            sourceWF.Description = "TestDescription";
            sourceWF.Id = 1;
            sourceWF.IsActive = true;
            sourceWF.NewArtifactEvents = new List<IeNewArtifactEvent>()
            {
                new IeNewArtifactEvent()
                {
                    Id = 2,
                    Name = "TestName2",
                    Triggers = new List<IeTrigger>()
                    {
                        new IeTrigger()
                        {
                            Name = "TestName3",
                            Condition = new IeStateCondition()
                            {
                                State = "State1", StateId = 1
                            },
                            Action = new IeGenerateAction()
                            {
                                ArtifactType = "ArtifactType1",
                                ArtifactTypeId = 1,
                                ChildCount = 1,
                                GenerateActionType = GenerateActionTypes.TestCases
                            }
                        }
                    }
                }
            };
            sourceWF.Projects = new List<IeProject>()
            {
                new IeProject()
                {
                    Id = 1,
                    Path = "Path",
                    ArtifactTypes = new List<IeArtifactType>()
                    {
                        new IeArtifactType()
                        {
                            Id = 2,
                            Name = "TestName4"
                        }
                    }
                }
            };
            sourceWF.PropertyChangeEvents = new List<IePropertyChangeEvent>()
            {
                new IePropertyChangeEvent()
                {
                    Id = 2,
                    Name = "TestName5",
                    PropertyId = 3,
                    PropertyName = "TestName6",
                    Triggers = new List<IeTrigger>()
                    {
                        new IeTrigger()
                        {
                            Name = "TestName7",
                            Condition = new IeStateCondition()
                            {
                                State = "State2", StateId = 2
                            },
                            Action = new IeGenerateAction()
                            {
                                ArtifactType = "ArtifactType2",
                                ArtifactTypeId = 2,
                                ChildCount = 2,
                                GenerateActionType = GenerateActionTypes.UserStories
                            }
                        }
                    }
                }
            };
            sourceWF.States = new List<IeState>()
            {
                new IeState()
                {
                    Id = 3,
                    IsInitial = true,
                    Location = "TestLocation",
                    Name = "TestName8"
                }
            };
            sourceWF.TransitionEvents = new List<IeTransitionEvent>()
            {
                new IeTransitionEvent()
                {
                    FromState = "FromState1",
                    FromStateId = 4,
                    Id = 5,
                    Name = "TestName9",
                    ToState = "ToState",
                    ToStateId = 6,
                    SkipPermissionGroups = true,
                    PortPair = new IePortPair()
                    {
                        FromPort = DiagramPort.Bottom,
                        ToPort = DiagramPort.Left
                    },
                    Triggers = new List<IeTrigger>()
                    {
                        new IeTrigger()
                        {
                            Name = "TestName10",
                            Condition = new IeStateCondition()
                            {
                                State = "State3", StateId = 7
                            },
                            Action = new IeGenerateAction()
                            {
                                ArtifactType = "ArtifactType3",
                                ArtifactTypeId = 8,
                                ChildCount = 9,
                                GenerateActionType = GenerateActionTypes.UserStories
                            }
                        }
                    },
                    PermissionGroups = new List<IeGroup>()
                    {
                        new IeGroup()
                        {
                            Id = 10,
                            Name = "TestName11"
                        }
                    }
                }
            };

            // Act
            DWorkflow mappedWorkflow = WorkflowHelper.MapIeWorkflowToDWorkflow(sourceWF);

            // Assert
            Assert.AreEqual(sourceWF.Name, mappedWorkflow.Name);
            Assert.AreEqual(sourceWF.Id, mappedWorkflow.Id);
            Assert.AreEqual(sourceWF.Description, mappedWorkflow.Description);
            Assert.AreEqual(sourceWF.IsActive, mappedWorkflow.IsActive);

            Assert.AreEqual(1, mappedWorkflow.NewArtifactEvents.Count());

            var expectedNewArtifactEvent = sourceWF.NewArtifactEvents[0];
            var actualNewArtifactEvent = mappedWorkflow.NewArtifactEvents.ToList()[0];
            Assert.IsNotNull(expectedNewArtifactEvent);
            Assert.IsNotNull(actualNewArtifactEvent);

            Assert.AreEqual(expectedNewArtifactEvent.Id, actualNewArtifactEvent.Id);
            Assert.AreEqual(expectedNewArtifactEvent.Name, actualNewArtifactEvent.Name);

            Assert.AreEqual(1, mappedWorkflow.NewArtifactEvents.ToList()[0].Triggers.Count());
            Assert.AreEqual(expectedNewArtifactEvent.Triggers[0].Name, actualNewArtifactEvent.Triggers.ToList()[0].Name);

            var expectedNewArtifactEventTriggerCondition = (IeStateCondition)sourceWF.NewArtifactEvents[0].Triggers[0].Condition;
            var actualNewArtifactEventTriggerCondition = mappedWorkflow.NewArtifactEvents.ToList()[0].Triggers.ToList()[0].Condition;

            Assert.IsNotNull(expectedNewArtifactEventTriggerCondition);
            Assert.IsNotNull(actualNewArtifactEventTriggerCondition);

            Assert.AreEqual(expectedNewArtifactEventTriggerCondition.StateId, actualNewArtifactEventTriggerCondition.StateId);
            Assert.AreEqual(expectedNewArtifactEventTriggerCondition.State, actualNewArtifactEventTriggerCondition.State);

            var expectedNewArtifactEventTriggerAction = (IeGenerateAction)sourceWF.NewArtifactEvents[0].Triggers[0].Action;
            var actualNewArtifactEventTriggerAction = (DGenerateAction)mappedWorkflow.NewArtifactEvents.ToList()[0].Triggers.ToList()[0].Action;

            Assert.IsNotNull(expectedNewArtifactEventTriggerAction);
            Assert.IsNotNull(actualNewArtifactEventTriggerAction);

            Assert.AreEqual(expectedNewArtifactEventTriggerAction.ActionType, actualNewArtifactEventTriggerAction.ActionType);
            Assert.AreEqual(expectedNewArtifactEventTriggerAction.ArtifactType, actualNewArtifactEventTriggerAction.ArtifactType);
            Assert.AreEqual(expectedNewArtifactEventTriggerAction.ArtifactTypeId, actualNewArtifactEventTriggerAction.ArtifactTypeId);
            Assert.AreEqual(expectedNewArtifactEventTriggerAction.ChildCount, actualNewArtifactEventTriggerAction.ChildCount);
            Assert.AreEqual(expectedNewArtifactEventTriggerAction.GenerateActionType, actualNewArtifactEventTriggerAction.GenerateActionType);

            Assert.AreEqual(1, mappedWorkflow.Projects.Count());
            Assert.AreEqual(sourceWF.Projects[0].Id, mappedWorkflow.Projects.ToList()[0].Id);
            Assert.AreEqual(sourceWF.Projects[0].Path, mappedWorkflow.Projects.ToList()[0].Path);
            Assert.AreEqual(1, mappedWorkflow.Projects.ToList()[0].ArtifactTypes.ToList().Count);
            Assert.AreEqual(sourceWF.Projects[0].ArtifactTypes[0].Id, mappedWorkflow.Projects.ToList()[0].ArtifactTypes.ToList()[0].Id);
            Assert.AreEqual(sourceWF.Projects[0].ArtifactTypes[0].Name, mappedWorkflow.Projects.ToList()[0].ArtifactTypes.ToList()[0].Name);

            Assert.AreEqual(1, mappedWorkflow.PropertyChangeEvents.Count());
            Assert.AreEqual(sourceWF.PropertyChangeEvents[0].Id, mappedWorkflow.PropertyChangeEvents.ToList()[0].Id);
            Assert.AreEqual(sourceWF.PropertyChangeEvents[0].Name, mappedWorkflow.PropertyChangeEvents.ToList()[0].Name);
            Assert.AreEqual(sourceWF.PropertyChangeEvents[0].PropertyId, mappedWorkflow.PropertyChangeEvents.ToList()[0].PropertyId);
            Assert.AreEqual(sourceWF.PropertyChangeEvents[0].PropertyName, mappedWorkflow.PropertyChangeEvents.ToList()[0].PropertyName);

            Assert.AreEqual(sourceWF.PropertyChangeEvents[0].Triggers[0].Name, mappedWorkflow.PropertyChangeEvents.ToList()[0].Triggers.ToList()[0].Name);

            var expectedPropertyChangeEventTriggerAction = ((IeGenerateAction)(sourceWF.PropertyChangeEvents[0].Triggers[0].Action));
            var actualPropertyChangeEventTriggerAction = ((DGenerateAction)mappedWorkflow.PropertyChangeEvents.ToList()[0].Triggers.ToList()[0].Action);

            Assert.IsNotNull(expectedPropertyChangeEventTriggerAction);
            Assert.IsNotNull(actualPropertyChangeEventTriggerAction);

            var expectedPropertyChangeEventTriggerCondition = ((IeStateCondition)(sourceWF.PropertyChangeEvents[0].Triggers[0].Condition));
            var actualPropertyChangeEventTriggerCondition = mappedWorkflow.PropertyChangeEvents.ToList()[0].Triggers.ToList()[0].Condition;

            Assert.IsNotNull(expectedPropertyChangeEventTriggerCondition);
            Assert.IsNotNull(actualPropertyChangeEventTriggerCondition);

            Assert.AreEqual(expectedPropertyChangeEventTriggerCondition.StateId, actualPropertyChangeEventTriggerCondition.StateId);
            Assert.AreEqual(expectedPropertyChangeEventTriggerCondition.State, actualPropertyChangeEventTriggerCondition.State);
            Assert.AreEqual(expectedPropertyChangeEventTriggerAction.ActionType, actualPropertyChangeEventTriggerAction.ActionType);
            Assert.AreEqual(expectedPropertyChangeEventTriggerAction.ArtifactType, actualPropertyChangeEventTriggerAction.ArtifactType);
            Assert.AreEqual(expectedPropertyChangeEventTriggerAction.ArtifactTypeId, actualPropertyChangeEventTriggerAction.ArtifactTypeId);
            Assert.AreEqual(expectedPropertyChangeEventTriggerAction.ChildCount, actualPropertyChangeEventTriggerAction.ChildCount);
            Assert.AreEqual(expectedPropertyChangeEventTriggerAction.GenerateActionType, actualPropertyChangeEventTriggerAction.GenerateActionType);

            Assert.AreEqual(1, mappedWorkflow.States.Count());
            Assert.AreEqual(sourceWF.States[0].Id, mappedWorkflow.States.ToList()[0].Id);
            Assert.AreEqual(sourceWF.States[0].Name, mappedWorkflow.States.ToList()[0].Name);
            Assert.AreEqual(sourceWF.States[0].IsInitial, mappedWorkflow.States.ToList()[0].IsInitial);
            Assert.AreEqual(sourceWF.States[0].Location, mappedWorkflow.States.ToList()[0].Location);

            Assert.AreEqual(1, mappedWorkflow.TransitionEvents.Count());
            Assert.AreEqual(sourceWF.TransitionEvents[0].Id, mappedWorkflow.TransitionEvents.ToList()[0].Id);
            Assert.AreEqual(sourceWF.TransitionEvents[0].Name, mappedWorkflow.TransitionEvents.ToList()[0].Name);
            Assert.AreEqual(sourceWF.TransitionEvents[0].FromState, mappedWorkflow.TransitionEvents.ToList()[0].FromState);
            Assert.AreEqual(sourceWF.TransitionEvents[0].FromStateId, mappedWorkflow.TransitionEvents.ToList()[0].FromStateId);
            Assert.AreEqual(sourceWF.TransitionEvents[0].ToState, mappedWorkflow.TransitionEvents.ToList()[0].ToState);
            Assert.AreEqual(sourceWF.TransitionEvents[0].ToStateId, mappedWorkflow.TransitionEvents.ToList()[0].ToStateId);
            Assert.AreEqual(sourceWF.TransitionEvents[0].SkipPermissionGroups, mappedWorkflow.TransitionEvents.ToList()[0].SkipPermissionGroups);
            Assert.AreEqual(sourceWF.TransitionEvents[0].PortPair.FromPort, mappedWorkflow.TransitionEvents.ToList()[0].PortPair.FromPort);
            Assert.AreEqual(sourceWF.TransitionEvents[0].PortPair.ToPort, mappedWorkflow.TransitionEvents.ToList()[0].PortPair.ToPort);


            var expectedTransitionEventTriggerAction = ((IeGenerateAction)(sourceWF.TransitionEvents[0].Triggers[0].Action));
            var actualTransitionEventTriggerAction = ((DGenerateAction)mappedWorkflow.TransitionEvents.ToList()[0].Triggers.ToList()[0].Action);

            Assert.IsNotNull(expectedTransitionEventTriggerAction);
            Assert.IsNotNull(actualTransitionEventTriggerAction);

            var expectedTransitionEventTriggerCondition = ((IeStateCondition)(sourceWF.TransitionEvents[0].Triggers[0].Condition));
            var actualTransitionEventTriggerCondition = mappedWorkflow.TransitionEvents.ToList()[0].Triggers.ToList()[0].Condition;

            Assert.IsNotNull(expectedTransitionEventTriggerCondition);
            Assert.IsNotNull(actualTransitionEventTriggerCondition);

            Assert.AreEqual(sourceWF.TransitionEvents[0].Triggers[0].Name, mappedWorkflow.TransitionEvents.ToList()[0].Triggers.ToList()[0].Name);
            Assert.AreEqual(expectedTransitionEventTriggerCondition.StateId, actualTransitionEventTriggerCondition.StateId);
            Assert.AreEqual(expectedTransitionEventTriggerCondition.State, actualTransitionEventTriggerCondition.State);
            Assert.AreEqual(expectedTransitionEventTriggerAction.ActionType, actualTransitionEventTriggerAction.ActionType);
            Assert.AreEqual(expectedTransitionEventTriggerAction.ArtifactType, actualTransitionEventTriggerAction.ArtifactType);
            Assert.AreEqual(expectedTransitionEventTriggerAction.ArtifactTypeId, actualTransitionEventTriggerAction.ArtifactTypeId);
            Assert.AreEqual(expectedTransitionEventTriggerAction.ChildCount, actualTransitionEventTriggerAction.ChildCount);
            Assert.AreEqual(expectedTransitionEventTriggerAction.GenerateActionType, actualTransitionEventTriggerAction.GenerateActionType);

            Assert.AreEqual(1, mappedWorkflow.TransitionEvents.ToList()[0].PermissionGroups.Count());
            Assert.AreEqual(sourceWF.TransitionEvents[0].PermissionGroups[0].Name, mappedWorkflow.TransitionEvents.ToList()[0].PermissionGroups.ToList()[0].Name);
            Assert.AreEqual(sourceWF.TransitionEvents[0].PermissionGroups[0].Id, mappedWorkflow.TransitionEvents.ToList()[0].PermissionGroups.ToList()[0].Id);
        }

        #endregion MapIeWorkflowToDWorkflow
    }
}
