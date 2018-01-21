using System.Net.Http;
using ArtifactStore.Services.ArtifactListSettings;
using ArtifactStore.Services.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace ArtifactStore.Controllers
{
    [TestClass]
    public class CollectionsControllerTests
    {
        private Mock<ICollectionsService> _collectionsServiceMock;
        private Mock<IArtifactListSettingsService> _mockArtifactListSettingsService;
        private CollectionsController _collectionsController;
        private Session _session;
        private int UserId = 1;

        [TestInitialize]
        public void Initialize()
        {
            _session = new Session { UserId = UserId };

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