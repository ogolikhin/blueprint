using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Web.Http;
using Moq;
using System.Threading.Tasks;
using ServiceLibrary.Repositories;

namespace FileStore.Controllers
{
    [TestClass]
    public class StatusControllerTests
    {
        [TestMethod]
        public void GetStatus_ProperRequest_Success()
        {
            // Arrange
            var moq = new Mock<IStatusRepository>();

            moq.Setup(t => t.GetStatus()).Returns(Task.FromResult(true));

            var controller = new StatusController(moq.Object);

            controller.Request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost/files"),
                Method = HttpMethod.Get
            };

            controller.Configuration = new HttpConfiguration();
            controller.Configuration.Routes.MapHttpRoute(
                 name: "DefaultApi",
                 routeTemplate: "status");

            // Act
            var actionResult = controller.GetStatus().Result;

            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;
            var content = response.Content;

            // Assert
            Assert.IsTrue(response.IsSuccessStatusCode);
        }

        [TestMethod]
        public void GetStatus_ExceptionDuringRequest_InternalServerErrorFailure()
        {
            // Arrange
            var moq = new Mock<IStatusRepository>();

            moq.Setup(t => t.GetStatus()).Throws(new Exception());

            var controller = new StatusController(moq.Object);

            controller.Request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost/files"),
                Method = HttpMethod.Get
            };

            controller.Configuration = new HttpConfiguration();
            controller.Configuration.Routes.MapHttpRoute(
                 name: "DefaultApi",
                 routeTemplate: "status");

            // Act
            var actionResult = controller.GetStatus().Result;

            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;
            var content = response.Content;

            // Assert
            Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.InternalServerError);
        }
    }
}
