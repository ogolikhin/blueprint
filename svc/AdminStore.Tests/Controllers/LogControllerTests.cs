using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Results;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;

namespace AdminStore.Controllers
{
    [TestClass]
    public class LogControllerTests
    {
        #region Constuctor

        [TestMethod]
        public void Constructor_CreatesDefaultDependencies()
        {
            // Arrange

            // Act
            var controller = new LogController();

            // Assert
            Assert.IsInstanceOfType(controller.LogRepository, typeof(ServiceLogRepository));
        }

        #endregion

        #region PostReset

        [TestMethod]
        public async Task PostLog_Success()
        {
            // Arrange
            ClientLogModel logModel = new ClientLogModel
            {
                LogLevel = 2,
                Message = "test",
                Source = "testClass",
                StackTrace = ""
            };
            var session = new Session {UserName = "admin"};

            var logMock = new Mock<IServiceLogRepository>();

            var token = Guid.NewGuid().ToString();

            var controller = new LogController(logMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Headers.Add("Session-Token", token);
            controller.Request.Properties[ServiceConstants.SessionProperty] = session;

            logMock.Setup(
                repo => repo.LogClientMessage(logModel, It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var result = (ResponseMessageResult)await controller.Log(logModel);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.Response.StatusCode);
        }

        [TestMethod]
        public async Task PostLog_NoUserName_Success()
        {
            // Arrange
            ClientLogModel logModel = new ClientLogModel
            {
                LogLevel = 2,
                Message = "test",
                Source = "testClass",
                StackTrace = ""
            };

            var logMock = new Mock<IServiceLogRepository>();

            var token = Guid.NewGuid().ToString();

            var controller = new LogController(logMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Headers.Add("Session-Token", token);
            controller.Request.Properties[ServiceConstants.SessionProperty] = null;

            logMock.Setup(
                repo => repo.LogClientMessage(logModel, It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var result = (ResponseMessageResult)await controller.Log(logModel);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.Response.StatusCode);
        }

        [TestMethod]
        public async Task PostLog_NoToken_Success()
        {
            // Arrange
            ClientLogModel logModel = new ClientLogModel
            {
                LogLevel = 2,
                Message = "test",
                Source = "testClass",
                StackTrace = ""
            };
            var session = new Session { UserName = "admin" };

            var logMock = new Mock<IServiceLogRepository>();

            var controller = new LogController(logMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Headers.Add("Session-Token", "");
            controller.Request.Properties[ServiceConstants.SessionProperty] = session;

            logMock.Setup(
                repo => repo.LogClientMessage(logModel, It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var result = (ResponseMessageResult)await controller.Log(logModel);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.Response.StatusCode);
        }

        [TestMethod]
        public async Task PostLog_ExpiredSessionToken_Success()
        {
            // Arrange
            ClientLogModel logModel = new ClientLogModel
            {
                LogLevel = 2,
                Message = "test",
                Source = "testClass",
                StackTrace = ""
            };
            var logMock = new Mock<IServiceLogRepository>();

            var controller = new LogController(logMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            var token = Guid.NewGuid().ToString();
            controller.Request.Headers.Add("Session-Token", token);

            logMock.Setup(
                repo => repo.LogClientMessage(logModel, It.IsAny<string>(), string.Empty))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var result = (ResponseMessageResult)await controller.Log(logModel);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.Response.StatusCode);
        }
        #endregion PostReset
    }
}
