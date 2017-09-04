using System.Collections.Generic;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.MessageHandlers.GenerateUserStories;
using BlueprintSys.RC.Services.Models;
using BlueprintSys.RC.Services.Repositories;
using BluePrintSys.Messaging.Models.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Jobs;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.Jobs;

namespace BlueprintSys.RC.Services.Tests
{
    /// <summary>
    /// Tests for the Generate User Stories Action Helper in the Action Handler Service
    /// </summary>
    [TestClass]
    public class GenerateUserStoriesActionHelperTests
    {
        private Mock<IJobsRepository> _repositoryMock;
        private TenantInformation _tenantInformation;

        [TestInitialize]
        public void TestInitialize()
        {
            _repositoryMock = new Mock<IJobsRepository>();

            int jobId = 10;
            _repositoryMock.Setup(m => m.AddJobMessage(It.IsAny<JobType>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(jobId);
            _tenantInformation = new TenantInformation { BlueprintConnectionString = "", TenantId = "" };
        }

        [TestMethod]
        public async Task GenerateUserStoriesActionHelper_HandleActionReturnsFalse_WhenNoMessage()
        {
            var actionHelper = new GenerateUserStoriesActionHelper();
            var result = await actionHelper.HandleAction(null, null, null);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task GenerateUserStoriesActionHelper_HandleActionReturnsTrue_WhenMessageIsValid()
        {
            var message = new GenerateUserStoriesMessage
            {
                UserId = 1,
                UserName = "admin",
                ArtifactId = 23,
                BaseHostUri = "http://localhost:9801",
                ProjectId = 1,
                ProjectName = "test",
                RevisionId = 1
            };
            var actionHelper = new GenerateUserStoriesActionHelper();
            var actionHandlerServiceRepositoryMock = new Mock<IGenerateUserStoriesRepository>();
            var jobServicesMock = new Mock<IJobsRepository>();
            jobServicesMock.Setup(t => t.AddJobMessage(JobType.GenerateUserStories,
                false,
                It.IsAny<string>(),
                null,
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>()
                )).ReturnsAsync(1);
            var userRepoMock = new Mock<IUsersRepository>();
            actionHandlerServiceRepositoryMock.Setup(t => t.UsersRepository).Returns(userRepoMock.Object);
            userRepoMock.Setup(t => t.GetExistingUsersByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new SqlUser[] { new SqlUser()
                {
                    UserId = 1,
                    Login = "admin"
                }});
            actionHandlerServiceRepositoryMock.Setup(t => t.JobsRepository).Returns(jobServicesMock.Object);

            //Act
            var result = await actionHelper.HandleAction(_tenantInformation, message, actionHandlerServiceRepositoryMock.Object);

            //Assert
            Assert.IsTrue(result);
        }
    }
}
