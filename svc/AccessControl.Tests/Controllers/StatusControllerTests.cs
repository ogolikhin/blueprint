using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace AccessControl.Controllers
{
    [TestClass]
    public class StatusControllerTests
    {
        #region GetStatus

        [TestMethod]
        public async Task GetStatus_HelperReturnsGoodStatus_ReturnsOkWithCorrectContent()
        {
            // Arrange
            var statusControllerHelper = new Mock<IStatusControllerHelper>();
            statusControllerHelper.Setup(r => r.GetStatus()).ReturnsAsync(new ServiceStatus() { NoErrors = true, ServiceName = "MyServiceName" });
            var controller = CreateController(statusControllerHelper.Object);

            // Act
            OkNegotiatedContentResult<ServiceStatus> result = await controller.GetStatus("mypreauthorizedkey") as OkNegotiatedContentResult<ServiceStatus>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("MyServiceName", result.Content.ServiceName);
        }

        [TestMethod]
        public async Task GetStatus_HelperReturnsWithErrors_ReturnsInternalServerErrorWithCorrectContent()
        {
            // Arrange
            var statusControllerHelper = new Mock<IStatusControllerHelper>();
            statusControllerHelper.Setup(r => r.GetStatus()).ReturnsAsync(new ServiceStatus() { NoErrors = false, ServiceName = "MyServiceName" });
            var controller = CreateController(statusControllerHelper.Object);

            // Act
            ResponseMessageResult result = await controller.GetStatus("mypreauthorizedkey") as ResponseMessageResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.InternalServerError, result.Response.StatusCode);

            var content = await result.Response.Content.ReadAsAsync<ServiceStatus>();
            Assert.AreEqual("MyServiceName", content.ServiceName);
        }

        [TestMethod]
        public async Task GetStatus_PreAuthorizedKeysNull_ReturnsOkWithCorrectContent()
        {
            // Arrange
            var statusControllerHelper = new Mock<IStatusControllerHelper>();
            statusControllerHelper.Setup(r => r.GetStatus()).ReturnsAsync(new ServiceStatus() { NoErrors = true, ServiceName = "MyServiceName", AccessInfo = "MyAccessInfo" });
            statusControllerHelper.Setup(e => e.GetShorterStatus(It.IsAny<ServiceStatus>())).Returns(new ServiceStatus() { NoErrors = true, ServiceName = "MyServiceName", AccessInfo = null });
            var controller = CreateController(statusControllerHelper.Object);

            // Act
            OkNegotiatedContentResult<ServiceStatus> result = await controller.GetStatus() as OkNegotiatedContentResult<ServiceStatus>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("MyServiceName", result.Content.ServiceName);
            Assert.AreEqual(null, result.Content.AccessInfo);
        }


        private static StatusController CreateController(IStatusControllerHelper statusControllerHelper)
        {
            var controller = new StatusController(statusControllerHelper)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };

            return controller;
        }

        #endregion GetStatus

        #region StatusUpcheck
        [TestMethod]
        public void GetStatusUpcheck_ReturnsOk()
        {
            // Arrange
            var statusControllerHelper = new Mock<IStatusControllerHelper>();
            statusControllerHelper.Setup(r => r.GetStatus()).ReturnsAsync(new ServiceStatus() { NoErrors = true, ServiceName = "MyServiceName" });
            var controller = CreateController(statusControllerHelper.Object);

            // Act
            IHttpActionResult result = controller.GetStatusUpCheck();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }
        #endregion
    }
}
