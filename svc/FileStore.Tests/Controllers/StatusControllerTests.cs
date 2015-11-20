using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Repositories;

namespace FileStore.Controllers
{
    [TestClass]
    public class StatusControllerTests
    {
        #region Constructor

        [TestMethod]
        public void Constructor_CreatesDefaultDependencies()
        {
            // Arrange

            // Act
            var controller = new StatusController();

            // Assert
            Assert.IsInstanceOfType(controller.StatusRepo, typeof(SqlStatusRepository));
        }

        #endregion Constructor

        #region GetStatus

        [TestMethod]
        public async Task GetStatus_RepositoryReturnsTrue_ReturnsOk()
        {
            // Arrange
            var statusRepo = new Mock<IStatusRepository>();
            statusRepo.Setup(r => r.GetStatus()).Returns(Task.FromResult(true)).Verifiable();
            var controller = new StatusController(statusRepo.Object) { Request = new HttpRequestMessage() };

            // Act
            IHttpActionResult result = await controller.GetStatus();

            // Assert
            statusRepo.Verify();
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public async Task GetStatus_RepositoryReturnsFalse_ReturnsServiceUnavailable()
        {
            // Arrange
            var statusRepo = new Mock<IStatusRepository>();
            statusRepo.Setup(r => r.GetStatus()).Returns(Task.FromResult(false)).Verifiable();
            var controller = new StatusController(statusRepo.Object) { Request = new HttpRequestMessage() };

            // Act
            var result = await controller.GetStatus() as StatusCodeResult;

            // Assert
            statusRepo.Verify();
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.ServiceUnavailable, result.StatusCode);
            Assert.AreEqual(controller.Request, result.Request);
        }

        [TestMethod]
        public async Task GetStatus_RepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var statusRepo = new Mock<IStatusRepository>();
            statusRepo.Setup(r => r.GetStatus()).Throws(new Exception()).Verifiable();
            var controller = new StatusController(statusRepo.Object) { Request = new HttpRequestMessage() };

            // Act
            IHttpActionResult result = await controller.GetStatus();

            // Assert
            statusRepo.Verify();
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }

        #endregion GetStatus
    }
}
