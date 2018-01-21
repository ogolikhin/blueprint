using System.Net.Http;
using ArtifactStore.ArtifactList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace ArtifactStore.Collections
{
    [TestClass]
    public class CollectionsControllerTests
    {
        private int _userId = 1;

        private Mock<ICollectionsService> _collectionsServiceMock;
        private Mock<IArtifactListSettingsService> _mockArtifactListSettingsService;
        private CollectionsController _collectionsController;
        private Session _session;


        [TestInitialize]
        public void Initialize()
        {
            _userId = 1;
            _session = new Session { UserId = _userId };

            _collectionsServiceMock = new Mock<ICollectionsService>();
            _mockArtifactListSettingsService = new Mock<IArtifactListSettingsService>();

            _collectionsController = new CollectionsController(
                _collectionsServiceMock.Object,
                _mockArtifactListSettingsService.Object)
            {
                Request = new HttpRequestMessage()
            };

            _collectionsController.Request.Properties[ServiceConstants.SessionProperty] = _session;
        }
    }
}