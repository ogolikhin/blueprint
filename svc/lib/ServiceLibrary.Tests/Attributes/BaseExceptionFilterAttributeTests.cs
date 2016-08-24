using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories.ConfigControl;

namespace ServiceLibrary.Attributes
{
    [TestClass]
    public class BaseExceptionFilterAttributeTests
    {
        private Mock<IServiceLogRepository> _mockServiceLogRepository ;
        private Mock<ApiController> _mockController;

        [TestInitialize]
        public void Initialize()
        {
            _mockServiceLogRepository = new Mock<IServiceLogRepository>();
            _mockController = new Mock<ApiController>();
        }

        private async Task TestOnExceptionAsync(Exception ex, HttpStatusCode expectedStatusCode)
        {
            //Arrange
            var filter = new BaseExceptionFilterAttribute();

            var request = new HttpRequestMessage(new HttpMethod("GET"), "http://someurl");
            request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            var contextAction = HttpFilterHelper.CreateActionExecutedContext(request, null);

            contextAction.ActionContext.ControllerContext.Controller = _mockController.Object;
            contextAction.Exception = ex;

            //Act
            await filter.OnExceptionAsync(contextAction, CancellationToken.None);

            //Assert
            Assert.IsTrue(contextAction.Response.StatusCode == expectedStatusCode);
        }

        [TestMethod]
        public async Task OnExceptionAsync_NotImplementedException()
        {

            await TestOnExceptionAsync(new NotImplementedException(), HttpStatusCode.NotImplemented);
        }

        [TestMethod]
        public async Task OnExceptionAsync_AuthenticationException()
        {
            await TestOnExceptionAsync(new AuthenticationException(string.Empty), HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task OnExceptionAsync_ResourceNotFoundException()
        {
            await TestOnExceptionAsync(new ResourceNotFoundException(), HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task OnExceptionAsync_BadRequestException()
        {
            await TestOnExceptionAsync(new BadRequestException(), HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task OnExceptionAsync_AuthorizationException()
        {
            await TestOnExceptionAsync(new AuthorizationException(), HttpStatusCode.Forbidden);
        }

        [TestMethod]
        public async Task OnExceptionAsync_UnknownException_LogError()
        {
            //Arrange
            var exception = new Exception();
            var logSource = "source";
            _mockController.As<ILoggable>().SetupGet(c => c.LogSource).Returns(logSource);
            _mockController.As<ILoggable>().SetupGet(c => c.Log).Returns(_mockServiceLogRepository.Object);

            //Act
            await TestOnExceptionAsync(exception, HttpStatusCode.InternalServerError);

            //Assert
            _mockServiceLogRepository.Verify(l => l.LogError(logSource, exception, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }
    }
}
