using ArtifactStore.Models;
using ArtifactStore.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Attributes;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ArtifactStore.Controllers
{
    [TestClass]
    public class ArtifactVersionsControllerTest
    {
        private Mock<IArtifactPermissionsRepository> _artifactPermissionsRepositoryMock;

        private Mock<IArtifactVersionsRepository> _artifactVersionsRepositoryMock;

        private Session _session;

        private const int DEFAULT_LIMIT = 10;
        private const int DEFAULT_OFFSET = 0;
        private const int MIN_LIMIT = 1;
        private const int MAX_LIMIT = 100;

        [TestInitialize]
        public void Initialize()
        {
            // Arrange
            var userId = 1;
            _session = new Session { UserId = userId };
            
            _artifactPermissionsRepositoryMock = new Mock<IArtifactPermissionsRepository>();
            _artifactVersionsRepositoryMock = new Mock<IArtifactVersionsRepository>();
        }

        [TestMethod]
        public void GetArtifactHistoryHasSessionRequiredAttribute()
        {
            var getDiscussionMethod = typeof(ArtifactVersionsController).GetMethod("GetArtifactHistory");
            var hasSessionRequiredAttr = getDiscussionMethod.GetCustomAttributes(false).Any(a => a is SessionRequiredAttribute);
            Assert.IsTrue(hasSessionRequiredAttr);
        }

        [TestMethod]
        public async Task GetArtifactHistory_Success()
        {
            //Arrange
            const int artifactId = 1;
            const int projectId = 10;
            var itemInfo = new ItemInfo { ProjectId = projectId, ArtifactId = artifactId, ItemId = artifactId };
            var permisionDictionary = new Dictionary<int, RolePermissions>();
            var resultSet = new ArtifactHistoryResultSet { ArtifactId = artifactId };
            permisionDictionary.Add(artifactId, RolePermissions.Read);
            _artifactPermissionsRepositoryMock.Setup(m => m.GetItemInfo(artifactId, _session.UserId, true, int.MaxValue)).ReturnsAsync(itemInfo);
            _artifactPermissionsRepositoryMock.Setup(m => m.GetArtifactPermissions(It.IsAny<IEnumerable<int>>(), _session.UserId, false, int.MaxValue, true)).ReturnsAsync(permisionDictionary);
            _artifactVersionsRepositoryMock.Setup(m => m.GetArtifactVersions(artifactId, DEFAULT_LIMIT, DEFAULT_OFFSET, null, false, _session.UserId)).ReturnsAsync(resultSet);
            var controller = new ArtifactVersionsController(_artifactVersionsRepositoryMock.Object, _artifactPermissionsRepositoryMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = _session;
            //Act
            var result = await controller.GetArtifactHistory(artifactId);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(artifactId, result.ArtifactId);
        }

        [TestMethod]
        public async Task GetVersionControlArtifactInfoAsync_Success()
        {
            // Arrange
            const int userId = 1;
            var session = new Session { UserId = userId };
            const int itemId = 10;
            var vcArtifactInfo = new VersionControlArtifactInfo();
            var mockArtifactVersionsRepository = new Mock<IArtifactVersionsRepository>();
            mockArtifactVersionsRepository.Setup(r => r.GetVersionControlArtifactInfoAsync(itemId, userId)).ReturnsAsync(vcArtifactInfo);
            var artifactVersionsController = new ArtifactVersionsController(mockArtifactVersionsRepository.Object, null)
            {
                Request = new HttpRequestMessage()
            };
            artifactVersionsController.Request.Properties[ServiceConstants.SessionProperty] = session;

            // Act
            var result = await artifactVersionsController.GetVersionControlArtifactInfoAsync(itemId);

            //Assert
            Assert.AreSame(vcArtifactInfo, result);
        }
    }
}
