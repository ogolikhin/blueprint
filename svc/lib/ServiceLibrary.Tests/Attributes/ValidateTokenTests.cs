using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Repositories.ConfigControl;

namespace ServiceLibrary.Attributes
{
    [TestClass]
    public class ValidateTokenTests
    {
        [TestMethod]
        public async Task ValidateToken_ArgumentNullException_Unauthorize()
        {
            // Arrange
            var attribute = new ValidateToken();
            var request = new HttpRequestMessage(new HttpMethod("GET"), "http://tempuri.org/svc/filestore/files/1");
            var response = new HttpResponseMessage();

            var contextAction = ContextUtil.GetActionExecutingContext(request, response, "", typeof(ApiController));
            var serviceRepository = new Mock<ISessionRepository>();
            serviceRepository.Setup(m => m.GetAccessAsync(It.IsAny<HttpRequestMessage>()))
                .Throws<ArgumentNullException>();
            // Act
            await attribute.Validate(contextAction, serviceRepository.Object);

            //// Assert
            var responseResult = contextAction.Response;
            Assert.IsTrue(responseResult.StatusCode == HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task ValidateToken_HttpRequestException_Unauthorize()
        {
            // Arrange
            var attribute = new ValidateToken();
            var request = new HttpRequestMessage(new HttpMethod("GET"), "http://tempuri.org/svc/filestore/files/1");
            var response = new HttpResponseMessage();

            var contextAction = ContextUtil.GetActionExecutingContext(request, response, "", typeof(ApiController));
            var serviceRepository = new Mock<ISessionRepository>();
            serviceRepository.Setup(m => m.GetAccessAsync(It.IsAny<HttpRequestMessage>()))
                .Throws<HttpRequestException>();
            // Act
            await attribute.Validate(contextAction, serviceRepository.Object);

            //// Assert
            var responseResult = contextAction.Response;
            Assert.IsTrue(responseResult.StatusCode == HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task ValidateToken_Exception_InternalServerError()
        {
            // Arrange
            var attribute = new ValidateToken();
            var request = new HttpRequestMessage(new HttpMethod("GET"), "http://tempuri.org/svc/filestore/files/1");
            var response = new HttpResponseMessage();

            var contextAction = ContextUtil.GetActionExecutingContext(request, response, "", typeof(ApiController));
            var serviceRepository = new Mock<ISessionRepository>();
            serviceRepository.Setup(m => m.GetAccessAsync(It.IsAny<HttpRequestMessage>()))
                .Throws<Exception>();
            // Act
            await attribute.Validate(contextAction, serviceRepository.Object);

            //// Assert
            var responseResult = contextAction.Response;
            Assert.IsTrue(responseResult.StatusCode == HttpStatusCode.InternalServerError);
        }
    }
}
