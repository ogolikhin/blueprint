using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BlueprintSys.RC.Services.MessageHandlers.ArtifactPublished;
using BlueprintSys.RC.Services.Models;
using BlueprintSys.RC.Services.Repositories;
using BluePrintSys.Messaging.CrossCutting.Helpers;
using BluePrintSys.Messaging.Models.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Models.Workflow.Actions;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;
using ServiceLibrary.Repositories.Workflow;

namespace BlueprintSys.RC.Services.Tests.MessageHandlers.ArtifactPublished
{
    [TestClass]
    public class UpdatedArtifactsNotificationHandlerTests
    {
        private int _userId;
        private int _revisionId;
        private int _transitionType;
        private int _artifactId;
        private int _projectId;
        private string _projectName;

        private TenantInformation _tenant;
        private Mock<IArtifactsPublishedRepository> _artifactsPublishedRepositoryMock;
        private Mock<IActionsParser> _actionParserMock;
        private Mock<IWorkflowRepository> _workflowRepoMock;
        private Mock<IUsersRepository> _userRepoMock;
        private Mock<IServiceLogRepository> _serviceLogRepositoryMock;
        private Mock<IWorkflowMessagingProcessor> _wfMessagingMock;

        [TestInitialize]
        public void TestInitialize()
        {
            _userId = 2;
            _revisionId = 2548;
            _artifactId = 5;
            _projectId = 6;
            _projectName = "Test Project";

            _transitionType = (int) TransitionType.Property;

            _tenant = new TenantInformation
            {
                TenantId = "Blueprint",
                PackageName = "Professional",
                TenantName = "Blueprint",
                BlueprintConnectionString = "DBConnection",
                AdminStoreLog = "localhost",
                ExpirationDate = DateTime.MaxValue,
                PackageLevel = 2,
                StartDate = DateTime.Today
            };
            _artifactsPublishedRepositoryMock = new Mock<IArtifactsPublishedRepository>(MockBehavior.Strict);
            _actionParserMock = new Mock<IActionsParser>(MockBehavior.Strict);
            _serviceLogRepositoryMock = new Mock<IServiceLogRepository>(MockBehavior.Loose);
            _workflowRepoMock = new Mock<IWorkflowRepository>(MockBehavior.Loose);
            _userRepoMock = new Mock<IUsersRepository>(MockBehavior.Loose);
            _wfMessagingMock = new Mock<IWorkflowMessagingProcessor>(MockBehavior.Loose);

            _artifactsPublishedRepositoryMock.Setup(t => t.WorkflowRepository).Returns(_workflowRepoMock.Object);
            _artifactsPublishedRepositoryMock.Setup(t => t.UsersRepository).Returns(_userRepoMock.Object);
        }

        [TestMethod]
        public async Task ProcessUpdatedArtifacts_MessageIsNull_DoesNotProcessMessage()
        {
            //Arrange

            //Act
            var result = await UpdatedArtifactsNotificationHandler.ProcessUpdatedArtifacts(_tenant,
                null,
                _artifactsPublishedRepositoryMock.Object,
                _serviceLogRepositoryMock.Object,
                _actionParserMock.Object,
                _wfMessagingMock.Object
                );

            //Assert
            Assert.IsFalse(result, "Message should not be processed successfully");
        }

        [TestMethod]
        public async Task ProcessUpdatedArtifacts_NoUpdatedArtifacts_DoesNotProcessMessage()
        {
            //Arrange

            //Act
            var result = await UpdatedArtifactsNotificationHandler.ProcessUpdatedArtifacts(_tenant,
                new ArtifactsPublishedMessage()
                {
                    RevisionId = _revisionId,
                    UserId = _userId,
                    UserName = "admin",
                    Artifacts = new List<PublishedArtifactInformation>
                    {
                        new PublishedArtifactInformation
                        {
                            ModifiedProperties = new List<PublishedPropertyInformation>(),
                            IsFirstTimePublished = true
                        }
                    }
                }, 
                _artifactsPublishedRepositoryMock.Object,
                _serviceLogRepositoryMock.Object,
                _actionParserMock.Object,
                _wfMessagingMock.Object
                );

            //Assert
            Assert.IsFalse(result, "Message should not be processed successfully");
        }

        [TestMethod]
        public async Task ProcessUpdatedArtifacts_NoModifiedPropertiesAvailable_DoesNotProcessMessage()
        {
            //Arrange

            //Act
            var result = await UpdatedArtifactsNotificationHandler.ProcessUpdatedArtifacts(_tenant,
                new ArtifactsPublishedMessage()
                {
                    RevisionId = _revisionId,
                    UserId = _userId,
                    UserName = "admin",
                    Artifacts = new List<PublishedArtifactInformation>
                    {
                        new PublishedArtifactInformation
                        {
                            ModifiedProperties = new List<PublishedPropertyInformation>(),
                        }
                    }
                },
                _artifactsPublishedRepositoryMock.Object,
                _serviceLogRepositoryMock.Object,
                _actionParserMock.Object,
                _wfMessagingMock.Object
                );

            //Assert
            Assert.IsFalse(result, "Message should not be processed successfully");
        }

        [TestMethod]
        public async Task ProcessUpdatedArtifacts_NoPropertyTransitionEventAvailable_ProcessMessage()
        {
            //Arrange
            _artifactsPublishedRepositoryMock.Setup(t => t.GetWorkflowPropertyTransitionsForArtifactsAsync(_userId,
                _revisionId,
                _transitionType,
                It.IsAny<IEnumerable<int>>())).ReturnsAsync(new List<SqlWorkflowEvent>());

            //Act
            var result = await UpdatedArtifactsNotificationHandler.ProcessUpdatedArtifacts(_tenant,
                new ArtifactsPublishedMessage()
                {
                    RevisionId = _revisionId,
                    UserId = _userId,
                    UserName = "admin",
                    Artifacts = new List<PublishedArtifactInformation>
                    {
                        new PublishedArtifactInformation
                        {
                            ModifiedProperties = new List<PublishedPropertyInformation>
                            {
                                new PublishedPropertyInformation
                                {
                                    TypeId = 45,
                                    PredefinedType = (int)PropertyTypePredefined.CustomGroup
                                }
                            }
                        }
                    }
                },
                _artifactsPublishedRepositoryMock.Object,
                _serviceLogRepositoryMock.Object,
                _actionParserMock.Object,
                _wfMessagingMock.Object
                );

            //Assert
            Assert.IsFalse(result, "Message should not be processed successfully");
        }

        [TestMethod]
        public async Task ProcessUpdatedArtifacts_NoWorkflowStatesAvailable_ProcessMessage()
        {
            //Arrange
            int propertyTypeId = 45;
            _artifactsPublishedRepositoryMock.Setup(t => t.GetWorkflowPropertyTransitionsForArtifactsAsync(_userId,
                _revisionId,
                _transitionType,
                It.IsAny<IEnumerable<int>>())).ReturnsAsync(new List<SqlWorkflowEvent>
                {
                    new SqlWorkflowEvent
                    {
                        EventType = (int)TransitionType.Property,
                        VersionItemId = _artifactId,
                        EventPropertyTypeId = propertyTypeId
                    }
                });
            _artifactsPublishedRepositoryMock.Setup(t => t.GetWorkflowStatesForArtifactsAsync(_userId,
                It.IsAny<IEnumerable<int>>(),
                _revisionId,
                true)).ReturnsAsync(new List<SqlWorkFlowStateInformation>());
            
            //Act
            var result = await UpdatedArtifactsNotificationHandler.ProcessUpdatedArtifacts(_tenant,
                new ArtifactsPublishedMessage()
                {
                    RevisionId = _revisionId,
                    UserId = _userId,
                    UserName = "admin",
                    Artifacts = new List<PublishedArtifactInformation>
                    {
                        new PublishedArtifactInformation
                        {
                            Id = _artifactId,
                            ModifiedProperties = new List<PublishedPropertyInformation>
                            {
                                new PublishedPropertyInformation
                                {
                                    TypeId = propertyTypeId,
                                    PredefinedType = (int)PropertyTypePredefined.CustomGroup
                                }
                            }
                        }
                    }
                },
                _artifactsPublishedRepositoryMock.Object,
                _serviceLogRepositoryMock.Object,
                _actionParserMock.Object,
                _wfMessagingMock.Object
                );

            //Assert
            Assert.IsFalse(result, "Message should not be processed successfully");
        }

        [TestMethod]
        public async Task ProcessUpdatedArtifacts_NoEmailNotificationAvailable_ProcessMessage()
        {
            //Arrange
            int propertyTypeId = 45;
            int workflowId = 46;
            int workflowStateId = 47;

            _artifactsPublishedRepositoryMock.Setup(t => t.GetWorkflowPropertyTransitionsForArtifactsAsync(_userId,
                _revisionId,
                _transitionType,
                It.IsAny<IEnumerable<int>>())).ReturnsAsync(new List<SqlWorkflowEvent>
                {
                    new SqlWorkflowEvent
                    {
                        EventType = (int)TransitionType.Property,
                        VersionItemId = _artifactId,
                        EventPropertyTypeId = propertyTypeId
                    }
                });
            _artifactsPublishedRepositoryMock.Setup(t => t.GetWorkflowStatesForArtifactsAsync(_userId,
                It.IsAny<IEnumerable<int>>(),
                _revisionId,
                true)).ReturnsAsync(new List<SqlWorkFlowStateInformation>
                {
                    new SqlWorkFlowStateInformation
                    {
                        ArtifactId = _artifactId,
                        WorkflowId = workflowId,
                        WorkflowStateId = workflowStateId
                    }
                });
            _artifactsPublishedRepositoryMock.Setup(t => t.GetProjectNameByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(
                    new List<SqlProject>
                    {
                        new SqlProject
                        {
                            ItemId = _projectId,
                            Name = _projectName
                        }
                    });
            _actionParserMock.Setup(t => t.GetNotificationActions(It.IsAny<IEnumerable<SqlWorkflowEvent>>())).
                Returns(new List<EmailNotificationAction>());

            //Act
            var result = await UpdatedArtifactsNotificationHandler.ProcessUpdatedArtifacts(_tenant,
                new ArtifactsPublishedMessage
                {
                    RevisionId = _revisionId,
                    UserId = _userId,
                    UserName = "admin",
                    Artifacts = new List<PublishedArtifactInformation>
                    {
                        new PublishedArtifactInformation
                        {
                            Id = _artifactId,
                            ProjectId = _projectId,
                            ModifiedProperties = new List<PublishedPropertyInformation>
                            {
                                new PublishedPropertyInformation
                                {
                                    TypeId = propertyTypeId,
                                    PredefinedType = (int)PropertyTypePredefined.CustomGroup
                                }
                            }
                        }
                    }
                },
                _artifactsPublishedRepositoryMock.Object,
                _serviceLogRepositoryMock.Object,
                _actionParserMock.Object,
                _wfMessagingMock.Object
                );

            //Assert
            Assert.IsFalse(result, "Message should not be processed successfully");
        }

        [TestMethod]
        public async Task ProcessUpdatedArtifacts_ReturnsFalse_WhenNoEmailNotificationActionHasEventPropertyTypeIdForSystemProperty()
        {
            //Arrange
            int propertyTypeId = 45;
            int workflowId = 46;
            int workflowStateId = 47;

            _artifactsPublishedRepositoryMock.Setup(t => t.GetWorkflowPropertyTransitionsForArtifactsAsync(_userId, _revisionId, _transitionType, It.IsAny<IEnumerable<int>>())).ReturnsAsync(
                new List<SqlWorkflowEvent>
                {
                    new SqlWorkflowEvent
                    {
                        EventType = (int) TransitionType.Property,
                        VersionItemId = _artifactId,
                        EventPropertyTypeId = propertyTypeId
                    }
                });
            _artifactsPublishedRepositoryMock.Setup(t => t.GetWorkflowStatesForArtifactsAsync(_userId, It.IsAny<IEnumerable<int>>(), _revisionId, true)).ReturnsAsync(
                new List<SqlWorkFlowStateInformation>
                {
                    new SqlWorkFlowStateInformation
                    {
                        ArtifactId = _artifactId,
                        WorkflowId = workflowId,
                        WorkflowStateId = workflowStateId
                    }
                });
            _artifactsPublishedRepositoryMock.Setup(t => t.GetProjectNameByIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(
                new List<SqlProject>
                {
                    new SqlProject
                    {
                        ItemId = _projectId,
                        Name = _projectName
                    }
                });

            var actionsWithoutEventPropertyTypeId = new List<EmailNotificationAction>
            {
                new EmailNotificationAction
                {
                    EventPropertyTypeId = null
                }
            };
            _actionParserMock.Setup(t => t.GetNotificationActions(It.IsAny<IEnumerable<SqlWorkflowEvent>>())).Returns(actionsWithoutEventPropertyTypeId);

            const PropertyTypePredefined propertyTypePredefined = PropertyTypePredefined.Name;

            //Act
            var result = await UpdatedArtifactsNotificationHandler.ProcessUpdatedArtifacts(
                _tenant,
                new ArtifactsPublishedMessage
                {
                    RevisionId = _revisionId,
                    UserId = _userId,
                    UserName = "admin",
                    Artifacts = new List<PublishedArtifactInformation>
                    {
                        new PublishedArtifactInformation
                        {
                            Id = _artifactId,
                            ProjectId = _projectId,
                            ModifiedProperties = new List<PublishedPropertyInformation>
                            {
                                new PublishedPropertyInformation
                                {
                                    TypeId = propertyTypeId,
                                    PredefinedType = (int) propertyTypePredefined
                                }
                            }
                        }
                    }
                },
                _artifactsPublishedRepositoryMock.Object,
                _serviceLogRepositoryMock.Object,
                _actionParserMock.Object,
                _wfMessagingMock.Object);

            //Assert
            Assert.IsFalse(result, "Message should not be processed successfully");
        }

        [TestMethod]
        public async Task ProcessUpdatedArtifacts_ReturnsFalse_WhenNoEmailNotificationActionHasEventPropertyTypeIdForCustomProperty()
        {
            //Arrange
            int propertyTypeId = 45;
            int workflowId = 46;
            int workflowStateId = 47;

            _artifactsPublishedRepositoryMock.Setup(t => t.GetWorkflowPropertyTransitionsForArtifactsAsync(_userId, _revisionId, _transitionType, It.IsAny<IEnumerable<int>>())).ReturnsAsync(
                new List<SqlWorkflowEvent>
                {
                    new SqlWorkflowEvent
                    {
                        EventType = (int) TransitionType.Property,
                        VersionItemId = _artifactId,
                        EventPropertyTypeId = propertyTypeId
                    }
                });
            _artifactsPublishedRepositoryMock.Setup(t => t.GetWorkflowStatesForArtifactsAsync(_userId, It.IsAny<IEnumerable<int>>(), _revisionId, true)).ReturnsAsync(
                new List<SqlWorkFlowStateInformation>
                {
                    new SqlWorkFlowStateInformation
                    {
                        ArtifactId = _artifactId,
                        WorkflowId = workflowId,
                        WorkflowStateId = workflowStateId
                    }
                });
            _artifactsPublishedRepositoryMock.Setup(t => t.GetProjectNameByIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(
                new List<SqlProject>
                {
                    new SqlProject
                    {
                        ItemId = _projectId,
                        Name = _projectName
                    }
                });
            _artifactsPublishedRepositoryMock.Setup(m => m.GetInstancePropertyTypeIdsMap(It.IsAny<IEnumerable<int>>())).ReturnsAsync(new Dictionary<int, List<int>>());

            var actionsWithoutEventPropertyTypeId = new List<EmailNotificationAction>
            {
                new EmailNotificationAction
                {
                    EventPropertyTypeId = null
                }
            };
            _actionParserMock.Setup(t => t.GetNotificationActions(It.IsAny<IEnumerable<SqlWorkflowEvent>>())).Returns(actionsWithoutEventPropertyTypeId);

            const PropertyTypePredefined propertyTypePredefined = PropertyTypePredefined.CustomGroup;

            //Act
            var result = await UpdatedArtifactsNotificationHandler.ProcessUpdatedArtifacts(
                _tenant,
                new ArtifactsPublishedMessage
                {
                    RevisionId = _revisionId,
                    UserId = _userId,
                    UserName = "admin",
                    Artifacts = new List<PublishedArtifactInformation>
                    {
                        new PublishedArtifactInformation
                        {
                            Id = _artifactId,
                            ProjectId = _projectId,
                            ModifiedProperties = new List<PublishedPropertyInformation>
                            {
                                new PublishedPropertyInformation
                                {
                                    TypeId = propertyTypeId,
                                    PredefinedType = (int) propertyTypePredefined
                                }
                            }
                        }
                    }
                },
                _artifactsPublishedRepositoryMock.Object,
                _serviceLogRepositoryMock.Object,
                _actionParserMock.Object,
                _wfMessagingMock.Object);

            //Assert
            Assert.IsFalse(result, "Message should not be processed successfully");
        }

        [TestMethod]
        public async Task ProcessUpdatedArtifacts_InstancePropertyTypeIdsDoesNotContainEvent_ProcessMessage()
        {
            //Arrange
            int eventPropertyTypeId = 45;
            int propertyTypeId = 45;
            int workflowId = 46;
            int workflowStateId = 47;

            _artifactsPublishedRepositoryMock.Setup(t => t.GetWorkflowPropertyTransitionsForArtifactsAsync(_userId,
                _revisionId,
                _transitionType,
                It.IsAny<IEnumerable<int>>())).ReturnsAsync(new List<SqlWorkflowEvent>
                {
                    new SqlWorkflowEvent
                    {
                        EventType = (int)TransitionType.Property,
                        VersionItemId = _artifactId,
                        EventPropertyTypeId = propertyTypeId
                    }
                });
            _artifactsPublishedRepositoryMock.Setup(t => t.GetWorkflowStatesForArtifactsAsync(_userId,
                It.IsAny<IEnumerable<int>>(),
                _revisionId,
                true)).ReturnsAsync(new List<SqlWorkFlowStateInformation>
                {
                    new SqlWorkFlowStateInformation
                    {
                        ArtifactId = _artifactId,
                        WorkflowId = workflowId,
                        WorkflowStateId = workflowStateId
                    }
                });
            _artifactsPublishedRepositoryMock.Setup(t => t.GetProjectNameByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(
                    new List<SqlProject>
                    {
                        new SqlProject
                        {
                            ItemId = _projectId,
                            Name = _projectName
                        }
                    });
            _actionParserMock.Setup(t => t.GetNotificationActions(It.IsAny<IEnumerable<SqlWorkflowEvent>>())).
                Returns(new List<EmailNotificationAction>
                {
                    new EmailNotificationAction
                    {
                        Emails = { "test@blueprintsys.com"},
                        Message = "My message",
                        EventPropertyTypeId = eventPropertyTypeId
                    }
                });
            _artifactsPublishedRepositoryMock.Setup(t => t.GetInstancePropertyTypeIdsMap(It.IsAny<IEnumerable<int>>())).
                ReturnsAsync(new Dictionary<int, List<int>>()
                {
                    { propertyTypeId, new List<int>() }
                });

            //Act
            var result = await UpdatedArtifactsNotificationHandler.ProcessUpdatedArtifacts(_tenant,
                new ArtifactsPublishedMessage
                {
                    RevisionId = _revisionId,
                    UserId = _userId,
                    UserName = "admin",
                    Artifacts = new List<PublishedArtifactInformation>
                    {
                        new PublishedArtifactInformation
                        {
                            Id = _artifactId,
                            ProjectId = _projectId,
                            ModifiedProperties = new List<PublishedPropertyInformation>
                            {
                                new PublishedPropertyInformation
                                {
                                    TypeId = propertyTypeId,
                                    PredefinedType = (int)PropertyTypePredefined.CustomGroup
                                }
                            }
                        }
                    }
                },
                _artifactsPublishedRepositoryMock.Object,
                _serviceLogRepositoryMock.Object,
                _actionParserMock.Object,
                _wfMessagingMock.Object
                );

            //Assert
            Assert.IsFalse(result, "Message should not be processed successfully");
        }

        [TestMethod]
        public async Task ProcessUpdatedArtifacts_ConditionalStateIdDoesNotMatchWorkflowStateId_ProcessMessage()
        {
            //Arrange
            int eventPropertyTypeId = 45;
            int propertyTypeId = 45;
            int workflowId = 46;
            int workflowStateId = 47;
            int conditionalStateId = 48;

            _artifactsPublishedRepositoryMock.Setup(t => t.GetWorkflowPropertyTransitionsForArtifactsAsync(_userId,
                _revisionId,
                _transitionType,
                It.IsAny<IEnumerable<int>>())).ReturnsAsync(new List<SqlWorkflowEvent>
                {
                    new SqlWorkflowEvent
                    {
                        EventType = (int)TransitionType.Property,
                        VersionItemId = _artifactId,
                        EventPropertyTypeId = propertyTypeId
                    }
                });
            _artifactsPublishedRepositoryMock.Setup(t => t.GetWorkflowStatesForArtifactsAsync(_userId,
                It.IsAny<IEnumerable<int>>(),
                _revisionId,
                true)).ReturnsAsync(new List<SqlWorkFlowStateInformation>
                {
                    new SqlWorkFlowStateInformation
                    {
                        ArtifactId = _artifactId,
                        WorkflowId = workflowId,
                        WorkflowStateId = workflowStateId
                    }
                });
            _artifactsPublishedRepositoryMock.Setup(t => t.GetProjectNameByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(
                    new List<SqlProject>
                    {
                        new SqlProject
                        {
                            ItemId = _projectId,
                            Name = _projectName
                        }
                    });
            _actionParserMock.Setup(t => t.GetNotificationActions(It.IsAny<IEnumerable<SqlWorkflowEvent>>())).
                Returns(new List<EmailNotificationAction>
                {
                    new EmailNotificationAction
                    {
                        Emails = { "test@blueprintsys.com"},
                        Message = "My message",
                        EventPropertyTypeId = eventPropertyTypeId,
                        ConditionalStateId = conditionalStateId
                    }
                });
            _artifactsPublishedRepositoryMock.Setup(t => t.GetInstancePropertyTypeIdsMap(It.IsAny<IEnumerable<int>>())).
                ReturnsAsync(new Dictionary<int, List<int>>()
                {
                    {
                        propertyTypeId, new List<int>
                        {
                            propertyTypeId
                        }
                    }
                });

            //Act
            var result = await UpdatedArtifactsNotificationHandler.ProcessUpdatedArtifacts(_tenant,
                new ArtifactsPublishedMessage
                {
                    RevisionId = _revisionId,
                    UserId = _userId,
                    UserName = "admin",
                    Artifacts = new List<PublishedArtifactInformation>
                    {
                        new PublishedArtifactInformation
                        {
                            Id = _artifactId,
                            ProjectId = _projectId,
                            ModifiedProperties = new List<PublishedPropertyInformation>
                            {
                                new PublishedPropertyInformation
                                {
                                    TypeId = propertyTypeId,
                                    PredefinedType = (int)PropertyTypePredefined.CustomGroup
                                }
                            }
                        }
                    }
                },
                _artifactsPublishedRepositoryMock.Object,
                _serviceLogRepositoryMock.Object,
                _actionParserMock.Object,
                _wfMessagingMock.Object
                );

            //Assert
            Assert.IsFalse(result, "Message should not be processed successfully");
        }


        [TestMethod]
        public async Task ProcessUpdatedArtifacts_MessageWithNameSystemPropertyAndConditionalStateIdDoesNotMatchWorkflowStateId_ReturnsFalse()
        {
            //Arrange
            int propertyTypeId = 45;
            int workflowId = 46;
            int workflowStateId = 47;
            int conditionalStateId = 48;

            _artifactsPublishedRepositoryMock.Setup(t => t.GetWorkflowPropertyTransitionsForArtifactsAsync(_userId,
                _revisionId,
                _transitionType,
                It.IsAny<IEnumerable<int>>())).ReturnsAsync(new List<SqlWorkflowEvent>
                {
                    new SqlWorkflowEvent
                    {
                        EventType = (int)TransitionType.Property,
                        VersionItemId = _artifactId,
                        EventPropertyTypeId = WorkflowConstants.PropertyTypeFakeIdName
                    }
                });
            _artifactsPublishedRepositoryMock.Setup(t => t.GetWorkflowStatesForArtifactsAsync(_userId,
                It.IsAny<IEnumerable<int>>(),
                _revisionId,
                true)).ReturnsAsync(new List<SqlWorkFlowStateInformation>
                {
                    new SqlWorkFlowStateInformation
                    {
                        ArtifactId = _artifactId,
                        WorkflowId = workflowId,
                        WorkflowStateId = workflowStateId
                    }
                });
            _artifactsPublishedRepositoryMock.Setup(t => t.GetProjectNameByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(
                    new List<SqlProject>
                    {
                        new SqlProject
                        {
                            ItemId = _projectId,
                            Name = _projectName
                        }
                    });
            _actionParserMock.Setup(t => t.GetNotificationActions(It.IsAny<IEnumerable<SqlWorkflowEvent>>())).
                Returns(new List<EmailNotificationAction>
                {
                    new EmailNotificationAction
                    {
                        Emails = { "test@blueprintsys.com"},
                        Message = "My message",
                        EventPropertyTypeId = WorkflowConstants.PropertyTypeFakeIdName,
                        ConditionalStateId = conditionalStateId
                    }
                });
            

            //Act
            var result = await UpdatedArtifactsNotificationHandler.ProcessUpdatedArtifacts(_tenant,
                new ArtifactsPublishedMessage
                {
                    RevisionId = _revisionId,
                    UserId = _userId,
                    UserName = "admin",
                    Artifacts = new List<PublishedArtifactInformation>
                    {
                        new PublishedArtifactInformation
                        {
                            Id = _artifactId,
                            ProjectId = _projectId,
                            ModifiedProperties = new List<PublishedPropertyInformation>
                            {
                                new PublishedPropertyInformation
                                {
                                    TypeId = propertyTypeId,
                                    PredefinedType = (int)PropertyTypePredefined.Name
                                }
                            }
                        }
                    }
                },
                _artifactsPublishedRepositoryMock.Object,
                _serviceLogRepositoryMock.Object,
                _actionParserMock.Object,
                _wfMessagingMock.Object
                );

            //Assert
            Assert.IsFalse(result, "Message should not be processed successfully");
        }

        [TestMethod]
        public async Task ProcessUpdatedArtifacts_MessageWithNameSystemPropertyAndConditionalStateIdMatchWorkflowStateId_ReturnsTrue()
        {
            //Arrange
            int propertyTypeId = 45;
            int workflowId = 46;
            int workflowStateId = 48;
            int conditionalStateId = 48;

            _artifactsPublishedRepositoryMock.Setup(t => t.GetWorkflowPropertyTransitionsForArtifactsAsync(_userId,
                _revisionId,
                _transitionType,
                It.IsAny<IEnumerable<int>>())).ReturnsAsync(new List<SqlWorkflowEvent>
                {
                    new SqlWorkflowEvent
                    {
                        EventType = (int)TransitionType.Property,
                        VersionItemId = _artifactId,
                        EventPropertyTypeId = WorkflowConstants.PropertyTypeFakeIdName
                    }
                });
            _artifactsPublishedRepositoryMock.Setup(t => t.GetWorkflowStatesForArtifactsAsync(_userId,
                It.IsAny<IEnumerable<int>>(),
                _revisionId,
                true)).ReturnsAsync(new List<SqlWorkFlowStateInformation>
                {
                    new SqlWorkFlowStateInformation
                    {
                        ArtifactId = _artifactId,
                        WorkflowId = workflowId,
                        WorkflowStateId = workflowStateId
                    }
                });
            _artifactsPublishedRepositoryMock.Setup(t => t.GetProjectNameByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(
                    new List<SqlProject>
                    {
                        new SqlProject
                        {
                            ItemId = _projectId,
                            Name = _projectName
                        }
                    });
            _actionParserMock.Setup(t => t.GetNotificationActions(It.IsAny<IEnumerable<SqlWorkflowEvent>>())).
                Returns(new List<EmailNotificationAction>
                {
                    new EmailNotificationAction
                    {
                        Emails = { "test@blueprintsys.com"},
                        Message = "My message",
                        EventPropertyTypeId = WorkflowConstants.PropertyTypeFakeIdName,
                        ConditionalStateId = conditionalStateId
                    }
                });


            //Act
            var result = await UpdatedArtifactsNotificationHandler.ProcessUpdatedArtifacts(_tenant,
                new ArtifactsPublishedMessage
                {
                    RevisionId = _revisionId,
                    UserId = _userId,
                    UserName = "admin",
                    Artifacts = new List<PublishedArtifactInformation>
                    {
                        new PublishedArtifactInformation
                        {
                            Id = _artifactId,
                            ProjectId = _projectId,
                            ModifiedProperties = new List<PublishedPropertyInformation>
                            {
                                new PublishedPropertyInformation
                                {
                                    TypeId = propertyTypeId,
                                    PredefinedType = (int)PropertyTypePredefined.Name
                                }
                            }
                        }
                    }
                },
                _artifactsPublishedRepositoryMock.Object,
                _serviceLogRepositoryMock.Object,
                _actionParserMock.Object,
                _wfMessagingMock.Object
                );

            //Assert
            Assert.IsTrue(result, "Message should be processed successfully");
        }

        [TestMethod]
        public async Task ProcessUpdatedArtifacts_MessageWithDescriptionSystemPropertyAndConditionalStateIdMatchWorkflowStateId_ReturnsTrue()
        {
            //Arrange
            const int propertyTypeId = 33;
            const int workflowId = 34;
            const int workflowStateId = 35;
            const int conditionalStateId = workflowStateId;

            _artifactsPublishedRepositoryMock.Setup(t => t.GetWorkflowPropertyTransitionsForArtifactsAsync(_userId, _revisionId, _transitionType, It.IsAny<IEnumerable<int>>())).ReturnsAsync(
                new List<SqlWorkflowEvent>
                {
                    new SqlWorkflowEvent
                    {
                        EventType = (int) TransitionType.Property,
                        VersionItemId = _artifactId,
                        EventPropertyTypeId = WorkflowConstants.PropertyTypeFakeIdDescription
                    }
                });
            _artifactsPublishedRepositoryMock.Setup(t => t.GetWorkflowStatesForArtifactsAsync(_userId, It.IsAny<IEnumerable<int>>(), _revisionId, true)).ReturnsAsync(
                new List<SqlWorkFlowStateInformation>
                {
                    new SqlWorkFlowStateInformation
                    {
                        ArtifactId = _artifactId,
                        WorkflowId = workflowId,
                        WorkflowStateId = workflowStateId
                    }
                });
            _artifactsPublishedRepositoryMock.Setup(t => t.GetProjectNameByIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(
                new List<SqlProject>
                {
                    new SqlProject
                    {
                        ItemId = _projectId,
                        Name = _projectName
                    }
                });
            _actionParserMock.Setup(t => t.GetNotificationActions(It.IsAny<IEnumerable<SqlWorkflowEvent>>())).Returns(
                new List<EmailNotificationAction>
                {
                    new EmailNotificationAction
                    {
                        Emails =
                        {
                            "test@blueprintsys.com"
                        },
                        Message = "My message",
                        EventPropertyTypeId = WorkflowConstants.PropertyTypeFakeIdDescription,
                        ConditionalStateId = conditionalStateId
                    }
                });

            //Act
            var result = await UpdatedArtifactsNotificationHandler.ProcessUpdatedArtifacts(
                _tenant,
                new ArtifactsPublishedMessage
                {
                    RevisionId = _revisionId,
                    UserId = _userId,
                    UserName = "admin",
                    Artifacts = new List<PublishedArtifactInformation>
                    {
                        new PublishedArtifactInformation
                        {
                            Id = _artifactId,
                            ProjectId = _projectId,
                            ModifiedProperties = new List<PublishedPropertyInformation>
                            {
                                new PublishedPropertyInformation
                                {
                                    TypeId = propertyTypeId,
                                    PredefinedType = (int) PropertyTypePredefined.Description
                                }
                            }
                        }
                    }
                },
                _artifactsPublishedRepositoryMock.Object,
                _serviceLogRepositoryMock.Object,
                _actionParserMock.Object,
                _wfMessagingMock.Object);

            //Assert
            Assert.IsTrue(result, "Message with a Description system property should be processed successfully");
        }

        [TestMethod]
        public async Task ProcessUpdatedArtifacts_AllEvaluationsSuccessful_ProcessMessage()
        {
            //Arrange
            int eventPropertyTypeId = 45;
            int propertyTypeId = 45;
            int workflowId = 46;
            int workflowStateId = 47;
            int conditionalStateId = 47;

            _artifactsPublishedRepositoryMock.Setup(t => t.GetWorkflowPropertyTransitionsForArtifactsAsync(_userId,
                _revisionId,
                _transitionType,
                It.IsAny<IEnumerable<int>>())).ReturnsAsync(new List<SqlWorkflowEvent>
                {
                    new SqlWorkflowEvent
                    {
                        EventType = (int)TransitionType.Property,
                        VersionItemId = _artifactId,
                        EventPropertyTypeId = propertyTypeId
                    }
                });
            _artifactsPublishedRepositoryMock.Setup(t => t.GetWorkflowStatesForArtifactsAsync(_userId,
                It.IsAny<IEnumerable<int>>(),
                _revisionId,
                true)).ReturnsAsync(new List<SqlWorkFlowStateInformation>
                {
                    new SqlWorkFlowStateInformation
                    {
                        ArtifactId = _artifactId,
                        WorkflowId = workflowId,
                        WorkflowStateId = workflowStateId
                    }
                });
            _artifactsPublishedRepositoryMock.Setup(t => t.GetProjectNameByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(
                    new List<SqlProject>
                    {
                        new SqlProject
                        {
                            ItemId = _projectId,
                            Name = _projectName
                        }
                    });
            _actionParserMock.Setup(t => t.GetNotificationActions(It.IsAny<IEnumerable<SqlWorkflowEvent>>())).
                Returns(new List<EmailNotificationAction>
                {
                    new EmailNotificationAction
                    {
                        Emails = { "test@blueprintsys.com"},
                        Message = "My message",
                        EventPropertyTypeId = eventPropertyTypeId,
                        ConditionalStateId = conditionalStateId
                    }
                });
            _artifactsPublishedRepositoryMock.Setup(t => t.GetInstancePropertyTypeIdsMap(It.IsAny<IEnumerable<int>>())).
                ReturnsAsync(new Dictionary<int, List<int>>()
                {
                    {
                        propertyTypeId, new List<int>
                        {
                            propertyTypeId
                        }
                    }
                });

            //Act
            var result = await UpdatedArtifactsNotificationHandler.ProcessUpdatedArtifacts(_tenant,
                new ArtifactsPublishedMessage
                {
                    RevisionId = _revisionId,
                    UserId = _userId,
                    UserName = "admin",
                    Artifacts = new List<PublishedArtifactInformation>
                    {
                        new PublishedArtifactInformation
                        {
                            Id = _artifactId,
                            ProjectId = _projectId,
                            BaseUrl = "localhost",
                            Url = "completeUrl",
                            Name = "Test",
                            IsFirstTimePublished = false,
                            Predefined = (int)ItemTypePredefined.Process,
                            ModifiedProperties = new List<PublishedPropertyInformation>
                            {
                                new PublishedPropertyInformation
                                {
                                    TypeId = propertyTypeId,
                                    PredefinedType = (int)PropertyTypePredefined.CustomGroup
                                }
                            }
                        }
                    }
                },
                _artifactsPublishedRepositoryMock.Object,
                _serviceLogRepositoryMock.Object,
                _actionParserMock.Object,
                _wfMessagingMock.Object
                );

            //Assert
            Assert.IsTrue(result, "Did not process message.");
        }
    }
}
