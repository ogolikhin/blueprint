using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Web.Http.Results;

namespace AdminStore.Controllers
{
    [TestClass]
    public class LogControllerTests
    {
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
            var session = new Session { UserName = "admin" };

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

            var logMock = new Mock<IServiceLogRepository>();

            var controller = new LogController(logMock.Object)
            {
                Request = new HttpRequestMessage()
            };

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

        [TestMethod]
        public void LogController_ExtractSessionId_SessionTokenToShort_SessionIdReturnsWholeToken()
        {
            var token = "token";
            var headers = new HttpRequestMessage().Headers;
            headers.Add(ServiceConstants.BlueprintSessionTokenKey, token);

            var sessionId = LogController.ExtractSessionId(headers);

            Assert.AreEqual(token, sessionId);
        }

        [TestMethod]
        public void LogController_ExtractSessionId_SessionTokenMoreThen8Chars_SessionIdReturnsFirst8Chars()
        {
            var token = "1E6207CE-6AB4-438F-8F87-9C76BEB95FB8";
            var headers = new HttpRequestMessage().Headers;
            headers.Add(ServiceConstants.BlueprintSessionTokenKey, token);

            var sessionId = LogController.ExtractSessionId(headers);

            Assert.AreEqual("1E6207CE", sessionId);
        }

        [TestMethod]
        public void LogController_ExtractSessionId_SessionTokenHeaderNotDefined_SessionIdReturnsEmptyString()
        {
            var headers = new HttpRequestMessage().Headers;

            var sessionId = LogController.ExtractSessionId(headers);

            Assert.AreEqual(string.Empty, sessionId);
        }

        [TestMethod]
        public void LogController_ExtractSessionId_EmptySessionToken_SessionIdReturnsEmptyString()
        {
            var token = "";
            var headers = new HttpRequestMessage().Headers;
            headers.Add(ServiceConstants.BlueprintSessionTokenKey, token);

            var sessionId = LogController.ExtractSessionId(headers);

            Assert.AreEqual(string.Empty, sessionId);
        }

        [TestMethod]
        public async Task PostLog_LogEntryNotProvided_BadRequest()
        {
            // Arrange
            var httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            var controller = new LogController(null)
            {
                Request = httpRequestMessage,
            };

            // Act
            var result = await controller.Log(null);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, (await result.ExecuteAsync(CancellationToken.None)).StatusCode);
        }
        #endregion PostReset
    }
}
