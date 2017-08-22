using System.Collections.Generic;
using AdminStore.Models.Workflow;
using ServiceLibrary.Models.Enums;

namespace AdminStore.Services.Workflow
{
    public static class WorkflowTestHelper
    {
        public static IeWorkflow TestWorkflow
        {
            get
            {
                var workflow = new IeWorkflow
                {
                    Name = "My Workflow",
                    Description = "This is my workflow.",
                    States = new List<IeState>(),
                    TransitionEvents = new List<IeTransitionEvent>(),
                    PropertyChangeEvents = new List<IePropertyChangeEvent>(),
                    NewArtifactEvents = new List<IeNewArtifactEvent>(),
                    Projects = new List<IeProject>()
                };

                // States
                workflow.States.Add(new IeState
                {
                    Name = "New",
                    IsInitial = true
                });

                workflow.States.Add(new IeState
                {
                    Name = "Active"
                });

                workflow.States.Add(new IeState
                {
                    Name = "Closed"
                });

                // Projects
                workflow.Projects.Add(new IeProject
                {
                    Id = 10,
                    ArtifactTypes = new List<IeArtifactType>
                {
                    new IeArtifactType { Name = "Actor" },
                    new IeArtifactType { Name = "Use Case" }
                }

                });

                workflow.Projects.Add(new IeProject
                {
                    Path = @"Blueprint/folder/project1",
                    ArtifactTypes = new List<IeArtifactType>
                {
                    new IeArtifactType { Name = "Actor" },
                    new IeArtifactType { Name = "Process" }
                }
                });

                workflow.Projects.Add(new IeProject
                {
                    Path = @"Blueprint/folder/project2",
                    ArtifactTypes = new List<IeArtifactType>
                {
                    new IeArtifactType { Name = "Business Rules" }
                }
                });

                // State Change Events (Transitions)
                workflow.TransitionEvents.Add(new IeTransitionEvent
                {
                    Name = "From New to Active",
                    FromState = "New",
                    ToState = "Active",
                    SkipPermissionGroups = true,
                    PermissionGroups = new List<IeGroup>
                {
                    new IeGroup { Name = "Authors"},
                    new IeGroup { Name = "Group 1" }
                },
                    Triggers = new List<IeTrigger>
                {
                    new IeTrigger {
                        Name = "Email Notification Trigger 1",
                        Action = new IeEmailNotificationAction
                        {
                            Emails = new List<string>
                            {
                                "user1@company.com"
                            },
                            Message = "Message 1"
                        }
                    },
                    new IeTrigger {
                        Name = "Add Children Trigger 1",
                        Action = new IeGenerateAction
                        {
                            GenerateActionType = GenerateActionTypes.Children,
                            ArtifactType = "Textual Requirement",
                            ChildCount = 3
                        }
                    },
                    new IeTrigger {
                        Name = "Generate User Stories Trigger 1",
                        Action = new IeGenerateAction
                        {
                            GenerateActionType = GenerateActionTypes.UserStories
                        }
                    },
                    new IeTrigger {
                        Name = "Generate Test Cases Trigger 1",
                        Action = new IeGenerateAction
                        {
                            GenerateActionType = GenerateActionTypes.TestCases
                        }
                    }
                }
                });

                workflow.TransitionEvents.Add(new IeTransitionEvent
                {
                    Name = "From Active to Closed",
                    FromState = "Active",
                    ToState = "Closed",
                    SkipPermissionGroups = true,
                    PermissionGroups = new List<IeGroup>
                {
                    new IeGroup { Name = "Authors"},
                    new IeGroup { Name = "Group 2" }
                },
                    Triggers = new List<IeTrigger>
                {
                    new IeTrigger {
                        Name = "Text Property Change Trigger",
                        Action = new IePropertyChangeAction
                        {
                            PropertyName = "Text Property",
                            PropertyValue = "New Property Value"
                        }
                    },
                    new IeTrigger {
                        Name = "Number Property Change Trigger",
                        Action = new IePropertyChangeAction
                        {
                            PropertyName = "Number Property",
                            PropertyValue = "10.56"
                        }
                    },
                    new IeTrigger {
                        Name = "Date Property Change Trigger",
                        Action = new IePropertyChangeAction
                        {
                            PropertyName = "Date Property",
                            PropertyValue = "2017-07-21"
                        }
                    },
                    new IeTrigger {
                        Name = "Choice Property Change Trigger",
                        Action = new IePropertyChangeAction
                        {
                            PropertyName = "Choice Property",
                            ValidValues = new List<IeValidValue>
                            {
                                new IeValidValue { Value = "Canada" },
                                new IeValidValue { Value = "Russia"}
                            }
                        }
                    },
                    new IeTrigger {
                        Name = "User Property Change Trigger",
                        Action = new IePropertyChangeAction
                        {
                            PropertyName = "User Property",
                            UsersGroups = new List<IeUserGroup>
                            {
                                new IeUserGroup { Name = "user1"},
                                new IeUserGroup { Name = "group2", IsGroup = true}
                            }
                        }
                    }
                }
                });
                workflow.TransitionEvents.Add(new IeTransitionEvent
                {
                    Name = "Back to Close",
                    FromState = "Active",
                    ToState = "New"
                });

                // Property Change Events
                workflow.PropertyChangeEvents.Add(new IePropertyChangeEvent
                {
                    Name = "Text Property Changed",
                    PropertyName = "My Text Property",
                    Triggers = new List<IeTrigger>
                {
                    new IeTrigger {
                        Name = "Email Notification Trigger 2",
                        Condition = new IeStateCondition
                        {
                            State = "Closed"
                        },
                        Action = new IeEmailNotificationAction
                        {
                            Emails = new List<string>
                            {
                                "user2@company.com",
                                "user3@company.com"
                            },
                            Message = "Message 2"
                        }
                    }
                }
                });

                workflow.PropertyChangeEvents.Add(new IePropertyChangeEvent
                {
                    Name = "Choice Property Changed",
                    PropertyName = "My Choice Property",
                    Triggers = new List<IeTrigger>
                {
                    new IeTrigger {
                        Name = "Email Notification Trigger 3",
                        Action = new IeEmailNotificationAction
                        {
                            PropertyName = "Users",
                            Message = "Message 3"
                        }
                    }
                }
                });

                // New Artifact Events
                workflow.NewArtifactEvents.Add(new IeNewArtifactEvent
                {
                    Name = "New Artifact 1",
                    Triggers = new List<IeTrigger>
                {
                    new IeTrigger {
                        Name = "Email Notification Trigger 4",
                        Action = new IeEmailNotificationAction
                        {
                            Emails = new List<string>
                            {
                                "user5@company.com"
                            },
                            Message = "Message 4"
                        }
                    },
                    new IeTrigger {
                        Name = "Add Children Trigger 2",
                        Action = new IeGenerateAction
                        {
                            GenerateActionType = GenerateActionTypes.Children,
                            ArtifactType = "Textual Requirement",
                            ChildCount = 3
                        }
                    },
                    new IeTrigger {
                        Name = "Generate User Stories Trigger 2",
                        Action = new IeGenerateAction
                        {
                            GenerateActionType = GenerateActionTypes.UserStories
                        }
                    },
                    new IeTrigger {
                        Name = "Generate Test Cases Trigger 2",
                        Action = new IeGenerateAction
                        {
                            GenerateActionType = GenerateActionTypes.TestCases
                        }
                    }
                }
                });

                workflow.NewArtifactEvents.Add(new IeNewArtifactEvent
                {
                    Name = "New Artifact 2",
                    Triggers = new List<IeTrigger>
                {
                    new IeTrigger {
                        Name = "Text Property Change Trigger 2",
                        Action = new IePropertyChangeAction
                        {
                            PropertyName = "Text Property 2",
                            PropertyValue = "New Property Value"
                        }
                    },
                    new IeTrigger {
                        Name = "Number Property Change Trigger 2",
                        Action = new IePropertyChangeAction
                        {
                            PropertyName = "Number Property",
                            PropertyValue = "10.56"
                        }
                    },
                    new IeTrigger {
                        Name = "Date Property Change Trigger 2",
                        Action = new IePropertyChangeAction
                        {
                            PropertyName = "Date Property",
                            PropertyValue = "2017-07-21"
                        }
                    },
                    new IeTrigger {
                        Name = "Choice Property Change Trigger 2",
                        Action = new IePropertyChangeAction
                        {
                            PropertyName = "Choice Property",
                            PropertyValue = "Low"
                        }
                    },
                    new IeTrigger {
                        Name = "User Property Change Trigger 2",
                        Action = new IePropertyChangeAction
                        {
                            PropertyName = "User Property",
                            UsersGroups = new List<IeUserGroup>
                            {
                                new IeUserGroup
                                {
                                    Name = "user"
                                },
                                new IeUserGroup
                                {
                                    Name = "instance group",
                                    IsGroup = true
                                },
                                new IeUserGroup
                                {
                                    Name = "project group by id",
                                    IsGroup = true,
                                    GroupProjectId = 99
                                },
                                new IeUserGroup
                                {
                                    Name = "project group by path",
                                    IsGroup = true,
                                    GroupProjectPath = "path"
                                },
                                new IeUserGroup
                                {
                                    Name = "project group by path",
                                    IsGroup = true,
                                    GroupProjectPath = "path2"
                                }
                            }
                        }
                    }
                }
                });

                return workflow;
            }
        }
    }
}