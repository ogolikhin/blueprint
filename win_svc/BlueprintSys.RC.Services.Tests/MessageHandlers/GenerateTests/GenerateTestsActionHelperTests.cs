using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.MessageHandlers.GenerateTests;
using BlueprintSys.RC.Services.Models;
using BlueprintSys.RC.Services.Repositories;
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

        private Mock<IGenerateActionsRepository> _actionHandMock;
        private Mock<ISqlItemTypeRepository> _itemTypeRepoMock;
        private Mock<IUsersRepository> _userRepoMock;
        private Mock<IJobsRepository> _jobsRepoMock;

        [TestInitialize]
        public void TestInitialize()
        {
            _actionHandMock = new Mock<IGenerateActionsRepository>(MockBehavior.Strict);
            _itemTypeRepoMock = new Mock<ISqlItemTypeRepository>(MockBehavior.Strict);
            _jobsRepoMock = new Mock<IJobsRepository>(MockBehavior.Strict);
            _userRepoMock = new Mock<IUsersRepository>(MockBehavior.Strict);
            _actionHandMock.Setup(t => t.ItemTypeRepository).Returns(_itemTypeRepoMock.Object);
            _actionHandMock.Setup(t => t.JobsRepository).Returns(_jobsRepoMock.Object);
            _actionHandMock.Setup(t => t.UsersRepository).Returns(_userRepoMock.Object);
        }

        [TestMethod]
        public async Task GenerateTestsActionHelper_NullMessage_HandleActionReturnsFalse()
        {
            var actionHelper = new GenerateTestsActionHelper();
            var result = await actionHelper.HandleAction(new TenantInformation(), null, null);
            Assert.IsFalse(result, "Action should have failed for null message");
        }

        [TestMethod]
        public async Task GenerateTestsActionHelper_NullTenant_HandleActionReturnsFalse()
        {
            var actionHelper = new GenerateTestsActionHelper();
            var result = await actionHelper.HandleAction(null, new GenerateTestsMessage(), null);
            Assert.IsFalse(result, "Action should have failed for null tenant");
        }

        [TestMethod]
        public async Task GenerateTestsActionHelper_ArtifactIdIsInvalid_HandleActionReturnsFalse()
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
            _actionHandMock.Object
            );

            //Assert
            Assert.IsFalse(result, "Action should have failed for null message");
        }

        [TestMethod]
        public async Task GenerateTestsActionHelper_RevisionIdIsInvalid_HandleActionReturnsFalse()
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
            _actionHandMock.Object
            );

            //Assert
            Assert.IsFalse(result, "Action should have failed for null message");
        }

        [TestMethod]
        public async Task GenerateTestsActionHelper_UserNameIsInvalid_HandleActionReturnsFalse()
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
            _actionHandMock.Object
            );

            //Assert
            Assert.IsFalse(result, "Action should have failed for null message");
        }

        [TestMethod]
        public async Task GenerateTestsActionHelper_CannotCreateJob_HandleActionReturnsFalse()
        {
            //Arrange
            var actionHelper = new GenerateTestsActionHelper();

            _userRepoMock.Setup(t => t.GetExistingUsersByIdsAsync(It.IsAny<IEnumerable<int>>())).
                ReturnsAsync(new List<SqlUser>
                {
                    new SqlUser
                    {
                        UserId = UserId,
                        Login = UserName
                    }
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
            _actionHandMock.Object
            );

            //Assert
            Assert.IsFalse(result, "Action should have failed for null message");
        }

        [TestMethod]
        public async Task GenerateTestsActionHelper_CreatesJob_HandleActionReturnsTrue()
        {
            //Arrange
            var actionHelper = new GenerateTestsActionHelper();

            _userRepoMock.Setup(t => t.GetExistingUsersByIdsAsync(It.IsAny<IEnumerable<int>>())).
                ReturnsAsync(new List<SqlUser>
                {
                    new SqlUser
                    {
                        UserId = UserId,
                        Login = UserName
                    }
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
            _actionHandMock.Object
            );

            //Assert
            Assert.IsTrue(result, "Action should have succeeded");
        }
    }
}
