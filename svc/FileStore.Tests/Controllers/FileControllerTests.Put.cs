using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using FileStore.Models;
using FileStore.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using File = FileStore.Models.File;
using sl = ServiceLibrary.Repositories.ConfigControl;

namespace FileStore.Controllers
{
    public partial class FilesControllerTests
    {

        [TestCategory("FileStoreTests.Put")]
        [TestMethod]
        public async Task PutFile_FileNotFound()
        {
            //Arrange
            var guid = Guid.NewGuid();
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqLog = new Mock<sl.IServiceLogRepository>();
            var httpContent = new StringContent("my file");

            moq.Setup(t => t.GetFileHead(It.IsAny<Guid>())).ReturnsAsync(null);
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
            var response = await controller.PutFileHttpContext(guid.ToString(), new HttpContextWrapper(HttpContext.Current));


            // Assert
            Assert.IsTrue(response.Status == HttpStatusCode.NotFound);
        }

        [TestCategory("FileStoreTests.Put")]
        [TestMethod]
        public async Task PutFile_FileChunkCount_Correct()
        {
            //Arrange
            HttpContext.Current = new HttpContext(
                new HttpRequest("", "http://tempuri.org", ""),
                new HttpResponse(new StringWriter())
                );

            var guid = Guid.NewGuid();
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqHttpContextWrapper = new Mock<HttpContextWrapper>(HttpContext.Current);
            var moqLog = new Mock<sl.IServiceLogRepository>();
            var file = new File { ChunkCount = 1, FileId = guid };
            var paramFileChunk = new FileChunk();
            var httpContent = "my file";
            HttpContent content = new ByteArrayContent(Encoding.UTF8.GetBytes(httpContent));
            var stream = await content.ReadAsStreamAsync();

            moq.Setup(t => t.PostFileHead(It.IsAny<File>())).ReturnsAsync(guid);
            moq.Setup(t => t.GetFileHead(It.IsAny<Guid>())).ReturnsAsync(file);
            moq.Setup(t => t.PostFileChunk(It.IsAny<FileChunk>()))
                .Callback<FileChunk>(chunk => paramFileChunk = chunk).
                ReturnsAsync(3);
            moq.Setup(t => t.UpdateFileHead(It.IsAny<Guid>(), It.IsAny<long>(), It.IsAny<int>())).Returns(Task.FromResult(0));

            moqHttpContextWrapper.Setup(c => c.Request.GetBufferlessInputStream()).Returns(stream);

            moqConfigRepo.Setup(t => t.FileChunkSize).Returns(DefaultChunkSize);

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqConfigRepo.Object, moqLog.Object)
            {
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("http://localhost/files"),
                    Content = content
                },
                Configuration = new HttpConfiguration()
            };

            controller.Configuration.Routes.MapHttpRoute(
                 name: "DefaultApi",
                 routeTemplate: "files/{id}",
                 defaults: new { id = RouteParameter.Optional });

            // Act
            // 1. Upload file
            var response = await controller.PutFileHttpContext(guid.ToString(), moqHttpContextWrapper.Object);


            // Assert
            Assert.IsTrue(response.Status == HttpStatusCode.OK);
            Assert.IsTrue(paramFileChunk.ChunkNum == file.ChunkCount + 2);
        }

        [TestCategory("FileStoreTests.Put")]
        [TestMethod]
        public async Task PutFile_HttpContextIsNull_InternalServerError()
        {
            var guid = Guid.NewGuid();
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqLog = new Mock<sl.IServiceLogRepository>();
            var httpContent = "my file";
            HttpContent content = new ByteArrayContent(Encoding.UTF8.GetBytes(httpContent));

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqConfigRepo.Object, moqLog.Object)
            {
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("http://localhost/files"),
                    Content = content
                },
                Configuration = new HttpConfiguration()
            };

            HttpContext.Current = null;


            controller.Configuration.Routes.MapHttpRoute(
                 name: "DefaultApi",
                 routeTemplate: "files/{id}",
                 defaults: new { id = RouteParameter.Optional });

            // Act
            // 1. Upload file
            var actionResult = await controller.PutFile(guid.ToString());

            //Assert
            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.InternalServerError);
        }

        [TestCategory("FileStoreTests.Put")]
        [TestMethod]
        public async Task PutFile_ExceptionThrows_InternalServerError()
        {
            var guid = Guid.NewGuid();
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqLog = new Mock<sl.IServiceLogRepository>();
            var httpContent = "my file";
            moq.Setup(t => t.GetFileHead(It.IsAny<Guid>())).Throws(new Exception());
            HttpContent content = new ByteArrayContent(Encoding.UTF8.GetBytes(httpContent));

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqConfigRepo.Object, moqLog.Object)
            {
                Request = new HttpRequestMessage
                {
                    RequestUri = new Uri("http://localhost/files"),
                    Content = content
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
            var actionResult = await controller.PutFile(guid.ToString());

            //Assert
            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.InternalServerError);
        }
    }
}
