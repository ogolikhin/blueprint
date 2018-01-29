using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BlueprintSys.RC.Services.MessageHandlers.ArtifactsChanged;
using BluePrintSys.Messaging.Models.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BlueprintSys.RC.Services.Tests.MessageHandlers.ArtifactsChanged
{
    /// <summary>
    /// Tests for the ArtifactsChangedActionHelper
    /// </summary>
    [TestClass]
    public class ArtifactsChangedActionHelperTests
    {
        private Mock<IArtifactsChangedRepository> _repositoryMock;
        private ArtifactsChangedActionHelper _helper;
        private ArtifactsChangedMessage _message;
        private TenantInformation _tenantInformation;

        [TestInitialize]
        public void TestInitialize()
        {
            var artifactIds = new[]
            {
                1
            };
            _message = new ArtifactsChangedMessage(artifactIds)
            {
                ChangeType = ArtifactChangedType.Save,
                RevisionId = 12,
                UserId = 34
            };
            _repositoryMock = new Mock<IArtifactsChangedRepository>(MockBehavior.Strict);
            _helper = new ArtifactsChangedActionHelper();
            _tenantInformation = new TenantInformation();
        }

        [TestMethod]
        public async Task ArtifactsChangedActionHelper_DoesNotRepopulateSearchItems_WhenMessageHasNoArtifactIds()
        {
            //arrange
            var artifactIds = new List<int>();
            _message.ArtifactIds = artifactIds;
            //act
            var result = await _helper.HandleAction(_tenantInformation, _message, _repositoryMock.Object);
            //assert
            Assert.AreEqual(_message.ArtifactIds.Count(), artifactIds.Count);
            Assert.IsFalse(result);
            _repositoryMock.Verify(m => m.RepopulateSearchItems(It.IsAny<IEnumerable<int>>()), Times.Never);
        }

        [TestMethod]
        public async Task ArtifactsChangedActionHelper_DoesNotRepopulateSearchItems_WhenMessageHasNullArtifactIds()
        {
            //arrange
            _message.ArtifactIds = null;
            _message.ChangeType = ArtifactChangedType.Indirect;
            //act
            var result = await _helper.HandleAction(_tenantInformation, _message, _repositoryMock.Object);
            //assert
            Assert.IsNull(_message.ArtifactIds);
            Assert.IsFalse(result);
            _repositoryMock.Verify(m => m.RepopulateSearchItems(It.IsAny<IEnumerable<int>>()), Times.Never);
        }

        [TestMethod]
        public async Task ArtifactsChangedActionHelper_RepopulatesSearchItems_WhenMessageContainsSingleArtifactId()
        {
            //arrange
            var artifactIds = new List<int>
            {
                1
            };
            _message.ArtifactIds = artifactIds;
            _message.ChangeType = ArtifactChangedType.Publish;
            _repositoryMock.Setup(m => m.RepopulateSearchItems(It.IsAny<IEnumerable<int>>())).ReturnsAsync(1);
            //act
            var result = await _helper.HandleAction(_tenantInformation, _message, _repositoryMock.Object);
            //assert
            Assert.AreEqual(_message.ArtifactIds.Count(), artifactIds.Count);
            Assert.IsTrue(result);
            _repositoryMock.Verify(m => m.RepopulateSearchItems(It.IsAny<IEnumerable<int>>()), Times.Once);
        }

        [TestMethod]
        public async Task ArtifactsChangedActionHelper_RepopulatesSearchItems_WhenMessageContainsMultipleArtifactIds()
        {
            //arrange
            var artifactIds = new[]
            {
                123,
                456,
                789
            };
            _message.ArtifactIds = artifactIds;
            _message.ChangeType = ArtifactChangedType.Move;
            _repositoryMock.Setup(m => m.RepopulateSearchItems(It.IsAny<IEnumerable<int>>())).ReturnsAsync(2);
            //act
            var result = await _helper.HandleAction(_tenantInformation, _message, _repositoryMock.Object);
            //assert
            Assert.AreEqual(_message.ArtifactIds.Count(), artifactIds.Length);
            Assert.IsTrue(result);
            _repositoryMock.Verify(m => m.RepopulateSearchItems(It.IsAny<IEnumerable<int>>()), Times.Once);
        }
    }
}
