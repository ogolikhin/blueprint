using System;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using FileStore.Models;
using FileStore.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using sl = ServiceLibrary.Repositories.ConfigControl;

namespace FileStore.Controllers
{
    public partial class FilesControllerTests
    {
        [TestCategory("FileStoreTests.Get")]
        [TestMethod]
        public async Task GetFile_ImproperGuid_FormatException()
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
                    RequestUri = new Uri("http://localhost/files")
                },
                Configuration = new HttpConfiguration()
            };

            controller.Configuration.Routes.MapHttpRoute(
                 name: "DefaultApi",
                 routeTemplate: "files/{id}",
                 defaults: new { id = RouteParameter.Optional });

            // Act
            var actionResult = await controller.GetFileContent("333333333!@#@!@!@!33333333333333333333333");

            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [TestCategory("FileStoreTests.Get")]
        [TestMethod]
        public async Task GetFile_UnknownException_InternalServerErrorFailure()
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
                    RequestUri = new Uri("http://localhost/files")
                },
                Configuration = new HttpConfiguration()
            };

            controller.Configuration.Routes.MapHttpRoute(
                 name: "DefaultApi",
                 routeTemplate: "files/{id}",
                 defaults: new { id = RouteParameter.Optional });

            // Act
            var actionResult = await controller.GetFileContent("33333333333333333333333333333333");

            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.InternalServerError);
        }

        [TestCategory("FileStoreTests.Get")]
        [TestMethod]
        public async Task GetFile_NonExistentFile_NotFoundFailure()
        {
            // Arrange
            var moq = new Mock<IFilesRepository>();
            moq.Setup(t => t.GetFileHead(It.IsAny<Guid>())).ReturnsAsync(null);
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqLog = new Mock<sl.IServiceLogRepository>();

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqConfigRepo.Object, moqLog.Object)
            {
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("http://localhost/files")
                },
                Configuration = new HttpConfiguration()
            };

            controller.Configuration.Routes.MapHttpRoute(
                 name: "DefaultApi",
                 routeTemplate: "files/{id}",
                 defaults: new { id = RouteParameter.Optional });

            // Act
            var actionResult = await controller.GetFileContent("33333333333333333333333333333333");

            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.NotFound);
        }

        [TestCategory("FileStoreTests.Get")]
        [TestMethod]
        public async Task GetFile_ProperRequest_Success()
        {
            // Arrange
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqLog = new Mock<sl.IServiceLogRepository>();
            var contentString = "Test2 content";
            var fileChunk = new FileChunk()
            {
                ChunkNum = 1,
                ChunkContent = Encoding.UTF8.GetBytes(contentString),
                ChunkSize = Encoding.UTF8.GetBytes(contentString).Length
            };
            var file = new File
            {
                FileId = new Guid("22222222-2222-2222-2222-222222222222"),
                FileName = "Test2.txt",
                StoredTime = I18NHelper.DateTimeParseExactInvariant("2015-09-05T22:57:31.7824054-04:00", "o"),
                FileType = "application/octet-stream",
                FileSize = fileChunk.ChunkSize,
                ChunkCount = 1
            };

            var moqDbConnection = new Mock<DbConnection>();

            moq.Setup(t => t.CreateConnection()).Returns(moqDbConnection.Object);

            moq.Setup(t => t.GetFileHead(It.IsAny<Guid>())).ReturnsAsync(file);
            moq.Setup(t => t.GetFileInfo(It.IsAny<Guid>())).Returns(file);

            moq.Setup(t => t.ReadChunkContent(moqDbConnection.Object, file.FileId, 1)).Returns(fileChunk.ChunkContent);

            moqConfigRepo.Setup(t => t.FileChunkSize).Returns(1 * 1024 * 1024);

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqConfigRepo.Object, moqLog.Object)
            {
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("http://localhost/files")
                },
                Configuration = new HttpConfiguration()
            };

            controller.Configuration.Routes.MapHttpRoute(
                 name: "DefaultApi",
                 routeTemplate: "files/{id}",
                 defaults: new { id = RouteParameter.Optional });

            // Act
            var actionResult = await controller.GetFileContent("22222222222222222222222222222222");

            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);
            var content = response.Content;
            var fileContent = await content.ReadAsStringAsync();
            var contentType = content.Headers.ContentType;
            var fileName = content.Headers.ContentDisposition.FileName;
            var storedTime = response.Headers.GetValues("Stored-Date");

            // Assert
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsTrue(fileName == "Test2.txt");
            Assert.IsTrue(fileContent == "Test2 content");
            Assert.IsTrue(storedTime.First() == "2015-09-05T22:57:31.7824054-04:00");
        }

        [TestCategory("FileStoreTests.Get")]
        [TestMethod]
        public async Task GetFile_ProperRequestFileStream_Success()
        {
            // Arrange
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqLog = new Mock<sl.IServiceLogRepository>();
            var contentString = "Test2 content";
            var fileChunk = new FileChunk()
            {
                ChunkNum = 1,
                ChunkContent = Encoding.UTF8.GetBytes(contentString),
                ChunkSize = Encoding.UTF8.GetBytes(contentString).Length
            };
            var file = new File
            {
                FileId = new Guid("22222222-2222-2222-2222-222222222222"),
                FileName = "Test2.txt",
                StoredTime = I18NHelper.DateTimeParseExactInvariant("2015-09-05T22:57:31.7824054-04:00", "o"),
                FileType = "application/octet-stream",
                FileSize = fileChunk.ChunkSize,
                ChunkCount = 1,
                IsLegacyFile = true
            };

            var moqDbConnection = new Mock<DbConnection>();

            moqConfigRepo.Setup(t => t.FileChunkSize).Returns(1 * 1024 * 1024);
            moqConfigRepo.Setup(t => t.LegacyFileChunkSize).Returns(1);

            moq.Setup(t => t.GetFileHead(It.IsAny<Guid>())).ReturnsAsync(null);
            moq.Setup(t => t.GetFileInfo(It.IsAny<Guid>())).Returns(file);
            moq.Setup(t => t.ReadChunkContent(moqDbConnection.Object, file.FileId, 1)).Returns(fileChunk.ChunkContent);

            moqFileStreamRepo.Setup(t => t.CreateConnection()).Returns(moqDbConnection.Object);
            moqFileStreamRepo.Setup(t => t.FileExists(It.IsAny<Guid>())).Returns(true);
            moqFileStreamRepo.Setup(t => t.GetFileHead(It.IsAny<Guid>())).Returns(file);
            moqFileStreamRepo.Setup(t => t.ReadChunkContent(moqDbConnection.Object, file.FileId, 1, 0)).Returns(fileChunk.ChunkContent);

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqConfigRepo.Object, moqLog.Object)
            {
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("http://localhost/files")
                },
                Configuration = new HttpConfiguration()
            };

            controller.Configuration.Routes.MapHttpRoute(
                 name: "DefaultApi",
                 routeTemplate: "files/{id}",
                 defaults: new { id = RouteParameter.Optional });

            // Act
            var actionResult = await controller.GetFileContent("22222222222222222222222222222222");

            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);
            var content = response.Content;

            var contentType = content.Headers.ContentType;
            var fileName = content.Headers.ContentDisposition.FileName;
            var storedTime = response.Headers.GetValues("Stored-Date");

            var fileContent = await content.ReadAsStringAsync();

            // Assert
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsTrue(fileName == "Test2.txt");
            Assert.IsTrue(contentType.MediaType == "application/octet-stream");
            Assert.IsTrue(fileContent == "Test2 content", "Improper content was returned");
            Assert.IsTrue(storedTime.First() == "2015-09-05T22:57:31.7824054-04:00");
        }

        [TestCategory("FileStoreTests.Get")]
        [TestMethod]
        public async Task GetFile_NoFileRetrieved_Failure()
        {
            // Arrange
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqLog = new Mock<sl.IServiceLogRepository>();

            moq.Setup(t => t.GetFileHead(It.IsAny<Guid>())).ReturnsAsync((File)null);
            // #DEBUG

            //moqFileStreamRepo.Setup(m => m.GetFileContent(It.IsAny<Guid>())).Returns((System.IO.Stream)null);

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqConfigRepo.Object, moqLog.Object)
            {
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("http://localhost/files")
                },
                Configuration = new HttpConfiguration()
            };

            controller.Configuration.Routes.MapHttpRoute(
                 name: "DefaultApi",
                 routeTemplate: "files/{id}",
                 defaults: new { id = RouteParameter.Optional });

            // Act
            var actionResult = await controller.GetFileContent("22222222222222222222222222222222");

            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.NotFound);
        }

        [TestCategory("FileStoreTests.Get")]
        [TestMethod]
        public async Task GetFile_NoFileRetrievedEmptyName_Failure()
        {
            // Arrange
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqLog = new Mock<sl.IServiceLogRepository>();

            File file = new File();
            moq.Setup(t => t.GetFileHead(It.IsAny<Guid>())).ReturnsAsync((File)null);

            // #DEBUG
            // moqFileStreamRepo.Setup(m => m.GetFileContent(It.IsAny<Guid>())).Returns((System.IO.Stream)null);


            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqConfigRepo.Object, moqLog.Object)
            {
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("http://localhost/files")
                },
                Configuration = new HttpConfiguration()
            };

            controller.Configuration.Routes.MapHttpRoute(
                 name: "DefaultApi",
                 routeTemplate: "files/{id}",
                 defaults: new { id = RouteParameter.Optional });

            // Act
            var actionResult = controller.GetFileContent("22222222222222222222222222222222").Result;

            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.NotFound);
        }

        [TestCategory("FileStoreTests.Get")]
        [TestMethod]
        public async Task GetFile_FileAlreadyExpiredBeforeNow_NotFound()
        {
            // Arrange
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqLog = new Mock<sl.IServiceLogRepository>();
            var contentString = "Test2 content";
            var fileChunk = new FileChunk()
            {
                ChunkNum = 1,
                ChunkContent = Encoding.UTF8.GetBytes(contentString),
                ChunkSize = Encoding.UTF8.GetBytes(contentString).Length
            };
            var file = new File
            {
                FileId = new Guid("22222222-2222-2222-2222-222222222222"),
                FileName = "Test2.txt",
                StoredTime = I18NHelper.DateTimeParseExactInvariant("2015-09-05T22:57:31.7824054-04:00", "o"),
                FileType = "application/octet-stream",
                FileSize = fileChunk.ChunkSize,
                ChunkCount = 1,
                ExpiredTime = DateTime.UtcNow
            };

            var moqDbConnection = new Mock<DbConnection>();

            moq.Setup(t => t.CreateConnection()).Returns(moqDbConnection.Object);

            moq.Setup(t => t.GetFileHead(It.IsAny<Guid>())).ReturnsAsync(file);
            moq.Setup(t => t.GetFileInfo(It.IsAny<Guid>())).Returns(file);

            moq.Setup(t => t.ReadChunkContent(moqDbConnection.Object, file.FileId, 1)).Returns(fileChunk.ChunkContent);

            moqConfigRepo.Setup(t => t.FileChunkSize).Returns(1 * 1024 * 1024);

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqConfigRepo.Object, moqLog.Object)
            {
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("http://localhost/files")
                },
                Configuration = new HttpConfiguration()
            };

            controller.Configuration.Routes.MapHttpRoute(
                 name: "DefaultApi",
                 routeTemplate: "files/{id}",
                 defaults: new { id = RouteParameter.Optional });

            // Act
            var actionResult = await controller.GetFileContent("22222222222222222222222222222222");

            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.NotFound, "Expired file should return status code as NotFound");
        }
    }
}
