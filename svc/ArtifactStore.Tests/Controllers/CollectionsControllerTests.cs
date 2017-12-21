using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
        private Mock<ICollectionsRepository> mockCollectionsRepository;
        private Mock<IServiceLogRepository> mockServiceLogRepository;
        private Mock<IPrivilegesRepository> _mockSqlPrivilegesRepository;
        private CollectionsController collectionsController;

        private const int userId = 1;
        private Session session;

        [TestInitialize]
        public void Initialize()
        {
            session = new Session { UserId = userId };

            mockServiceLogRepository = new Mock<IServiceLogRepository>();
            _mockSqlPrivilegesRepository = new Mock<IPrivilegesRepository>();
            mockCollectionsRepository = new Mock<ICollectionsRepository>();

            collectionsController = new CollectionsController(mockCollectionsRepository.Object, _mockSqlPrivilegesRepository.Object, mockServiceLogRepository.Object)
            {
                Request = new HttpRequestMessage()
            };

            collectionsController.Request.Properties[ServiceConstants.SessionProperty] = session;
        }
    }
}
