using System;
using System.Collections.Generic;
using System.Linq;
using AdminStore.Models.DiagramWorkflow;
using AdminStore.Models.Enums;
using AdminStore.Models.Workflow;
using AdminStore.Services.Workflow;
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

            var sourceWF = new IeWorkflow();
            sourceWF.Name = "sourceWFTestName";
            sourceWF.Description = "TestDescription";
            sourceWF.Id = 1;
            sourceWF.IsActive = true;
            sourceWF.NewArtifactEvents = new List<IeNewArtifactEvent>()
             {
                new IeNewArtifactEvent()
                {
                    Id = 2,
                    Name = "TestName3",
                    Triggers = new List<IeTrigger>()
                    {
                        new IeTrigger()
                        {
                            Name = "TestName4",
                            Condition = new IeStateCondition()
                            {
                                State = "State5", StateId = 6
                            },
                            Action = new IeGenerateAction()
                            {
                                ArtifactType = "TestCasesArtifactType7",
                                ArtifactTypeId = 8,
                                ChildCount = 9,
                                GenerateActionType = GenerateActionTypes.TestCases
                            }
                        },
                        new IeTrigger()
                        {
                            Name = "TestName10",
                            Condition = new IeStateCondition()
                            {
                                State = "State11", StateId = 12
                            },
                            Action = new IeGenerateAction()
                            {
                                ArtifactType = "UserStoriesArtifactType13",
                                ArtifactTypeId = 14,
                                ChildCount = 15,
                                GenerateActionType = GenerateActionTypes.UserStories
                            }
                        },
                        new IeTrigger()
                        {
                            Name = "TestName16",
                            Condition = new IeStateCondition()
                            {
                                State = "State17", StateId = 18
                            },
                            Action = new IeGenerateAction()
                            {
                                ArtifactType = "ChildrenArtifactType19",
                                ArtifactTypeId = 20,
                                ChildCount = 21,
                                GenerateActionType = GenerateActionTypes.Children
                            }
                        },
                        new IeTrigger()
                        {
                            Name = "TestName22",
                            Condition = new IeStateCondition()
                            {
                                State = "State23", StateId = 24
                            },
                            Action = new IeEmailNotificationAction
                            {
                                PropertyId = 5000,

                                Emails = new List<string>
                                {
                                    "user1@company.com"
                                },
                                Message = "Message 1"
                            }
                        },
                        new IeTrigger
                        {
                            Name = "Date Property Change Trigger",
                            Action = new IePropertyChangeAction
                            {
                                PropertyName = "Date Property",
                                PropertyValue = "2017-07-21",
                                Name = "TestNameIETriggerIePropertyChangeAction",
                                PropertyId = 20
                            }
                        },
                        new IeTrigger
                        {
                            Name = "Choice Property Change Trigger",
                            Action = new IePropertyChangeAction
                            {
                                PropertyName = "Choice Property",
                                ValidValues = new List<IeValidValue>
                                {
                                    new IeValidValue { Id = 111, Value = "Canada" },
                                    new IeValidValue { Id = 222, Value = "Russia" }
                                }
                            }
                    },
                    new IeTrigger {
                        Name = "User Property Change Trigger",
                        Action = new IePropertyChangeAction
                        {
                            PropertyName = "User Property",
                            UsersGroups = new IeUsersGroups
                            {
                                UsersGroups = new List<IeUserGroup>
                                {
                                    new IeUserGroup { Name = "user1", GroupProjectId = 11, GroupProjectPath = "TestGroupProjectPath", Id = 1000, IsGroup = true },
                                    new IeUserGroup { Name = "group2", IsGroup = true }
                                }
                            }
                        }
                    }
                  }
                }
             };
            sourceWF.Projects = new List<IeProject>()
             {
                new IeProject()
                {
                    Id = 26,
                    Path = "Path27",
                    ArtifactTypes = new List<IeArtifactType>()
                    {
                        new IeArtifactType()
                        {
                            Id = 28,
                            Name = "TestName29"
                        }
                    }
                }
             };
            sourceWF.PropertyChangeEvents = new List<IePropertyChangeEvent>()
             {
                new IePropertyChangeEvent()
                {
                    Id = 30,
                    Name = "TestName31",
                    PropertyId = 32,
                    PropertyName = "TestName33",
                    Triggers = new List<IeTrigger>()
                    {
                        new IeTrigger()
                        {
                            Name = "TestName34",
                            Condition = new IeStateCondition()
                            {
                                State = "State35", StateId = 36
                            },
                            Action = new IeGenerateAction()
                            {
                                ArtifactType = "ArtifactType37",
                                ArtifactTypeId = 38,
                                ChildCount = 39,
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
                    Id = 40,
                    IsInitial = true,
                    Location = "TestLocation41",
                    Name = "TestName42"
                }
             };
            sourceWF.TransitionEvents = new List<IeTransitionEvent>()
             {
                new IeTransitionEvent()
                {
                    FromState = "FromState43",
                    FromStateId = 44,
                    Id = 45,
                    Name = "TestName46",
                    ToState = "ToState47",
                    ToStateId = 48,
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
                            Name = "TestName49",
                            Condition = new IeStateCondition()
                            {
                                State = "State50", StateId = 51
                            },
                            Action = new IeGenerateAction()
                            {
                                ArtifactType = "ArtifactType52",
                                ArtifactTypeId = 53,
                                ChildCount = 54,
                                GenerateActionType = GenerateActionTypes.UserStories
                            }
                        }
                    },
                    PermissionGroups = new List<IeGroup>()
                    {
                        new IeGroup()
                        {
                            Id = 55,
                            Name = "TestName56"
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

            Assert.AreEqual(7, mappedWorkflow.NewArtifactEvents.ToList()[0].Triggers.Count());

            Assert.AreEqual(expectedNewArtifactEvent.Triggers[0].Name, actualNewArtifactEvent.Triggers.ToList()[0].Name);

            var expectedNewArtifactEventTriggerCondition = (IeStateCondition)sourceWF.NewArtifactEvents[0].Triggers[0].Condition;
            var actualNewArtifactEventTriggerCondition = mappedWorkflow.NewArtifactEvents.ToList()[0].Triggers.ToList()[0].Condition;

            Assert.IsNotNull(expectedNewArtifactEventTriggerCondition);
            Assert.IsNotNull(actualNewArtifactEventTriggerCondition);

            Assert.AreEqual(expectedNewArtifactEventTriggerCondition.StateId, actualNewArtifactEventTriggerCondition.StateId);
            Assert.AreEqual(expectedNewArtifactEventTriggerCondition.State, actualNewArtifactEventTriggerCondition.State);

            var expectedNewArtifactEventTriggerActionTestCases = (IeGenerateAction)sourceWF.NewArtifactEvents[0].Triggers[0].Action;
            var actualNewArtifactEventTriggerActionTestCases = (DGenerateAction)mappedWorkflow.NewArtifactEvents.ToList()[0].Triggers.ToList()[0].Action;

            Assert.IsNotNull(expectedNewArtifactEventTriggerActionTestCases);
            Assert.IsNotNull(actualNewArtifactEventTriggerActionTestCases);

            Assert.AreEqual(expectedNewArtifactEventTriggerActionTestCases.ActionType, actualNewArtifactEventTriggerActionTestCases.ActionType);
            Assert.AreEqual(expectedNewArtifactEventTriggerActionTestCases.ArtifactType, actualNewArtifactEventTriggerActionTestCases.ArtifactType);
            Assert.AreEqual(expectedNewArtifactEventTriggerActionTestCases.ArtifactTypeId, actualNewArtifactEventTriggerActionTestCases.ArtifactTypeId);
            Assert.AreEqual(expectedNewArtifactEventTriggerActionTestCases.ChildCount, actualNewArtifactEventTriggerActionTestCases.ChildCount);
            Assert.AreEqual(expectedNewArtifactEventTriggerActionTestCases.GenerateActionType, actualNewArtifactEventTriggerActionTestCases.GenerateActionType);

            var expectedNewArtifactEventTriggerActionUserStories = (IeGenerateAction)sourceWF.NewArtifactEvents[0].Triggers[1].Action;
            var actualNewArtifactEventTriggerActionUserStories = (DGenerateAction)mappedWorkflow.NewArtifactEvents.ToList()[0].Triggers.ToList()[1].Action;

            Assert.IsNotNull(expectedNewArtifactEventTriggerActionUserStories);
            Assert.IsNotNull(actualNewArtifactEventTriggerActionUserStories);

            Assert.AreEqual(expectedNewArtifactEventTriggerActionUserStories.ActionType, actualNewArtifactEventTriggerActionUserStories.ActionType);
            Assert.AreEqual(expectedNewArtifactEventTriggerActionUserStories.ArtifactType, actualNewArtifactEventTriggerActionUserStories.ArtifactType);
            Assert.AreEqual(expectedNewArtifactEventTriggerActionUserStories.ArtifactTypeId, actualNewArtifactEventTriggerActionUserStories.ArtifactTypeId);
            Assert.AreEqual(expectedNewArtifactEventTriggerActionUserStories.ChildCount, actualNewArtifactEventTriggerActionUserStories.ChildCount);
            Assert.AreEqual(expectedNewArtifactEventTriggerActionUserStories.GenerateActionType, actualNewArtifactEventTriggerActionUserStories.GenerateActionType);

            var expectedNewArtifactEventTriggerActionChildren = (IeGenerateAction)sourceWF.NewArtifactEvents[0].Triggers[2].Action;
            var actualNewArtifactEventTriggerActionChildren = (DGenerateAction)mappedWorkflow.NewArtifactEvents.ToList()[0].Triggers.ToList()[2].Action;

            Assert.IsNotNull(expectedNewArtifactEventTriggerActionChildren);
            Assert.IsNotNull(actualNewArtifactEventTriggerActionChildren);

            Assert.AreEqual(expectedNewArtifactEventTriggerActionChildren.ActionType, actualNewArtifactEventTriggerActionChildren.ActionType);
            Assert.AreEqual(expectedNewArtifactEventTriggerActionChildren.ArtifactType, actualNewArtifactEventTriggerActionChildren.ArtifactType);
            Assert.AreEqual(expectedNewArtifactEventTriggerActionChildren.ArtifactTypeId, actualNewArtifactEventTriggerActionChildren.ArtifactTypeId);
            Assert.AreEqual(expectedNewArtifactEventTriggerActionChildren.ChildCount, actualNewArtifactEventTriggerActionChildren.ChildCount);
            Assert.AreEqual(expectedNewArtifactEventTriggerActionChildren.GenerateActionType, actualNewArtifactEventTriggerActionChildren.GenerateActionType);

            var expectedNewArtifactEventTriggerEmailNotificationAction = (IeEmailNotificationAction)sourceWF.NewArtifactEvents[0].Triggers[3].Action;
            var actualNewArtifactEventTriggerEmailNotification = (DEmailNotificationAction)mappedWorkflow.NewArtifactEvents.ToList()[0].Triggers.ToList()[3].Action;

            Assert.IsNotNull(expectedNewArtifactEventTriggerEmailNotificationAction);
            Assert.IsNotNull(actualNewArtifactEventTriggerEmailNotification);

            Assert.AreEqual(expectedNewArtifactEventTriggerEmailNotificationAction.ActionType, actualNewArtifactEventTriggerEmailNotification.ActionType);
            Assert.AreEqual(1, actualNewArtifactEventTriggerEmailNotification.Emails.Count());

            Assert.AreEqual(expectedNewArtifactEventTriggerEmailNotificationAction.Emails[0], actualNewArtifactEventTriggerEmailNotification.Emails.ToList()[0]);
            Assert.AreEqual(expectedNewArtifactEventTriggerEmailNotificationAction.Message, actualNewArtifactEventTriggerEmailNotification.Message);
            Assert.AreEqual(expectedNewArtifactEventTriggerEmailNotificationAction.PropertyId, actualNewArtifactEventTriggerEmailNotification.PropertyId);
            Assert.AreEqual(expectedNewArtifactEventTriggerEmailNotificationAction.PropertyName, actualNewArtifactEventTriggerEmailNotification.PropertyName);


            var expectedNewArtifactEventTriggerPropertyChangeAction = (IePropertyChangeAction)sourceWF.NewArtifactEvents[0].Triggers[4].Action;
            var actualNewArtifactEventTriggerPropertyChangeAction = (DPropertyChangeAction)mappedWorkflow.NewArtifactEvents.ToList()[0].Triggers.ToList()[4].Action;

            Assert.IsNotNull(expectedNewArtifactEventTriggerPropertyChangeAction);
            Assert.IsNotNull(actualNewArtifactEventTriggerPropertyChangeAction);


            Assert.AreEqual(expectedNewArtifactEventTriggerPropertyChangeAction.PropertyId, actualNewArtifactEventTriggerPropertyChangeAction.PropertyId);
            Assert.AreEqual(expectedNewArtifactEventTriggerPropertyChangeAction.PropertyName, actualNewArtifactEventTriggerPropertyChangeAction.PropertyName);
            Assert.AreEqual(expectedNewArtifactEventTriggerPropertyChangeAction.PropertyValue, actualNewArtifactEventTriggerPropertyChangeAction.PropertyValue);
            Assert.AreEqual(expectedNewArtifactEventTriggerPropertyChangeAction.ActionType, actualNewArtifactEventTriggerPropertyChangeAction.ActionType);

            var expectedNewArtifactEventTriggerPropertyChangeValidValuesAction = (IePropertyChangeAction)sourceWF.NewArtifactEvents[0].Triggers[5].Action;
            var actualNewArtifactEventTriggerPropertyChangeValidValuesAction = (DPropertyChangeAction)mappedWorkflow.NewArtifactEvents.ToList()[0].Triggers.ToList()[5].Action;

            Assert.IsNotNull(expectedNewArtifactEventTriggerPropertyChangeValidValuesAction);
            Assert.IsNotNull(actualNewArtifactEventTriggerPropertyChangeValidValuesAction);


            Assert.AreEqual(expectedNewArtifactEventTriggerPropertyChangeValidValuesAction.PropertyId, actualNewArtifactEventTriggerPropertyChangeValidValuesAction.PropertyId);
            Assert.AreEqual(expectedNewArtifactEventTriggerPropertyChangeValidValuesAction.PropertyName, actualNewArtifactEventTriggerPropertyChangeValidValuesAction.PropertyName);
            Assert.AreEqual(expectedNewArtifactEventTriggerPropertyChangeValidValuesAction.PropertyValue, actualNewArtifactEventTriggerPropertyChangeValidValuesAction.PropertyValue);
            Assert.AreEqual(expectedNewArtifactEventTriggerPropertyChangeValidValuesAction.ActionType, actualNewArtifactEventTriggerPropertyChangeValidValuesAction.ActionType);
            Assert.AreEqual(expectedNewArtifactEventTriggerPropertyChangeValidValuesAction.ValidValues.Count(), actualNewArtifactEventTriggerPropertyChangeValidValuesAction.ValidValues.Count());
            Assert.AreEqual(expectedNewArtifactEventTriggerPropertyChangeValidValuesAction.ValidValues[0].Id, actualNewArtifactEventTriggerPropertyChangeValidValuesAction.ValidValues.ToList()[0].Id);
            Assert.AreEqual(expectedNewArtifactEventTriggerPropertyChangeValidValuesAction.ValidValues[0].Value, actualNewArtifactEventTriggerPropertyChangeValidValuesAction.ValidValues.ToList()[0].Value);
            Assert.AreEqual(expectedNewArtifactEventTriggerPropertyChangeValidValuesAction.ValidValues[1].Id, actualNewArtifactEventTriggerPropertyChangeValidValuesAction.ValidValues.ToList()[1].Id);
            Assert.AreEqual(expectedNewArtifactEventTriggerPropertyChangeValidValuesAction.ValidValues[1].Value, actualNewArtifactEventTriggerPropertyChangeValidValuesAction.ValidValues.ToList()[1].Value);

            var expectedNewArtifactEventTriggerPropertyChangeGroupsAction = (IePropertyChangeAction)sourceWF.NewArtifactEvents[0].Triggers[6].Action;
            var actualNewArtifactEventTriggerPropertyChangeGroupsAction = (DPropertyChangeAction)mappedWorkflow.NewArtifactEvents.ToList()[0].Triggers.ToList()[6].Action;

            Assert.IsNotNull(expectedNewArtifactEventTriggerPropertyChangeGroupsAction);
            Assert.IsNotNull(actualNewArtifactEventTriggerPropertyChangeGroupsAction);


            Assert.AreEqual(expectedNewArtifactEventTriggerPropertyChangeGroupsAction.PropertyId, actualNewArtifactEventTriggerPropertyChangeGroupsAction.PropertyId);
            Assert.AreEqual(expectedNewArtifactEventTriggerPropertyChangeGroupsAction.PropertyName, actualNewArtifactEventTriggerPropertyChangeGroupsAction.PropertyName);
            Assert.AreEqual(expectedNewArtifactEventTriggerPropertyChangeGroupsAction.PropertyValue, actualNewArtifactEventTriggerPropertyChangeGroupsAction.PropertyValue);
            Assert.AreEqual(expectedNewArtifactEventTriggerPropertyChangeGroupsAction.ActionType, actualNewArtifactEventTriggerPropertyChangeGroupsAction.ActionType);
            Assert.AreEqual(expectedNewArtifactEventTriggerPropertyChangeGroupsAction.UsersGroups.UsersGroups.Count, actualNewArtifactEventTriggerPropertyChangeGroupsAction.UsersGroups.UsersGroups.Count());
            Assert.AreEqual(expectedNewArtifactEventTriggerPropertyChangeGroupsAction.UsersGroups.UsersGroups[0].Id, actualNewArtifactEventTriggerPropertyChangeGroupsAction.UsersGroups.UsersGroups.ToList()[0].Id);
            Assert.AreEqual(expectedNewArtifactEventTriggerPropertyChangeGroupsAction.UsersGroups.UsersGroups[0].Name, actualNewArtifactEventTriggerPropertyChangeGroupsAction.UsersGroups.UsersGroups.ToList()[0].Name);
            Assert.AreEqual(expectedNewArtifactEventTriggerPropertyChangeGroupsAction.UsersGroups.UsersGroups[0].IsGroup, actualNewArtifactEventTriggerPropertyChangeGroupsAction.UsersGroups.UsersGroups.ToList()[0].IsGroup);
            Assert.AreEqual(expectedNewArtifactEventTriggerPropertyChangeGroupsAction.UsersGroups.UsersGroups[0].GroupProjectId, actualNewArtifactEventTriggerPropertyChangeGroupsAction.UsersGroups.UsersGroups.ToList()[0].GroupProjectId);
            Assert.AreEqual(expectedNewArtifactEventTriggerPropertyChangeGroupsAction.UsersGroups.UsersGroups[0].GroupProjectPath, actualNewArtifactEventTriggerPropertyChangeGroupsAction.UsersGroups.UsersGroups.ToList()[0].GroupProjectPath);



            Assert.AreEqual(1, mappedWorkflow.Projects.Count());
            Assert.AreEqual(sourceWF.Projects[0].Id, mappedWorkflow.Projects.ToList()[0].Id);
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
