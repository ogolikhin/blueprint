using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.MessageHandlers.ArtifactPublished;
using BlueprintSys.RC.Services.Models;
using BlueprintSys.RC.Services.Repositories;
using BluePrintSys.Messaging.CrossCutting.Helpers;
using BluePrintSys.Messaging.CrossCutting.Models.Exceptions;
using BluePrintSys.Messaging.Models.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Models.Workflow.Actions;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;
using ServiceLibrary.Repositories.Workflow;

namespace BlueprintSys.RC.Services.Tests.MessageHandlers.ArtifactPublished
{
    [TestClass]
    public class CreatedArtifactsNotificationHandlerTests
    {
        private int _userId;
        private int _revisionId;
        private TenantInformation _tenant;
        private ArtifactsPublishedMessage _message;
        private List<WorkflowMessageArtifactInfo> _workflowMessageArtifactInfos;
        private Mock<IArtifactsPublishedRepository> _artifactsPublishedRepositoryMock;
        private Mock<IWorkflowRepository> _workflowRepoMock;
        private Mock<IUsersRepository> _userRepoMock;
        private Mock<IServiceLogRepository> _serviceLogRepositoryMock;
        private Mock<IWorkflowMessagingProcessor> _wfMessagingMock;

        private const int ProjectId = 1;
        private const int ArtifactId = 2;
        private const string ArtifactName = "New Artifact";
        private const ItemTypePredefined Predefined = ItemTypePredefined.Process;
        private const int ItemTypeId = 3;

        [TestInitialize]
        public void TestInitialize()
        {
            _userId = 2;
            _revisionId = 2548;

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

            _message = new ArtifactsPublishedMessage
            {
                RevisionId = _revisionId,
                UserId = _userId,
                UserName = "admin",
                Artifacts = new List<PublishedArtifactInformation>
                {
                    new PublishedArtifactInformation
                    {
                        Id = ArtifactId,
                        Name = ArtifactName,
                        ModifiedProperties = new List<PublishedPropertyInformation>(),
                        ProjectId = ProjectId,
                        IsFirstTimePublished = true,
                        Url = "localhost:id",
                        BaseUrl = "localhost",
                        Predefined = (int)Predefined
                    }
                }
            };

            _workflowMessageArtifactInfos = new List<WorkflowMessageArtifactInfo>
            {
                new WorkflowMessageArtifactInfo
                {
                    Id = ArtifactId,
                    ProjectName = "ProjectName",
                    ProjectId = ProjectId,
                    Name = ArtifactName,
                    PredefinedType = Predefined,
                    ItemTypeId = ItemTypeId
                }
            };

            _artifactsPublishedRepositoryMock = new Mock<IArtifactsPublishedRepository>(MockBehavior.Loose);
            _serviceLogRepositoryMock = new Mock<IServiceLogRepository>(MockBehavior.Loose);
            _workflowRepoMock = new Mock<IWorkflowRepository>(MockBehavior.Loose);
            _userRepoMock = new Mock<IUsersRepository>(MockBehavior.Loose);
            _wfMessagingMock = new Mock<IWorkflowMessagingProcessor>(MockBehavior.Loose);

            _artifactsPublishedRepositoryMock.Setup(t => t.WorkflowRepository).Returns(_workflowRepoMock.Object);
            _artifactsPublishedRepositoryMock.Setup(t => t.UsersRepository).Returns(_userRepoMock.Object);
        }

        [TestMethod]
        public async Task ProcessCreatedArtifacts_MessageIsNull_ReturnsFalse()
        {
            
            //Act
            var result = await CreatedArtifactsNotificationHandler.ProcessCreatedArtifacts(_tenant,
                null,
                _artifactsPublishedRepositoryMock.Object,
                _serviceLogRepositoryMock.Object,
                _wfMessagingMock.Object);

            //Assert
            Assert.IsFalse(result, "Message should not be processed successfully");
        }

        [TestMethod]
        public async Task ProcessCreatedArtifacts_NoCreatedArtifacts_ReturnsFalse()
        {
            var message = new ArtifactsPublishedMessage
            {
                RevisionId = _revisionId,
                UserId = _userId,
                UserName = "admin",
                Artifacts = new List<PublishedArtifactInformation>()
            };
            //Act
            var result = await CreatedArtifactsNotificationHandler.ProcessCreatedArtifacts(_tenant, 
                message, 
                _artifactsPublishedRepositoryMock.Object,
                _serviceLogRepositoryMock.Object,
                _wfMessagingMock.Object);

            //Assert
            Assert.IsFalse(result, "Message should not be processed successfully");
        }

        [ExpectedException(typeof(EntityNotFoundException))]
        [TestMethod]
        public async Task ProcessCreatedArtifacts_NoArtifactInfosRetrieved_ThrowsException()
        {
            //Arrange
            var message = new ArtifactsPublishedMessage
            {
                RevisionId = _revisionId,
                UserId = _userId,
                UserName = "admin",
                Artifacts = new List<PublishedArtifactInformation>
                {
                    new PublishedArtifactInformation
                    {
                        Id = 1,
                        Name = "A",
                        ModifiedProperties = new List<PublishedPropertyInformation>(),
                        ProjectId = 2,
                        IsFirstTimePublished = true,
                        Url = "localhost:id",
                        BaseUrl = "localhost",
                        Predefined = (int)ItemTypePredefined.Process
                    }
                }
            };
            _workflowRepoMock.Setup(t => t.GetWorkflowMessageArtifactInfoAsync(_userId,
                It.IsAny<IEnumerable<int>>(),
                _revisionId,
                It.IsAny<IDbTransaction>())).ReturnsAsync(Enumerable.Empty<WorkflowMessageArtifactInfo>());

            //Act
            await CreatedArtifactsNotificationHandler.ProcessCreatedArtifacts(_tenant,
                message,
                _artifactsPublishedRepositoryMock.Object,
                _serviceLogRepositoryMock.Object,
                _wfMessagingMock.Object,
                1);
        }

        [TestMethod]
        public async Task ProcessCreatedArtifacts_NoEventTriggersForArtifact_ReturnsFalseAndSendsNoMessages()
        {
            //Arrange
            _workflowRepoMock.Setup(t => t.GetWorkflowMessageArtifactInfoAsync(_userId,
                It.IsAny<IEnumerable<int>>(),
                _revisionId,
                It.IsAny<IDbTransaction>())).ReturnsAsync(_workflowMessageArtifactInfos);
            _workflowRepoMock.Setup(t => t.GetWorkflowEventTriggersForNewArtifactEvent(_userId,
                It.IsAny<IEnumerable<int>>(),
                _revisionId, true)).ReturnsAsync(new WorkflowTriggersContainer());

            //Act
           var result =  await CreatedArtifactsNotificationHandler.ProcessCreatedArtifacts(_tenant,
                _message,
                _artifactsPublishedRepositoryMock.Object,
                _serviceLogRepositoryMock.Object,
                _wfMessagingMock.Object);

            //Assert
            Assert.IsFalse(result, "No messages should be sent when there are no triggers");
            _wfMessagingMock.Verify(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>()), Times.Exactly(0));
        }

        [TestMethod]
        public async Task ProcessCreatedArtifacts_OnlySynchronousTriggersForArtifact_ReturnsFalseAndSendsNoMessages()
        {
            //Arrange
            _workflowRepoMock.Setup(t => t.GetWorkflowMessageArtifactInfoAsync(_userId, It.IsAny<IEnumerable<int>>(), _revisionId, It.IsAny<IDbTransaction>())).ReturnsAsync(_workflowMessageArtifactInfos);
            var workflowTriggersContainer = new WorkflowTriggersContainer();
            workflowTriggersContainer.SynchronousTriggers.Add(
                new WorkflowEventTrigger
                {
                    Name = "Property Change Trigger",
                    Condition = new WorkflowEventCondition(),
                    Action = new PropertyChangeAction()
                });
            _workflowRepoMock.Setup(t => t.GetWorkflowEventTriggersForNewArtifactEvent(_userId, It.IsAny<IEnumerable<int>>(), _revisionId, true)).ReturnsAsync(workflowTriggersContainer);

            //Act
            var result = await CreatedArtifactsNotificationHandler.ProcessCreatedArtifacts(_tenant, _message, _artifactsPublishedRepositoryMock.Object, _serviceLogRepositoryMock.Object, _wfMessagingMock.Object);

            //Assert
            Assert.IsFalse(result, "No messages should be sent for synchronous triggers");
            _wfMessagingMock.Verify(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>()), Times.Exactly(0));
        }

        [TestMethod]
        public async Task ProcessCreatedArtifacts_SendsSingleMessage_WhenSingleGenerateChildrenTriggerExistsForArtifact()
        {
            //Arrange
            _workflowRepoMock.Setup(t => t.GetWorkflowMessageArtifactInfoAsync(_userId,
                It.IsAny<IEnumerable<int>>(),
                _revisionId,
                It.IsAny<IDbTransaction>())).ReturnsAsync(_workflowMessageArtifactInfos);
            var workflowTriggersContainer = new WorkflowTriggersContainer();
            workflowTriggersContainer.AsynchronousTriggers.Add(new WorkflowEventTrigger
            {
                Name = "Trigger One",
                Condition = new WorkflowEventCondition(),
                Action = new GenerateChildrenAction
                {
                    ArtifactTypeId = ItemTypeId,
                    ChildCount = 3
                } 
            });
            _workflowRepoMock.Setup(t => t.GetWorkflowEventTriggersForNewArtifactEvent(_userId,
                It.IsAny<IEnumerable<int>>(),
                _revisionId, true)).ReturnsAsync(workflowTriggersContainer);

            //Act
            var result = await CreatedArtifactsNotificationHandler.ProcessCreatedArtifacts(_tenant,
                 _message,
                 _artifactsPublishedRepositoryMock.Object,
                 _serviceLogRepositoryMock.Object,
                 _wfMessagingMock.Object);

            //Assert
            Assert.IsTrue(result, "One message was sent successfully");
            _wfMessagingMock.Verify(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>()), Times.Exactly(1));
        }

        [TestMethod]
        public async Task ProcessCreatedArtifacts_SendsMultipleMessages_WhenMultipleGenerateChildrenTriggersExistForArtifact()
        {
            //Arrange
            _workflowRepoMock.Setup(t => t.GetWorkflowMessageArtifactInfoAsync(_userId, It.IsAny<IEnumerable<int>>(), _revisionId, It.IsAny<IDbTransaction>())).ReturnsAsync(_workflowMessageArtifactInfos);
            var workflowTriggersContainer = new WorkflowTriggersContainer();
            _workflowRepoMock.Setup(t => t.GetWorkflowEventTriggersForNewArtifactEvent(_userId, It.IsAny<IEnumerable<int>>(), _revisionId, true)).ReturnsAsync(workflowTriggersContainer);

            var workflowEventTriggers = new List<WorkflowEventTrigger>();
            for (int i = 0; i < 10; i++)
            {
                workflowEventTriggers.Add(
                    new WorkflowEventTrigger
                    {
                        Name = $"Generate Children Trigger {i}",
                        Condition = new WorkflowEventCondition(),
                        Action = new GenerateChildrenAction
                        {
                            ArtifactTypeId = ItemTypeId,
                            ChildCount = 2
                        }
                    });
            }
            workflowTriggersContainer.AsynchronousTriggers.AddRange(workflowEventTriggers);

            //Act
            var result = await CreatedArtifactsNotificationHandler.ProcessCreatedArtifacts(_tenant, _message, _artifactsPublishedRepositoryMock.Object, _serviceLogRepositoryMock.Object, _wfMessagingMock.Object);

            //Assert
            Assert.IsTrue(result, "Multiple messages were sent successfully");
            _wfMessagingMock.Verify(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>()), Times.Exactly(workflowEventTriggers.Count));
        }

        [TestMethod]
        public async Task ProcessCreatedArtifacts_SendsSingleMessage_WhenSingleGenerateTestCasesTriggerExistsForArtifact()
        {
            //Arrange
            _workflowRepoMock.Setup(t => t.GetWorkflowMessageArtifactInfoAsync(_userId, It.IsAny<IEnumerable<int>>(), _revisionId, It.IsAny<IDbTransaction>())).ReturnsAsync(_workflowMessageArtifactInfos);
            var workflowTriggersContainer = new WorkflowTriggersContainer();
            workflowTriggersContainer.AsynchronousTriggers.Add(
                new WorkflowEventTrigger
                {
                    Name = "Generate Test Cases Trigger",
                    Condition = new WorkflowEventCondition(),
                    Action = new GenerateTestCasesAction()
                });
            _workflowRepoMock.Setup(t => t.GetWorkflowEventTriggersForNewArtifactEvent(_userId, It.IsAny<IEnumerable<int>>(), _revisionId, true)).ReturnsAsync(workflowTriggersContainer);

            //Act
            var result = await CreatedArtifactsNotificationHandler.ProcessCreatedArtifacts(_tenant, _message, _artifactsPublishedRepositoryMock.Object, _serviceLogRepositoryMock.Object, _wfMessagingMock.Object);

            //Assert
            Assert.IsTrue(result, "One message was sent successfully");
            _wfMessagingMock.Verify(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>()), Times.Exactly(1));
        }

        [TestMethod]
        public async Task ProcessCreatedArtifacts_SendsMultipleMessages_WhenMultipleGenerateTestCasesTriggersExistForArtifact()
        {
            //Arrange
            _workflowRepoMock.Setup(t => t.GetWorkflowMessageArtifactInfoAsync(_userId, It.IsAny<IEnumerable<int>>(), _revisionId, It.IsAny<IDbTransaction>())).ReturnsAsync(_workflowMessageArtifactInfos);
            var workflowTriggersContainer = new WorkflowTriggersContainer();
            _workflowRepoMock.Setup(t => t.GetWorkflowEventTriggersForNewArtifactEvent(_userId, It.IsAny<IEnumerable<int>>(), _revisionId, true)).ReturnsAsync(workflowTriggersContainer);

            var workflowEventTriggers = new List<WorkflowEventTrigger>();
            for (int i = 0; i < 9; i++)
            {
                workflowEventTriggers.Add(
                    new WorkflowEventTrigger
                    {
                        Name = $"Generate Test Cases Trigger {i}",
                        Condition = new WorkflowEventCondition(),
                        Action = new GenerateTestCasesAction()
                    });
            }
            workflowTriggersContainer.AsynchronousTriggers.AddRange(workflowEventTriggers);

            //Act
            var result = await CreatedArtifactsNotificationHandler.ProcessCreatedArtifacts(_tenant, _message, _artifactsPublishedRepositoryMock.Object, _serviceLogRepositoryMock.Object, _wfMessagingMock.Object);

            //Assert
            Assert.IsTrue(result, "Multiple messages were sent successfully");
            _wfMessagingMock.Verify(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>()), Times.Exactly(workflowEventTriggers.Count));
        }

        [TestMethod]
        public async Task ProcessCreatedArtifacts_SendsSingleMessage_WhenSingleGenerateUserStoriesTriggerExistsForArtifact()
        {
            //Arrange
            _workflowRepoMock.Setup(t => t.GetWorkflowMessageArtifactInfoAsync(_userId, It.IsAny<IEnumerable<int>>(), _revisionId, It.IsAny<IDbTransaction>())).ReturnsAsync(_workflowMessageArtifactInfos);
            var workflowTriggersContainer = new WorkflowTriggersContainer();
            workflowTriggersContainer.AsynchronousTriggers.Add(
                new WorkflowEventTrigger
                {
                    Name = "Generate User Stories Trigger",
                    Condition = new WorkflowEventCondition(),
                    Action = new GenerateUserStoriesAction()
                });
            _workflowRepoMock.Setup(t => t.GetWorkflowEventTriggersForNewArtifactEvent(_userId, It.IsAny<IEnumerable<int>>(), _revisionId, true)).ReturnsAsync(workflowTriggersContainer);

            //Act
            var result = await CreatedArtifactsNotificationHandler.ProcessCreatedArtifacts(_tenant, _message, _artifactsPublishedRepositoryMock.Object, _serviceLogRepositoryMock.Object, _wfMessagingMock.Object);

            //Assert
            Assert.IsTrue(result, "One message was sent successfully");
            _wfMessagingMock.Verify(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>()), Times.Exactly(1));
        }

        [TestMethod]
        public async Task ProcessCreatedArtifacts_SendsMultipleMessages_WhenMultipleGenerateUserStoriesTriggersExistForArtifact()
        {
            //Arrange
            _workflowRepoMock.Setup(t => t.GetWorkflowMessageArtifactInfoAsync(_userId, It.IsAny<IEnumerable<int>>(), _revisionId, It.IsAny<IDbTransaction>())).ReturnsAsync(_workflowMessageArtifactInfos);
            var workflowTriggersContainer = new WorkflowTriggersContainer();
            _workflowRepoMock.Setup(t => t.GetWorkflowEventTriggersForNewArtifactEvent(_userId, It.IsAny<IEnumerable<int>>(), _revisionId, true)).ReturnsAsync(workflowTriggersContainer);

            var workflowEventTriggers = new List<WorkflowEventTrigger>();
            for (int i = 0; i < 8; i++)
            {
                workflowEventTriggers.Add(
                    new WorkflowEventTrigger
                    {
                        Name = $"Generate User Stories Trigger {i}",
                        Condition = new WorkflowEventCondition(),
                        Action = new GenerateUserStoriesAction()
                    });
            }
            workflowTriggersContainer.AsynchronousTriggers.AddRange(workflowEventTriggers);

            //Act
            var result = await CreatedArtifactsNotificationHandler.ProcessCreatedArtifacts(_tenant, _message, _artifactsPublishedRepositoryMock.Object, _serviceLogRepositoryMock.Object, _wfMessagingMock.Object);

            //Assert
            Assert.IsTrue(result, "Multiple messages were sent successfully");
            _wfMessagingMock.Verify(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>()), Times.Exactly(workflowEventTriggers.Count));
        }

        [TestMethod]
        public async Task ProcessCreatedArtifacts_SendsSingleMessage_WhenSingleEmailNotificationTriggerExistsForArtifact()
        {
            //Arrange
            _workflowRepoMock.Setup(t => t.GetWorkflowMessageArtifactInfoAsync(_userId, It.IsAny<IEnumerable<int>>(), _revisionId, It.IsAny<IDbTransaction>())).ReturnsAsync(_workflowMessageArtifactInfos);
            var workflowTriggersContainer = new WorkflowTriggersContainer();
            workflowTriggersContainer.AsynchronousTriggers.Add(
                new WorkflowEventTrigger
                {
                    Name = "Email Notification Trigger",
                    Condition = new WorkflowEventCondition(),
                    Action = new EmailNotificationAction()
                });
            _workflowRepoMock.Setup(t => t.GetWorkflowEventTriggersForNewArtifactEvent(_userId, It.IsAny<IEnumerable<int>>(), _revisionId, true)).ReturnsAsync(workflowTriggersContainer);

            //Act
            var result = await CreatedArtifactsNotificationHandler.ProcessCreatedArtifacts(_tenant, _message, _artifactsPublishedRepositoryMock.Object, _serviceLogRepositoryMock.Object, _wfMessagingMock.Object);

            //Assert
            Assert.IsTrue(result, "One message was sent successfully");
            _wfMessagingMock.Verify(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>()), Times.Exactly(1));
        }

        [TestMethod]
        public async Task ProcessCreatedArtifacts_SendsMultipleMessages_WhenMultipleEmailNotificationTriggersExistForArtifact()
        {
            //Arrange
            _workflowRepoMock.Setup(t => t.GetWorkflowMessageArtifactInfoAsync(_userId, It.IsAny<IEnumerable<int>>(), _revisionId, It.IsAny<IDbTransaction>())).ReturnsAsync(_workflowMessageArtifactInfos);
            var workflowTriggersContainer = new WorkflowTriggersContainer();
            _workflowRepoMock.Setup(t => t.GetWorkflowEventTriggersForNewArtifactEvent(_userId, It.IsAny<IEnumerable<int>>(), _revisionId, true)).ReturnsAsync(workflowTriggersContainer);

            var workflowEventTriggers = new List<WorkflowEventTrigger>();
            for (int i = 0; i < 7; i++)
            {
                workflowEventTriggers.Add(
                    new WorkflowEventTrigger
                    {
                        Name = $"Email Notification Trigger {i}",
                        Condition = new WorkflowEventCondition(),
                        Action = new EmailNotificationAction()
                    });
            }
            workflowTriggersContainer.AsynchronousTriggers.AddRange(workflowEventTriggers);

            //Act
            var result = await CreatedArtifactsNotificationHandler.ProcessCreatedArtifacts(_tenant, _message, _artifactsPublishedRepositoryMock.Object, _serviceLogRepositoryMock.Object, _wfMessagingMock.Object);

            //Assert
            Assert.IsTrue(result, "Multiple messages were sent successfully");
            _wfMessagingMock.Verify(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>()), Times.Exactly(workflowEventTriggers.Count));
        }
    }
}
