using FileStore.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using FsFile = FileStore.Models.File;
using sl = ServiceLibrary.Repositories.ConfigControl;

namespace FileStore.Controllers
{
    public partial class FilesControllerTests
    {
        [TestCategory("FileStoreTests.Post")]
        [TestMethod]
        public async Task PostFile_MultipartSingleFile_Success()
        {
            //Arrange
            var guid = Guid.NewGuid();
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqLog = new Mock<sl.IServiceLogRepository>();

            moq.Setup(t => t.PostFileHead(It.IsAny<FsFile>())).ReturnsAsync(guid);

            string fileName4Upload = "\"UploadTest.txt\"";
            string fileContent4Upload = "This is the content of the uploaded test file";

            MultipartFormDataContent multiPartContent = new MultipartFormDataContent("----MyGreatBoundary");
            ByteArrayContent byteArrayContent = new ByteArrayContent(Encoding.UTF8.GetBytes(fileContent4Upload));
            byteArrayContent.Headers.Add("Content-Type", "multipart/form-data");
            multiPartContent.Add(byteArrayContent, "this is the name of the content", fileName4Upload);

            moqConfigRepo.Setup(t => t.FileChunkSize).Returns(DefaultChunkSize);

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqConfigRepo.Object, moqLog.Object)
            {
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("http://localhost/files"),
                    Content = multiPartContent
                },
                Configuration = new HttpConfiguration()
            };


            var context = await SetupMultipartPost(multiPartContent);

            controller.Configuration.Routes.MapHttpRoute(
                 name: "DefaultApi",
                 routeTemplate: "files/{id}",
                 defaults: new { id = RouteParameter.Optional });

            // Act
            // 1. Upload file
            var response = await controller.PostFileHttpContext(context.Object, null);

            // Assert
            Assert.IsTrue(response.FileId.HasValue);
        }

        [TestCategory("FileStoreTests.Post")]
        [TestMethod]
        public async Task PostFile_MultipartMultipleFiles_BadRequestFailure()
        {
            //Arrange
            var guid = Guid.NewGuid();
            var moq = new Mock<IFilesRepository>();
            moq.Setup(t => t.PostFileHead(It.IsAny<FsFile>())).ReturnsAsync(guid);
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqLog = new Mock<sl.IServiceLogRepository>();

            string fileName4Upload = "\"UploadTest.txt\"";
            string fileContent4Upload = "This is the content of the uploaded test file";

            var multiPartContent1 = new MultipartFormDataContent("----MyGreatBoundary");
            ByteArrayContent byteArrayContent1 = new ByteArrayContent(Encoding.UTF8.GetBytes(fileContent4Upload));
            ByteArrayContent byteArrayContent2 = new ByteArrayContent(Encoding.UTF8.GetBytes(fileContent4Upload));
            byteArrayContent1.Headers.Add("Content-Type", "multipart/form-data");
            multiPartContent1.Add(byteArrayContent1, "this is the name of the content", fileName4Upload);
            multiPartContent1.Add(byteArrayContent2, "this is the name of the content", fileName4Upload);

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqConfigRepo.Object, moqLog.Object)
            {
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("http://localhost/files"),
                    Content = multiPartContent1
                },
                Configuration = new HttpConfiguration()
            };

            var context = await SetupMultipartPost(multiPartContent1);


            controller.Configuration.Routes.MapHttpRoute(
                 name: "DefaultApi",
                 routeTemplate: "files/{id}",
                 defaults: new { id = RouteParameter.Optional });

            // Act
            // 1. Upload file
            var response = await controller.PostFileHttpContext(context.Object, null);

            //Assert
            Assert.IsTrue(response.Status == HttpStatusCode.BadRequest);
        }

        [TestCategory("FileStoreTests.Post")]
        [TestMethod]
        public async Task PostFile_NonMultipart_BadRequestFailure()
        {
            //Arrange
            var guid = Guid.NewGuid();
            var moq = new Mock<IFilesRepository>();
            moq.Setup(t => t.PostFileHead(It.IsAny<FsFile>())).ReturnsAsync(guid);
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqLog = new Mock<sl.IServiceLogRepository>();

            var httpContent = new StringContent("my file");
            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqConfigRepo.Object, moqLog.Object)
            {
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("http://localhost/files"),
                    Content = httpContent
                },
                Configuration = new HttpConfiguration()
            };

            HttpContext.Current = new HttpContext(
                new HttpRequest("", "http://tempuri.org", ""),
                new HttpResponse(new StringWriter())
                );

            controller.Configuration.Routes.MapHttpRoute(
                 name: "DefaultApi",
                 routeTemplate: "files/{id}",
                 defaults: new { id = RouteParameter.Optional });

            // Act
            // 1. Upload file
            var actionResult = await controller.PostFile();

            //Assert
            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [TestCategory("FileStoreTests.Post")]
        [TestMethod]
        public async Task PostFile_NonMultipartDateTimeExpired_BadRequestFailure()
        {
            //Arrange
            var guid = Guid.NewGuid();
            var moq = new Mock<IFilesRepository>();
            moq.Setup(t => t.PostFileHead(It.IsAny<FsFile>())).ReturnsAsync(guid);
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqLog = new Mock<sl.IServiceLogRepository>();

            var httpContent = new StringContent("my file");
            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqConfigRepo.Object, moqLog.Object)
            {
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("http://localhost/files"),
                    Content = httpContent
                },
                Configuration = new HttpConfiguration()
            };

            HttpContext.Current = new HttpContext(
                new HttpRequest("", "http://tempuri.org", ""),
                new HttpResponse(new StringWriter())
                );

            controller.Configuration.Routes.MapHttpRoute(
                 name: "DefaultApi",
                 routeTemplate: "files/{id}",
                 defaults: new { id = RouteParameter.Optional });

            // Act
            // 1. Upload file
            var actionResult = await controller.PostFile(DateTime.Now);

            //Assert
            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [TestCategory("FileStoreTests.Post")]
        [TestMethod]
        public async Task PostFile_HttpContextNull_InternalServerError()
        {
            //Arrange
            var guid = Guid.NewGuid();
            var moq = new Mock<IFilesRepository>();
            moq.Setup(t => t.PostFileHead(It.IsAny<FsFile>())).ReturnsAsync(guid);
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqLog = new Mock<sl.IServiceLogRepository>();

            var httpContent = new StringContent("my file");
            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqConfigRepo.Object, moqLog.Object)
            {
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("http://localhost/files"),
                    Content = httpContent
                },
                Configuration = new HttpConfiguration()
            };

            controller.Configuration.Routes.MapHttpRoute(
                 name: "DefaultApi",
                 routeTemplate: "files/{id}",
                 defaults: new { id = RouteParameter.Optional });

            // Act
            // 1. Upload file
            var actionResult = await controller.PostFile(DateTime.Now);

            //Assert
            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.InternalServerError);
        }

        [TestCategory("FileStoreTests.Post")]
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PostFile_MultipartRepoThrowsException()
        {
            //Arrange
            var moq = new Mock<IFilesRepository>();
            moq.Setup(t => t.PostFileHead(It.IsAny<FsFile>())).Throws(new Exception());
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqLog = new Mock<sl.IServiceLogRepository>();

            string fileName4Upload = "\"UploadTest.txt\"";
            string fileContent4Upload = "This is the content of the uploaded test file";

            MultipartFormDataContent multiPartContent = new MultipartFormDataContent("----MyGreatBoundary");
            ByteArrayContent byteArrayContent = new ByteArrayContent(Encoding.UTF8.GetBytes(fileContent4Upload));
            byteArrayContent.Headers.Add("Content-Type", "multipart/form-data");
            multiPartContent.Add(byteArrayContent, "this is the name of the content", fileName4Upload);

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqConfigRepo.Object, moqLog.Object)
            {
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("http://localhost/files"),
                    Content = multiPartContent
                },
                Configuration = new HttpConfiguration()
            };

            var context = await SetupMultipartPost(multiPartContent);


            controller.Configuration.Routes.MapHttpRoute(
                 name: "DefaultApi",
                 routeTemplate: "files/{id}",
                 defaults: new { id = RouteParameter.Optional });

            // Act
            // 1. Upload file
            await controller.PostFileHttpContext(context.Object, null);
        }

        private async Task<Mock<HttpContextWrapper>> SetupMultipartPost(MultipartFormDataContent multiPartContent)
        {
            HttpContext.Current = new HttpContext(
                new HttpRequest("", "http://tempuri.org", ""),
                new HttpResponse(new StringWriter())
                );

            var context = new Mock<HttpContextWrapper>(HttpContext.Current);
            var stream = await multiPartContent.ReadAsStreamAsync();

            context.Setup(c => c.Request.GetBufferlessInputStream()).Returns(stream);
            return context;
        }

    }
}
