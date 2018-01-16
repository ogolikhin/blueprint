using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SearchEngineLibrary.Service;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;

namespace ArtifactStore.Controllers
{
    [TestClass]
    public class CollectionsControllerTests
    {
        private const int UserId = 1;
        private Session _session;

        private Mock<ICollectionsRepository> _mockCollectionsRepository;
        private Mock<IServiceLogRepository> _mockServiceLogRepository;
        private Mock<IArtifactPermissionsRepository> _mockArtifactPermissionsRepository;
        private Mock<ISearchEngineService> _mockSearchEngineService;
        private CollectionsController _collectionsController;

        [TestInitialize]
        public void Initialize()
        {
            _session = new Session { UserId = UserId };

            _mockServiceLogRepository = new Mock<IServiceLogRepository>();
            _mockArtifactPermissionsRepository = new Mock<IArtifactPermissionsRepository>();
            _mockCollectionsRepository = new Mock<ICollectionsRepository>();
            _mockSearchEngineService = new Mock<ISearchEngineService>();

            _collectionsController = new CollectionsController(_mockArtifactPermissionsRepository.Object, _mockCollectionsRepository.Object, _mockServiceLogRepository.Object, _mockSearchEngineService.Object)
            {
                Request = new HttpRequestMessage()
            };

            _collectionsController.Request.Properties[ServiceConstants.SessionProperty] = _session;
        }
    }
}