using System.Net.Http;
using ArtifactStore.Services.ArtifactListSettings;
using ArtifactStore.Services.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SearchEngineLibrary.Service;
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
        private Mock<IArtifactListSettingsService> _mockArtifactListSettingsService;
        private CollectionsController _collectionsController;
        private Session _session;
        private int UserId = 1;

        [TestInitialize]
        public void Initialize()
        {
            _session = new Session { UserId = UserId };

            _collectionsServiceMock = new Mock<Services.Collections.ICollectionsService>();
            _mockSearchEngineService = new Mock<ISearchEngineService>();
            _mockArtifactListSettingsService = new Mock<IArtifactListSettingsService>();

            _collectionsController = new CollectionsController(_collectionsServiceMock.Object, _mockArtifactListSettingsService.Object, _mockSearchEngineService.Object)
            {
                Request = new HttpRequestMessage()
            };

            _collectionsController.Request.Properties[ServiceConstants.SessionProperty] = _session;
        }
    }
}