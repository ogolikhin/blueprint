//using System;
//using System.Net;
//using System.Net.Http;
//using System.Threading.Tasks;
//using System.Web.Http;
//using System.Web.Http.Results;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
//using ServiceLibrary.Repositories;
//using ServiceLibrary.Repositories.ConfigControl;

//namespace ConfigControl.Controllers
//{
//    [TestClass]
//    public class StatusControllerTests
//    {
//        #region Constructor

//        [TestMethod]
//        public void Constructor_Always_CreatesDefaultDependencies()
//        {
//            // Arrange

//            // Act
//            var controller = new StatusController();

//            // Assert
//            Assert.IsInstanceOfType(controller.StatusRepo, typeof(SqlStatusRepository));
//            Assert.IsInstanceOfType(controller.Log, typeof(ServiceLogRepository));
//        }

//        #endregion Constructor

//        #region GetStatus

//        [TestMethod]
//        public async Task GetStatus_RepositoryReturnsTrue_ReturnsOk()
//        {
//            // Arrange
//            var statusRepo = new Mock<IStatusRepository>();
//            var log = new Mock<IServiceLogRepository>();
//            statusRepo.Setup(r => r.GetStatus()).ReturnsAsync(true);
//            var controller = new StatusController(statusRepo.Object, log.Object) { Request = new HttpRequestMessage() };

//            // Act
//            IHttpActionResult result = await controller.GetStatus();

//            // Assert
//            Assert.IsInstanceOfType(result, typeof(OkResult));
//        }

//        [TestMethod]
//        public async Task GetStatus_RepositoryReturnsFalse_ReturnsServiceUnavailable()
//        {
//            // Arrange
//            var statusRepo = new Mock<IStatusRepository>();
//            var log = new Mock<IServiceLogRepository>();
//            statusRepo.Setup(r => r.GetStatus()).ReturnsAsync(false);
//            var controller = new StatusController(statusRepo.Object, log.Object) { Request = new HttpRequestMessage() };

//            // Act
//            var result = await controller.GetStatus() as StatusCodeResult;

//            // Assert
//            Assert.IsNotNull(result);
//            Assert.AreEqual(HttpStatusCode.ServiceUnavailable, result.StatusCode);
//            Assert.AreEqual(controller.Request, result.Request);
//        }

//        [TestMethod]
//        public async Task GetStatus_RepositoryThrowsException_LogsAndReturnsInternalServerError()
//        {
//            // Arrange
//            var statusRepo = new Mock<IStatusRepository>();
//            var log = new Mock<IServiceLogRepository>();
//            var exception = new Exception();
//            statusRepo.Setup(r => r.GetStatus()).Throws(exception);
//            var controller = new StatusController(statusRepo.Object, log.Object) { Request = new HttpRequestMessage() };

//            // Act
//            IHttpActionResult result = await controller.GetStatus();

//            // Assert
//            log.Verify(l => l.LogError(WebApiConfig.LogSourceStatus, exception, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));
//            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
//        }

//        #endregion GetStatus
//    }
//}
