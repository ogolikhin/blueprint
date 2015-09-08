using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using System.Text;
using System.Linq;
using Moq;
using FileStore.Repositories;
using FileStore.Controllers;
using System.Threading.Tasks;

namespace FileStore.Tests
{
    [TestClass]
    public class FileStoreSvcTest
    {
        [TestMethod]
        public void FileStorePostTest()
        {
            // Arrange
            //var guid = new Task<Guid?>(() => { return Guid.NewGuid(); });
            //var moq = new Mock<IFilesRepository>();
            //moq.Setup(t => t.PostFile(It.IsAny<Models.File>())).Returns(guid);

            string fileName4Upload = "UploadTest.txt";
            string fileContent4Upload = "This is the content of the uploaded test file";

            MultipartFormDataContent multiPartContent = new MultipartFormDataContent("----MyGreatBoundary");
            ByteArrayContent byteArrayContent = new ByteArrayContent(Encoding.UTF8.GetBytes(fileContent4Upload));
            byteArrayContent.Headers.Add("Content-Type", "multipart/form-data");
            multiPartContent.Add(byteArrayContent, "this is the name of the content", fileName4Upload);

            var controller = new FilesController(new MockRepo());// moq.Object);

            controller.Request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost/files"),
                Content = multiPartContent
            };

            controller.Configuration = new HttpConfiguration();
            controller.Configuration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "files/{id}",
                defaults: new { id = RouteParameter.Optional });

            // Act
            // 1. Upload file
            var strGuid = (OkNegotiatedContentResult<string>)controller.PostFile().Result;
            var newFileId = new Guid(strGuid.Content);

            // 2. Download file
            var secondActionResult = controller.GetFile(newFileId.ToString().Replace("-", "")).Result;

            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = secondActionResult.ExecuteAsync(cancellationToken).Result;
            var content = response.Content;
            var fileContent4Download = content.ReadAsStringAsync().Result;
            var contentType = content.Headers.ContentType;
            var fileName4Download = content.Headers.ContentDisposition.FileName;
            var storedTime = response.Headers.GetValues("Stored-Date");

            // Assert
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsTrue(fileContent4Upload == fileContent4Download);
            Assert.IsTrue(fileName4Upload == fileName4Download);
            Assert.IsTrue(storedTime.First() == "2015-09-05T22:57:31.7824054-04:00");
        }

        [TestMethod]
        public void FileStoreGetTest()
        {
            // Arrange
            var controller = new Controllers.FilesController(new MockRepo());
            controller.Request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost/files")
            };

            controller.Configuration = new HttpConfiguration();
            controller.Configuration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "files/{id}",
                defaults: new { id = RouteParameter.Optional });

            // Act
            var actionResult = controller.GetFile("22222222222222222222222222222222").Result;

            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;
            var content = response.Content;
            var fileContent = content.ReadAsStringAsync().Result;
            var contentType = content.Headers.ContentType;
            var fileName = content.Headers.ContentDisposition.FileName;
            var storedTime = response.Headers.GetValues("Stored-Date");

            // Assert
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsTrue(fileName == "Test2.txt");
            Assert.IsTrue(fileContent == "Test2 content");
            Assert.IsTrue(storedTime.First() == "2015-09-05T22:57:31.7824054-04:00");
        }

        [TestMethod]
        public void FileStoreHeadTest()
        {
            // Arrange
            var controller = new Controllers.FilesController(new MockRepo());
            controller.Request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost/files")
            };

            controller.Configuration = new HttpConfiguration();
            controller.Configuration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "files/{id}",
                defaults: new { id = RouteParameter.Optional });

            // Act
            var actionResult = controller.HeadFile("33333333333333333333333333333333").Result;

            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;
            var content = response.Content;
            var fileContent = content.ReadAsStringAsync().Result;
            var contentType = content.Headers.ContentType;
            var fileName = content.Headers.ContentDisposition.FileName;
            var storedTime = response.Headers.GetValues("Stored-Date");

            // Assert
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsTrue(fileName == "Test3.txt");
            Assert.IsTrue(storedTime.First() == "2015-09-05T22:57:31.7824054-04:00");
        }

        [TestMethod]
        public void FileStoreDeleteTest()
        {
            // Arrange
            var controller = new FilesController(new MockRepo());
            controller.Request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost/files")
            };

            controller.Configuration = new HttpConfiguration();
            controller.Configuration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "files/{id}",
                defaults: new { id = RouteParameter.Optional });

            // Act
            var actionResult = controller.DeleteFile("33333333333333333333333333333333").Result;

            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;
            var content = response.Content;
            var id = content.ReadAsStringAsync().Result;

            // Assert
            Assert.IsTrue(response.IsSuccessStatusCode);

            // Act
            actionResult = controller.DeleteFile("33333333333333333333333333333333").Result;

            cancellationToken = new System.Threading.CancellationToken();
            response = actionResult.ExecuteAsync(cancellationToken).Result;

            // Assert
            Assert.IsTrue(response.IsSuccessStatusCode);
        }
    }
}
