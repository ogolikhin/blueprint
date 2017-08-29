using System.Threading.Tasks;
using ActionHandlerService.MessageHandlers.GenerateUserStories;
using ActionHandlerService.Models;
using BluePrintSys.Messaging.Models.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models.Jobs;
using ServiceLibrary.Repositories.Jobs;

namespace ActionHandlerServiceTests
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
            _tenantInformation = new TenantInformation { BlueprintConnectionString = "", TenantId = ""};
        }

        [TestMethod]
        public async Task GenerateUserStoriesActionHelper_HandleActionReturnsFalse_WhenNoMessage()
        {
            var actionHelper = new GenerateUserStoriesActionHelper(_repositoryMock.Object);
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
            var actionHelper = new GenerateUserStoriesActionHelper(_repositoryMock.Object);
            var result = await actionHelper.HandleAction(_tenantInformation, message, null);
            Assert.IsTrue(result);
        }
    }
}
