using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BlueprintSys.RC.Services.MessageHandlers;
using BlueprintSys.RC.Services.MessageHandlers.GenerateUserStories;
using BluePrintSys.Messaging.Models.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Jobs;
using ServiceLibrary.Repositories.Jobs;

namespace BlueprintSys.RC.Services.Tests.MessageHandlers.GenerateUserStories
{
    /// <summary>
    /// Tests for the Generate User Stories Action Helper in the Action Handler Service
    /// </summary>
    [TestClass]
    public class GenerateUserStoriesActionHelperTests
    {
        private Mock<IJobsRepository> _jobsRepositoryMock;
        private Mock<IGenerateActionsRepository> _generateActionsRepositoryMock;
        private GenerateUserStoriesActionHelper _actionHelper;
        private GenerateUserStoriesMessage _message;
        private TenantInformation _tenantInformation;

        [TestInitialize]
        public void TestInitialize()
        {
            _jobsRepositoryMock = new Mock<IJobsRepository>(MockBehavior.Strict);
            _generateActionsRepositoryMock = new Mock<IGenerateActionsRepository>(MockBehavior.Strict);
            _actionHelper = new GenerateUserStoriesActionHelper();
            _message = new GenerateUserStoriesMessage
            {
                UserId = 123,
                UserName = "admin",
                ArtifactId = 23,
                BaseHostUri = "http://localhost:9801",
                ProjectId = 123,
                ProjectName = "my project",
                RevisionId = 123
            };
            _tenantInformation = new TenantInformation
            {
                BlueprintConnectionString = "TestBlueprintConnectionString",
                TenantId = "TestTenantId"
            };
        }

        [TestMethod]
        public async Task HandleAction_NullMessage_ReturnsFalse()
        {
            var result = await _actionHelper.HandleAction(new TenantInformation(), null, null);
            Assert.IsFalse(result, "Action should have failed for null message");
            _jobsRepositoryMock.Verify(m => m.AddJobMessage(It.IsAny<JobType>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandleAction_NullTenant_ReturnsFalse()
        {
            var result = await _actionHelper.HandleAction(null, new GenerateUserStoriesMessage(), null);
            Assert.IsFalse(result, "Action should have failed for null tenant");
            _jobsRepositoryMock.Verify(m => m.AddJobMessage(It.IsAny<JobType>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandleAction_BoundardyIsReachedForProjectTenant_ReturnsFalse()
        {
            // Arrange
            _generateActionsRepositoryMock.Setup(t => t.IsProjectMaxArtifactBoundaryReached(It.IsAny<int>())).ReturnsAsync(true);

            // Act
            var result = await _actionHelper.HandleAction(_tenantInformation, _message, _generateActionsRepositoryMock.Object);

            // Assert
            Assert.IsFalse(result, "Action should have failed when boundary reached");
            _jobsRepositoryMock.Verify(t => t.AddJobMessage(It.IsAny<JobType>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task HandleAction_CannotCreateJob_ReturnsFalse()
        {
            // Arrange
            _jobsRepositoryMock.Setup(t => t.AddJobMessage(JobType.GenerateUserStories,
                false,
                It.IsAny<string>(),
                null,
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>())).ReturnsAsync((int?)null);
            _generateActionsRepositoryMock.Setup(m => m.IsProjectMaxArtifactBoundaryReached(It.IsAny<int>())).ReturnsAsync(false);
            var sqlUser = new SqlUser
            {
                UserId = 123,
                Login = "admin"
            };
            _generateActionsRepositoryMock.Setup(m => m.GetUser(It.IsAny<int>())).ReturnsAsync(sqlUser);
            _generateActionsRepositoryMock.Setup(t => t.JobsRepository).Returns(_jobsRepositoryMock.Object);

            // Act
            var result = await _actionHelper.HandleAction(_tenantInformation, _message, _generateActionsRepositoryMock.Object);

            // Assert
            Assert.IsFalse(result, "Should return false if job creation fails.");
            _jobsRepositoryMock.Verify(m => m.AddJobMessage(JobType.GenerateUserStories, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task HandleAction_WhenMessageIsValid_ReturnsTrue()
        {
            // Arrange
            _jobsRepositoryMock.Setup(t => t.AddJobMessage(JobType.GenerateUserStories,
                false,
                It.IsAny<string>(),
                null,
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>())).ReturnsAsync(1);
            _generateActionsRepositoryMock.Setup(m => m.IsProjectMaxArtifactBoundaryReached(It.IsAny<int>())).ReturnsAsync(false);
            var sqlUser = new SqlUser
            {
                UserId = 1,
                Login = "admin"
            };
            _generateActionsRepositoryMock.Setup(m => m.GetUser(It.IsAny<int>())).ReturnsAsync(sqlUser);
            _generateActionsRepositoryMock.Setup(t => t.JobsRepository).Returns(_jobsRepositoryMock.Object);

            // Act
            var result = await _actionHelper.HandleAction(_tenantInformation, _message, _generateActionsRepositoryMock.Object);

            // Assert
            Assert.IsTrue(result);
            _jobsRepositoryMock.Verify(m => m.AddJobMessage(JobType.GenerateUserStories, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
