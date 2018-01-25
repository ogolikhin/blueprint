using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BlueprintSys.RC.Services.MessageHandlers;
using BlueprintSys.RC.Services.MessageHandlers.GenerateTests;
using BluePrintSys.Messaging.Models.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Jobs;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.Jobs;

namespace BlueprintSys.RC.Services.Tests.MessageHandlers.GenerateTests
{
    /// <summary>
    /// Tests for the Generate Tests Action Helper in the Action Handler Service
    /// </summary>
    [TestClass]
    public class GenerateTestsActionHelperTests
    {
        private const int ProjectId = 1;
        private const int ArtifactId = 2;
        private const int UserId = 3;
        private const int RevisionId = 5;
        private const string UserName = "testUser";

        private Mock<IGenerateActionsRepository> _generateActionsRepositoryMock;
        private Mock<ISqlItemTypeRepository> _itemTypeRepoMock;
        private Mock<IUsersRepository> _userRepoMock;
        private Mock<IJobsRepository> _jobsRepoMock;

        [TestInitialize]
        public void TestInitialize()
        {
            _generateActionsRepositoryMock = new Mock<IGenerateActionsRepository>(MockBehavior.Strict);
            _itemTypeRepoMock = new Mock<ISqlItemTypeRepository>(MockBehavior.Strict);
            _jobsRepoMock = new Mock<IJobsRepository>(MockBehavior.Strict);
            _userRepoMock = new Mock<IUsersRepository>(MockBehavior.Strict);
            _generateActionsRepositoryMock.Setup(t => t.ItemTypeRepository).Returns(_itemTypeRepoMock.Object);
            _generateActionsRepositoryMock.Setup(t => t.JobsRepository).Returns(_jobsRepoMock.Object);
            _generateActionsRepositoryMock.Setup(t => t.UsersRepository).Returns(_userRepoMock.Object);
        }

        [TestMethod]
        public async Task HandleAction_NullMessage_ReturnsFalse()
        {
            var actionHelper = new GenerateTestsActionHelper();
            var result = await actionHelper.HandleAction(new TenantInformation(), null, null);
            Assert.IsFalse(result, "Action should have failed for null message");
            _jobsRepoMock.Verify(t => t.AddJobMessage(It.IsAny<JobType>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandleAction_NullTenant_ReturnsFalse()
        {
            var actionHelper = new GenerateTestsActionHelper();
            var result = await actionHelper.HandleAction(null, new GenerateTestsMessage(), null);
            Assert.IsFalse(result, "Action should have failed for null tenant");
            _jobsRepoMock.Verify(t => t.AddJobMessage(It.IsAny<JobType>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandleAction_ArtifactIdIsInvalid_ReturnsFalse()
        {
            //Arrange
            var actionHelper = new GenerateTestsActionHelper();

            //Act
            var result = await actionHelper.HandleAction(new TenantInformation
            {
                TenantId = Guid.NewGuid().ToString(),
                BlueprintConnectionString = "BlueprintConnectionString"
            },
            new GenerateTestsMessage
            {
                UserId = UserId,
                ProjectId = ProjectId,
                RevisionId = RevisionId,
                UserName = UserName
            },
            _generateActionsRepositoryMock.Object
            );

            //Assert
            Assert.IsFalse(result, "Action should have failed for invalid message");
            _jobsRepoMock.Verify(t => t.AddJobMessage(It.IsAny<JobType>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandleAction_RevisionIdIsInvalid_ReturnsFalse()
        {
            //Arrange
            var actionHelper = new GenerateTestsActionHelper();

            //Act
            var result = await actionHelper.HandleAction(new TenantInformation
            {
                TenantId = Guid.NewGuid().ToString(),
                BlueprintConnectionString = "BlueprintConnectionString"
            },
            new GenerateTestsMessage
            {
                ArtifactId = ArtifactId,
                ProjectId = ProjectId,
                UserId = UserId,
                UserName = UserName
            },
            _generateActionsRepositoryMock.Object
            );

            //Assert
            Assert.IsFalse(result, "Action should have failed for invalid message");
            _jobsRepoMock.Verify(t => t.AddJobMessage(It.IsAny<JobType>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandleAction_UserNameIsInvalid_ReturnsFalse()
        {
            //Arrange
            var actionHelper = new GenerateTestsActionHelper();

            //Act
            var result = await actionHelper.HandleAction(new TenantInformation
            {
                TenantId = Guid.NewGuid().ToString(),
                BlueprintConnectionString = "BlueprintConnectionString"
            },
            new GenerateTestsMessage
            {
                ArtifactId = ArtifactId,
                ProjectId = ProjectId,
                UserId = UserId,
                RevisionId = RevisionId,
            },
            _generateActionsRepositoryMock.Object
            );

            //Assert
            Assert.IsFalse(result, "Action should have failed for invalid message");
            _jobsRepoMock.Verify(t => t.AddJobMessage(It.IsAny<JobType>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandleAction_CannotCreateJob_ReturnsFalse()
        {
            //Arrange
            var actionHelper = new GenerateTestsActionHelper();
            var sqlUser = new SqlUser
            {
                UserId = UserId,
                Login = UserName
            };
            _generateActionsRepositoryMock.Setup(m => m.GetUser(It.IsAny<int>())).ReturnsAsync(sqlUser);
            _userRepoMock.Setup(t => t.GetExistingUsersByIdsAsync(It.IsAny<IEnumerable<int>>())).
                ReturnsAsync(new List<SqlUser>
                {
                    sqlUser
                });

            _jobsRepoMock.Setup(t => t.AddJobMessage(JobType.GenerateProcessTests,
                false,
                It.IsAny<string>(),
                null,
                ProjectId,
                It.IsAny<string>(),
                UserId,
                UserName,
                null)).ReturnsAsync((int?)null);

            //Act
            var result = await actionHelper.HandleAction(new TenantInformation
            {
                TenantId = Guid.NewGuid().ToString(),
                BlueprintConnectionString = "BlueprintConnectionString"
            },
            new GenerateTestsMessage
            {
                ArtifactId = ArtifactId,
                UserId = UserId,
                RevisionId = RevisionId,
                ProjectId = ProjectId,
                UserName = UserName
            },
            _generateActionsRepositoryMock.Object
            );

            //Assert
            Assert.IsFalse(result, "Action should have failed when unable to create job");
            _jobsRepoMock.Verify(t => t.AddJobMessage(JobType.GenerateProcessTests, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task HandleAction_CreatesJob_ReturnsTrue()
        {
            //Arrange
            var actionHelper = new GenerateTestsActionHelper();
            var sqlUser = new SqlUser
            {
                UserId = UserId,
                Login = UserName
            };
            _generateActionsRepositoryMock.Setup(m => m.GetUser(It.IsAny<int>())).ReturnsAsync(sqlUser);
            _userRepoMock.Setup(t => t.GetExistingUsersByIdsAsync(It.IsAny<IEnumerable<int>>())).
                ReturnsAsync(new List<SqlUser>
                {
                    sqlUser
                });

            _jobsRepoMock.Setup(t => t.AddJobMessage(JobType.GenerateProcessTests,
                false,
                It.IsAny<string>(),
                null,
                ProjectId,
                It.IsAny<string>(),
                UserId,
                UserName,
                It.IsAny<string>())).ReturnsAsync(2);

            //Act
            var result = await actionHelper.HandleAction(new TenantInformation
            {
                TenantId = Guid.NewGuid().ToString(),
                BlueprintConnectionString = "BlueprintConnectionString"
            },
            new GenerateTestsMessage
            {
                ArtifactId = ArtifactId,
                UserId = UserId,
                RevisionId = RevisionId,
                ProjectId = ProjectId,
                UserName = UserName
            },
            _generateActionsRepositoryMock.Object
            );

            //Assert
            Assert.IsTrue(result, "Action should have succeeded");
            _jobsRepoMock.Verify(t => t.AddJobMessage(JobType.GenerateProcessTests, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
