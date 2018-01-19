using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ArtifactStore.Services.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Controllers
{
    [TestClass]
    public class CollectionsControllerTests
    {
        private Mock<Services.Collections.ICollectionsService> _collectionsServiceMock;
        private Mock<ISearchEngineService> _mockSearchEngineService;
        private CollectionsController _collectionsController;
        private Session _session;
        private int UserId = 1;
        private int SessionUserId = 1;
        private ISet<int> artifactIds;
        private int CollectionId;
        private AssignArtifactsResult assignArtifactsResult;

        [TestInitialize]
        public void Initialize()
        {
            _collectionsServiceMock = new Mock<ICollectionsService>();

            var session = new Session { UserId = SessionUserId };
            _controller = new CollectionsController(_collectionsServiceMock.Object)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
            _controller.Request.Properties[ServiceConstants.SessionProperty] = session;
            _controller.Request.RequestUri = new Uri("http://localhost");

            artifactIds = new HashSet<int>() { 1, 2, 3 };

            CollectionId = 1;
            assignArtifactsResult = new AssignArtifactsResult()
            {
                AddedCount = 1,
                Total = 1
            };
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task AddArtifactsToCollectionAsync_InvalidScope_ThrowsException()
        {

            _collectionsServiceMock.Setup(svc => svc.AddArtifactsToCollectionAsync(SessionUserId, CollectionId, artifactIds)).ReturnsAsync(assignArtifactsResult);

            artifactIds = null;

            await _controller.AddArtifactsToCollectionAsync(CollectionId, "add", artifactIds);
        }
    }
}