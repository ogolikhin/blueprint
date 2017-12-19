using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BlueprintSys.RC.Services.MessageHandlers.ArtifactsChanged;
using BluePrintSys.Messaging.Models.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BlueprintSys.RC.Services.Tests
{
    /// <summary>
    /// Tests for the ArtifactsChangedActionHelper
    /// </summary>
    [TestClass]
    public class ArtifactsChangedActionHelperTests
    {
        private Mock<IArtifactsChangedRepository> _repositoryMock;
        private TenantInformation _tenantInformation;

        [TestInitialize]
        public void TestInitialize()
        {
            _repositoryMock = new Mock<IArtifactsChangedRepository>(MockBehavior.Strict);
            _tenantInformation = new TenantInformation();
        }

        [TestMethod]
        public void ArtifactsChangedMessage_InstantiatesSuccessfully()
        {
            var artifactIds = new[]
            {
                1
            };
            var message = new ArtifactsChangedMessage(artifactIds);
            Assert.AreEqual(message.ArtifactIds.Count(), artifactIds.Length);
        }

        [TestMethod]
        public async Task ArtifactsChangedActionHelper_ReturnsFalse_WhenMessageHasNoArtifactIds()
        {
            var artifactIds = new int[0];
            var message = new ArtifactsChangedMessage(artifactIds);
            Assert.AreEqual(message.ArtifactIds.Count(), artifactIds.Length);
            var helper = new ArtifactsChangedActionHelper();
            var result = await helper.HandleAction(_tenantInformation, message, _repositoryMock.Object);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task ArtifactsChangedActionHelper_ReturnsTrue_WhenRepositoryIsCalled()
        {
            var artifactIds = new[]
            {
                1
            };
            var message = new ArtifactsChangedMessage(artifactIds);
            Assert.AreEqual(message.ArtifactIds.Count(), artifactIds.Length);
            var helper = new ArtifactsChangedActionHelper();
            _repositoryMock.Setup(m => m.RepopulateSearchItems(It.IsAny<IEnumerable<int>>())).ReturnsAsync(1);
            var result = await helper.HandleAction(_tenantInformation, message, _repositoryMock.Object);
            Assert.IsTrue(result);
        }
    }
}
