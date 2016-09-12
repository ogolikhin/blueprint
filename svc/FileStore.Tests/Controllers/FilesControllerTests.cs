using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using FileStore.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using File = FileStore.Models.File;
using sl = ServiceLibrary.Repositories.ConfigControl;

namespace FileStore.Controllers
{
    [TestClass]
    public partial class FilesControllerTests
    {
        private const int DefaultChunkSize = 1048576; // 1mb chunk size

        [TestCategory("FileStoreTests.Head")]
        [TestMethod]
        public async Task HeadFile_GetHeadForExistentFile_Success()
        {
            // Arrange
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqLog = new Mock<sl.IServiceLogRepository>();

            var file = new File
            {
                FileId = new Guid("33333333-3333-3333-3333-333333333333"),
                FileName = "Test3.txt",
                StoredTime = I18NHelper.DateTimeParseExactInvariant("2015-09-05T22:57:31.7824054-04:00", "o"),
                FileType = "text/html",
                ChunkCount = 1
            };

            moq.Setup(t => t.GetFileHead(It.IsAny<Guid>())).ReturnsAsync(file);

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqConfigRepo.Object, moqLog.Object)
            {
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("http://localhost/files"),
                    Method = HttpMethod.Head
                },
                Configuration = new HttpConfiguration()
            };

            controller.Configuration.Routes.MapHttpRoute(
                 name: "DefaultApi",
                 routeTemplate: "files/{id}",
                 defaults: new { id = RouteParameter.Optional });

            // Act
            var actionResult = await controller.GetFileHead("33333333333333333333333333333333");

            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);
            var content = response.Content;
            var contentType = content.Headers.ContentType;
            var fileName = content.Headers.ContentDisposition.FileName;
            var storedTime = response.Headers.GetValues("Stored-Date");

            // Assert
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsTrue(fileName == "Test3.txt");
            Assert.IsTrue(contentType.MediaType == "text/html", string.Format("Returned content type {0} does not match expected {1}", contentType, "text/html"));
            Assert.IsTrue(storedTime.First() == "2015-09-05T22:57:31.7824054-04:00");
        }

        [TestCategory("FileStoreTests.Head")]
        [TestMethod]
        public async Task HeadFile_GetHeadForExistentFileStreamFile_Success()
        {
            // Arrange
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqLog = new Mock<sl.IServiceLogRepository>();

            var file = new File
            {
                FileId = new Guid("33333333-3333-3333-3333-333333333333"),
                FileName = "Test3.txt",
                StoredTime = I18NHelper.DateTimeParseExactInvariant("2015-09-05T22:57:31.7824054-04:00", "o"),
                FileType = "text/html",
                ChunkCount = 1,
                IsLegacyFile = true
            };

            moq.Setup(t => t.GetFileHead(It.IsAny<Guid>())).ReturnsAsync(null);

            moqFileStreamRepo.Setup(t => t.FileExists(file.FileId)).Returns(true);
            moqFileStreamRepo.Setup(t => t.GetFileHead(file.FileId)).Returns(file);

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqConfigRepo.Object, moqLog.Object)
            {
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("http://localhost/files"),
                    Method = HttpMethod.Head
                },
                Configuration = new HttpConfiguration()
            };

            controller.Configuration.Routes.MapHttpRoute(
                 name: "DefaultApi",
                 routeTemplate: "files/{id}",
                 defaults: new { id = RouteParameter.Optional });

            // Act
            var actionResult = await controller.GetFileHead("33333333333333333333333333333333");

            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);
            var content = response.Content;
            var contentType = content.Headers.ContentType;
            var fileName = content.Headers.ContentDisposition.FileName;
            var storedTime = response.Headers.GetValues("Stored-Date");

            // Assert
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsTrue(fileName == "Test3.txt");
            Assert.IsTrue(contentType.MediaType == "application/octet-stream", string.Format("Returned content type {0} does not match expected {1}", contentType, "text/html"));
            Assert.IsTrue(storedTime.First() == "2015-09-05T22:57:31.7824054-04:00");
        }

        [TestCategory("FileStoreTests.Head")]
        [TestMethod]
        public async Task HeadFile_GetHeadForNonExistentFile_Failure()
        {
            // Arrange
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqLog = new Mock<sl.IServiceLogRepository>();

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqConfigRepo.Object, moqLog.Object)
            {
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("http://localhost/files"),
                    Method = HttpMethod.Head
                },
                Configuration = new HttpConfiguration()
            };

            controller.Configuration.Routes.MapHttpRoute(
                 name: "DefaultApi",
                 routeTemplate: "files/{id}",
                 defaults: new { id = RouteParameter.Optional });

            // Act
            var actionResult = await controller.GetFileHead("33333333333333333333333333333333");

            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.NotFound);
        }

        [TestCategory("FileStoreTests.Head")]
        [TestMethod]
        public async Task HeadFile_ImproperGuid_FormatException()
        {
            // Arrange
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqLog = new Mock<sl.IServiceLogRepository>();

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqConfigRepo.Object, moqLog.Object)
            {
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("http://localhost/files"),
                    Method = HttpMethod.Head
                },
                Configuration = new HttpConfiguration()
            };

            controller.Configuration.Routes.MapHttpRoute(
                 name: "DefaultApi",
                 routeTemplate: "files/{id}",
                 defaults: new { id = RouteParameter.Optional });

            // Act
            var actionResult = await controller.GetFileHead("333333333!@#@!@!@!33333333333333333333333");

            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [TestCategory("FileStoreTests.Head")]
        [TestMethod]
        public async Task HeadFile_UnknownException_InternalServerErrorFailure()
        {
            // Arrange
            var moq = new Mock<IFilesRepository>();
            moq.Setup(t => t.GetFileHead(It.IsAny<Guid>())).Throws(new Exception());
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqLog = new Mock<sl.IServiceLogRepository>();

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqConfigRepo.Object, moqLog.Object)
            {
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("http://localhost/files"),
                    Method = HttpMethod.Head
                },
                Configuration = new HttpConfiguration()
            };

            controller.Configuration.Routes.MapHttpRoute(
                 name: "DefaultApi",
                 routeTemplate: "files/{id}",
                 defaults: new { id = RouteParameter.Optional });

            // Act
            var actionResult = await controller.GetFileHead("33333333333333333333333333333333");

            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.InternalServerError);
        }
    }
}
