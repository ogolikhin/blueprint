using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ArtifactStore.Collections.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace ArtifactStore.Collections
{
    [TestClass]
    public class CollectionsControllerTests
    {
        private int _userId = 1;

        private Mock<ICollectionsService> _collectionsServiceMock;
        private CollectionsController _collectionsController;
        private Session _session;
        private int _sessionUserId = 1;
        private ISet<int> _artifactIds;
        private int _collectionId;
        private AddArtifactsResult _addArtifactsResult;


        [TestInitialize]
        public void Initialize()
        {
            _userId = 1;
            _session = new Session { UserId = _userId };

            _collectionsServiceMock = new Mock<ICollectionsService>();

            _collectionsController = new CollectionsController(
                _collectionsServiceMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            _collectionsController.Request.Properties[ServiceConstants.SessionProperty] = _session;

            _artifactIds = new HashSet<int>() { 1, 2, 3 };

            _collectionId = 1;
            _addArtifactsResult = new AddArtifactsResult()
            {
                AddedCount = 1,
                Total = 1
            };
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task AddArtifactsToCollectionAsync_InvalidScope_ThrowsException()
        {

            _collectionsServiceMock.Setup(svc => svc.AddArtifactsToCollectionAsync(_collectionId, _artifactIds, _sessionUserId)).ReturnsAsync(_addArtifactsResult);

            _artifactIds = null;

            await _collectionsController.AddArtifactsToCollectionAsync(_collectionId, "add", _artifactIds);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task AddArtifactsToCollectionAsync_EmptyScope_ThrowsException()
        {

            _collectionsServiceMock.Setup(svc => svc.AddArtifactsToCollectionAsync(_collectionId, _artifactIds, _sessionUserId)).ReturnsAsync(_addArtifactsResult);

            _artifactIds = new HashSet<int>();

            await _collectionsController.AddArtifactsToCollectionAsync(_collectionId, "add", _artifactIds);
        }

        [TestMethod]
        public async Task AddArtifactsToCollectionAsync_AllParametersAreValid_Success()
        {
            _collectionsServiceMock.Setup(svc => svc.AddArtifactsToCollectionAsync(_collectionId, _artifactIds, _sessionUserId)).ReturnsAsync(_addArtifactsResult);

            await _collectionsController.AddArtifactsToCollectionAsync(_collectionId, "add", _artifactIds);
        }
    }
}