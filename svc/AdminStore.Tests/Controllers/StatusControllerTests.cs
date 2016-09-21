using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Repositories.ConfigControl;
using ServiceLibrary.Helpers;

namespace AdminStore.Controllers
{
    [TestClass]
    public class StatusControllerTests
    {
        #region Constructor

        [TestMethod]
        public void Constructor_Always_CreatesDefaultDependencies()
        {
            // Arrange

            // Act
            var controller = new StatusController();

            // Assert
            Assert.IsInstanceOfType(controller._statusControllerHelper, typeof(StatusControllerHelper));
        }

        #endregion Constructor

        #region GetStatus


        [TestMethod]
        public async Task GetStatus_PreAuthorizedKeysDoNotMatch_ReturnsUnauthorized()
        {
            // Arrange
            var statusControllerHelper = new Mock<IStatusControllerHelper>();
            statusControllerHelper.Setup(r => r.GetStatus()).ReturnsAsync(new ServiceStatus() { NoErrors = true });
            var controller = CreateController(statusControllerHelper.Object, "mypreauthorizedkey");

            // Act
            ResponseMessageResult result = await controller.GetStatus("NOTmypreauthorizedkey") as ResponseMessageResult;

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.Response.StatusCode);
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

        [TestMethod]
        public async Task GetStatus_HelperReturnsGoodStatus_ReturnsOkWithCorrectContent()
        {
            // Arrange
            var statusControllerHelper = new Mock<IStatusControllerHelper>();
            statusControllerHelper.Setup(r => r.GetStatus()).ReturnsAsync(new ServiceStatus() { NoErrors = true, ServiceName = "MyServiceName" });
            var controller = CreateController(statusControllerHelper.Object, "mypreauthorizedkey");

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
            statusControllerHelper.Setup(r => r.GetStatus()).ReturnsAsync(new ServiceStatus() { NoErrors = false, ServiceName="MyServiceName" });
            var controller = CreateController(statusControllerHelper.Object, "mypreauthorizedkey");

            // Act
            ResponseMessageResult result = await controller.GetStatus("mypreauthorizedkey") as ResponseMessageResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.InternalServerError, result.Response.StatusCode);

            var content = await result.Response.Content.ReadAsAsync<ServiceStatus>();
            Assert.AreEqual("MyServiceName", content.ServiceName);
        }



        private static StatusController CreateController(IStatusControllerHelper statusControllerHelper, string preAuthorizedKey = null)
        {
            var controller = new StatusController(statusControllerHelper, preAuthorizedKey)
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
            var controller = CreateController(statusControllerHelper.Object, "mypreauthorizedkey");

            // Act
            IHttpActionResult result = controller.GetStatusUpCheck();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }
        #endregion
    }
}
