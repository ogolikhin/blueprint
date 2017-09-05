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
        private Mock<IArtifactsPublishedRepository> _artifactsPublishedRepositoryMock;
        private Mock<IWorkflowRepository> _workflowRepoMock;
        private Mock<IUsersRepository> _userRepoMock;
        private Mock<IServiceLogRepository> _serviceLogRepositoryMock;
        private Mock<IWorkflowMessagingProcessor> _wfMessagingMock;

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
            _artifactsPublishedRepositoryMock = new Mock<IArtifactsPublishedRepository>(MockBehavior.Loose);
            _serviceLogRepositoryMock = new Mock<IServiceLogRepository>(MockBehavior.Loose);
            _workflowRepoMock = new Mock<IWorkflowRepository>(MockBehavior.Loose);
            _userRepoMock = new Mock<IUsersRepository>(MockBehavior.Loose);
            _wfMessagingMock = new Mock<IWorkflowMessagingProcessor>(MockBehavior.Loose);

            _artifactsPublishedRepositoryMock.Setup(t => t.WorkflowRepository).Returns(_workflowRepoMock.Object);
            _artifactsPublishedRepositoryMock.Setup(t => t.UsersRepository).Returns(_userRepoMock.Object);
        }

        [TestMethod]
        public async Task ProcessCreatedArtifacts_MessageIsNull_ReturnsTrue()
        {
            
            //Act
            var result = await CreatedArtifactsNotificationHandler.ProcessCreatedArtifacts(_tenant,
                null,
                _artifactsPublishedRepositoryMock.Object,
                _serviceLogRepositoryMock.Object,
                _wfMessagingMock.Object);

            //Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ProcessCreatedArtifacts_NoCreatedArtifacts_ReturnsTrue()
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
            Assert.IsTrue(result);
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
        public async Task ProcessCreatedArtifacts_NoEventTriggersForArtifact_ReturnsTrue()
        {
            //Arrange
            int projectId = 1;
            int artifactId = 2;
            string artifactName = "New Artifact";
            ItemTypePredefined predefined = ItemTypePredefined.Process;
            int itemTypeId = 3;

            var message = new ArtifactsPublishedMessage
            {
                RevisionId = _revisionId,
                UserId = _userId,
                UserName = "admin",
                Artifacts = new List<PublishedArtifactInformation>
                {
                    new PublishedArtifactInformation
                    {
                        Id = artifactId,
                        Name = artifactName,
                        ModifiedProperties = new List<PublishedPropertyInformation>(),
                        ProjectId = projectId,
                        IsFirstTimePublished = true,
                        Url = "localhost:id",
                        BaseUrl = "localhost",
                        Predefined = (int)predefined
                    }
                }
            };
            _workflowRepoMock.Setup(t => t.GetWorkflowMessageArtifactInfoAsync(_userId,
                It.IsAny<IEnumerable<int>>(),
                _revisionId,
                It.IsAny<IDbTransaction>())).ReturnsAsync(new List<WorkflowMessageArtifactInfo>
                {
                    new WorkflowMessageArtifactInfo
                    {
                        Id = artifactId,
                        ProjectName = "ProjectName",
                        ProjectId = projectId,
                        Name = artifactName,
                        PredefinedType = predefined,
                        ItemTypeId = itemTypeId
                    }
                });
            _workflowRepoMock.Setup(t => t.GetWorkflowEventTriggersForNewArtifactEvent(_userId,
                It.IsAny<IEnumerable<int>>(),
                _revisionId)).ReturnsAsync(new WorkflowTriggersContainer());

            //Act
           var result =  await CreatedArtifactsNotificationHandler.ProcessCreatedArtifacts(_tenant,
                message,
                _artifactsPublishedRepositoryMock.Object,
                _serviceLogRepositoryMock.Object,
                _wfMessagingMock.Object);

            //Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ProcessCreatedArtifacts_EventTriggersExistForArtifact_ReturnsTrue()
        {
            //Arrange
            int projectId = 1;
            int artifactId = 2;
            string artifactName = "New Artifact";
            ItemTypePredefined predefined = ItemTypePredefined.Process;
            int itemTypeId = 3;

            var message = new ArtifactsPublishedMessage
            {
                RevisionId = _revisionId,
                UserId = _userId,
                UserName = "admin",
                Artifacts = new List<PublishedArtifactInformation>
                {
                    new PublishedArtifactInformation
                    {
                        Id = artifactId,
                        Name = artifactName,
                        ModifiedProperties = new List<PublishedPropertyInformation>(),
                        ProjectId = projectId,
                        IsFirstTimePublished = true,
                        Url = "localhost:id",
                        BaseUrl = "localhost",
                        Predefined = (int)predefined
                    }
                }
            };
            _workflowRepoMock.Setup(t => t.GetWorkflowMessageArtifactInfoAsync(_userId,
                It.IsAny<IEnumerable<int>>(),
                _revisionId,
                It.IsAny<IDbTransaction>())).ReturnsAsync(new List<WorkflowMessageArtifactInfo>
                {
                    new WorkflowMessageArtifactInfo
                    {
                        Id = artifactId,
                        ProjectName = "ProjectName",
                        ProjectId = projectId,
                        Name = artifactName,
                        PredefinedType = predefined,
                        ItemTypeId = itemTypeId
                    }
                });
            var workflowTriggersContainer = new WorkflowTriggersContainer();
            workflowTriggersContainer.AsynchronousTriggers.Add(new WorkflowEventTrigger()
            {
                Name = "Trigger One",
                Condition = new WorkflowEventCondition(),
                Action = new GenerateChildrenAction
                {
                    ArtifactTypeId = itemTypeId,
                    ChildCount = 3
                } 
            });
            _workflowRepoMock.Setup(t => t.GetWorkflowEventTriggersForNewArtifactEvent(_userId,
                It.IsAny<IEnumerable<int>>(),
                _revisionId)).ReturnsAsync(workflowTriggersContainer);

            //Act
            var result = await CreatedArtifactsNotificationHandler.ProcessCreatedArtifacts(_tenant,
                 message,
                 _artifactsPublishedRepositoryMock.Object,
                 _serviceLogRepositoryMock.Object,
                 _wfMessagingMock.Object);

            //Assert
            Assert.IsTrue(result, "Messages were note sent successfully");
        }
    }
}
