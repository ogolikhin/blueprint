using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
        private Mock<IPrivilegesRepository> _mockSqlPrivilegesRepository;
        private CollectionsController _collectionsController;

        [TestInitialize]
        public void Initialize()
        {
            _session = new Session { UserId = UserId };

            _mockServiceLogRepository = new Mock<IServiceLogRepository>();
            _mockSqlPrivilegesRepository = new Mock<IPrivilegesRepository>();
            _mockCollectionsRepository = new Mock<ICollectionsRepository>();

            _collectionsController = new CollectionsController(
                _mockCollectionsRepository.Object,
                _mockSqlPrivilegesRepository.Object,
                _mockServiceLogRepository.Object)
            {
                Request = new HttpRequestMessage()
            };

            _collectionsController.Request.Properties[ServiceConstants.SessionProperty] = _session;
        }
    }
}