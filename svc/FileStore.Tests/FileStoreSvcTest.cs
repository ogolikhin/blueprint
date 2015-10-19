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
using System.Globalization;
using System.Net;
using FileStore.Models;

namespace FileStore.Tests
{
    [TestClass]
    public class FileStoreSvcTest
    {
        [TestCategory("FileStoreSvc-UnitTests")]
        [TestMethod]
        public void PostFile_MultipartSingleFile_Success()
        {
            //Arrange
            var guid = Guid.NewGuid();
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqFileMapper = new Mock<IFileMapperRepository>();

            moq.Setup(t => t.PostFile(It.IsAny<Models.File>())).Returns(Task.FromResult<Guid?>(guid));

            string fileName4Upload = "UploadTest.txt";
            string fileContent4Upload = "This is the content of the uploaded test file";

            MultipartFormDataContent multiPartContent = new MultipartFormDataContent("----MyGreatBoundary");
            ByteArrayContent byteArrayContent = new ByteArrayContent(Encoding.UTF8.GetBytes(fileContent4Upload));
            byteArrayContent.Headers.Add("Content-Type", "multipart/form-data");
            multiPartContent.Add(byteArrayContent, "this is the name of the content", fileName4Upload);

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object);

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
            var actionResult = controller.PostFile().Result;

            //Assert
            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;

            var content = response.Content;
            var fileContent4Download = content.ReadAsStringAsync().Result;
            var contentType = content.Headers.ContentType;

            // Assert
            Assert.IsTrue(response.IsSuccessStatusCode);
        }

        [TestCategory("FileStoreSvc-UnitTests")]
        [TestMethod]
        public void PostFile_MultipartMultipleFiles_BadRequestFailure()
        {
            //Arrange
            var guid = Guid.NewGuid();
            var moq = new Mock<IFilesRepository>();
            moq.Setup(t => t.PostFile(It.IsAny<Models.File>())).Returns(Task.FromResult<Guid?>(guid));
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqFileMapper = new Mock<IFileMapperRepository>();

            string fileName4Upload = "UploadTest.txt";
            string fileContent4Upload = "This is the content of the uploaded test file";

            var multiPartContent1 = new MultipartFormDataContent("----MyGreatBoundary");
            ByteArrayContent byteArrayContent1 = new ByteArrayContent(Encoding.UTF8.GetBytes(fileContent4Upload));
            ByteArrayContent byteArrayContent2 = new ByteArrayContent(Encoding.UTF8.GetBytes(fileContent4Upload));
            byteArrayContent1.Headers.Add("Content-Type", "multipart/form-data");
            multiPartContent1.Add(byteArrayContent1, "this is the name of the content", fileName4Upload);            
            multiPartContent1.Add(byteArrayContent2, "this is the name of the content", fileName4Upload);

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object);

            controller.Request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost/files"),
                Content = multiPartContent1
            };

            controller.Configuration = new HttpConfiguration();
            controller.Configuration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "files/{id}",
                defaults: new { id = RouteParameter.Optional });

            // Act
            // 1. Upload file
            var actionResult = controller.PostFile().Result;

            //Assert
            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;

            var content = response.Content;

            // Assert
            Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.BadRequest);
        }

        [TestCategory("FileStoreSvc-UnitTests")]
        [TestMethod]
        public void PostFile_NonMultipart_BadRequestFailure()
        {
            //Arrange
            var guid = Guid.NewGuid();
            var moq = new Mock<IFilesRepository>();
            moq.Setup(t => t.PostFile(It.IsAny<Models.File>())).Returns(Task.FromResult<Guid?>(guid));
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqFileMapper = new Mock<IFileMapperRepository>();

            var httpContent = new StringContent("my file");
            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object);

            controller.Request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost/files"),
                Content = httpContent
            };

            controller.Configuration = new HttpConfiguration();
            controller.Configuration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "files/{id}",
                defaults: new { id = RouteParameter.Optional });

            // Act
            // 1. Upload file
            var actionResult = controller.PostFile().Result;

            //Assert
            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;

            // Assert
            Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.BadRequest);
        }

        [TestCategory("FileStoreSvc-UnitTests")]
        [TestMethod]
        public void PostFile_MultipartRepoThrowsException_InternalServerErrorFailure()
        {
            //Arrange
            var moq = new Mock<IFilesRepository>();
            moq.Setup(t => t.PostFile(It.IsAny<File>())).Throws(new Exception());
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqFileMapper = new Mock<IFileMapperRepository>();

            string fileName4Upload = "UploadTest.txt";
            string fileContent4Upload = "This is the content of the uploaded test file";

            MultipartFormDataContent multiPartContent = new MultipartFormDataContent("----MyGreatBoundary");
            ByteArrayContent byteArrayContent = new ByteArrayContent(Encoding.UTF8.GetBytes(fileContent4Upload));
            byteArrayContent.Headers.Add("Content-Type", "multipart/form-data");
            multiPartContent.Add(byteArrayContent, "this is the name of the content", fileName4Upload);

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object);

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
            var actionResult = controller.PostFile().Result;

            //Assert
            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;

            // Assert
            Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.InternalServerError);
        }

        [TestCategory("FileStoreSvc-UnitTests")]
        [TestMethod]
        public void HeadFile_GetHeadForExistentFile_Success()
        {
            // Arrange
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqFileMapper = new Mock<IFileMapperRepository>();

            var file = new File();
            file.FileId = new Guid("33333333-3333-3333-3333-333333333333");
            file.FileName = "Test3.txt";
            file.FileContent = Encoding.UTF8.GetBytes("Test3 content");
            file.StoredTime = DateTime.ParseExact("2015-09-05T22:57:31.7824054-04:00", "o", CultureInfo.InvariantCulture);
            file.FileType = "text/html";

            moq.Setup(t => t.HeadFile(It.IsAny<Guid>())).Returns(Task.FromResult(file));

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object);

            controller.Request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost/files"),
                Method = HttpMethod.Head
            };

            controller.Configuration = new HttpConfiguration();
            controller.Configuration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "files/{id}",
                defaults: new { id = RouteParameter.Optional });

            // Act
            var actionResult = controller.GetFile("33333333333333333333333333333333").Result;

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

        [TestCategory("FileStoreSvc-UnitTests")]
        [TestMethod]
        public void HeadFile_GetHeadForNonExistentFile_Failure()
        {
            // Arrange
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqFileMapper = new Mock<IFileMapperRepository>();

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object);

            controller.Request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost/files"),
                Method = HttpMethod.Head
            };

            controller.Configuration = new HttpConfiguration();
            controller.Configuration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "files/{id}",
                defaults: new { id = RouteParameter.Optional });

            // Act
            var actionResult = controller.GetFile("33333333333333333333333333333333").Result;

            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;

            // Assert
            Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.NotFound);
        }

        [TestCategory("FileStoreSvc-UnitTests")]
        [TestMethod]
        public void HeadFile_ImproperGuid_FormatException()
        {
            // Arrange
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqFileMapper = new Mock<IFileMapperRepository>();

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object);

            controller.Request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost/files"),
                Method = HttpMethod.Head
            };

            controller.Configuration = new HttpConfiguration();
            controller.Configuration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "files/{id}",
                defaults: new { id = RouteParameter.Optional });

            // Act
            var actionResult = controller.GetFile("333333333!@#@!@!@!33333333333333333333333").Result;

            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;

            // Assert
            Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.BadRequest);
        }

        [TestCategory("FileStoreSvc-UnitTests")]
        [TestMethod]
        public void HeadFile_UnknownException_InternalServerErrorFailure()
        {
            // Arrange
            var moq = new Mock<IFilesRepository>();
            moq.Setup(t => t.HeadFile(It.IsAny<Guid>())).Throws(new Exception());
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqFileMapper = new Mock<IFileMapperRepository>();

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object);

            controller.Request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost/files"),
                Method = HttpMethod.Head
            };

            controller.Configuration = new HttpConfiguration();
            controller.Configuration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "files/{id}",
                defaults: new { id = RouteParameter.Optional });

            // Act
            var actionResult = controller.GetFile("33333333333333333333333333333333").Result;

            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;

            // Assert
            Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.InternalServerError);
        }

        [TestCategory("FileStoreSvc-UnitTests")]
        [TestMethod]
        public void GetFile_ImproperGuid_FormatException()
        {
            // Arrange
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqFileMapper = new Mock<IFileMapperRepository>();

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object);

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
            var actionResult = controller.GetFile("333333333!@#@!@!@!33333333333333333333333").Result;

            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;

            // Assert
            Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.BadRequest);
        }

        [TestCategory("FileStoreSvc-UnitTests")]
        [TestMethod]
        public void GetFile_UnknownException_InternalServerErrorFailure()
        {
            // Arrange
            var moq = new Mock<IFilesRepository>();
            moq.Setup(t => t.GetFile(It.IsAny<Guid>())).Throws(new Exception());
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqFileMapper = new Mock<IFileMapperRepository>();

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object);

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
            var actionResult = controller.GetFile("33333333333333333333333333333333").Result;

            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;

            // Assert
            Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.InternalServerError);
        }

        [TestCategory("FileStoreSvc-UnitTests")]
        [TestMethod]
        public void GetFile_NonExistentFile_NotFoundFailure()
        {
            // Arrange
            var moq = new Mock<IFilesRepository>();
            moq.Setup(t => t.GetFile(It.IsAny<Guid>())).Returns(Task.FromResult<File>(null));
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqFileMapper = new Mock<IFileMapperRepository>();

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object);

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
            var actionResult = controller.GetFile("33333333333333333333333333333333").Result;

            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;

            // Assert
            Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.NotFound);
        }

        [TestCategory("FileStoreSvc-UnitTests")]
        [TestMethod]
        public void GetFile_ProperRequest_Success()
        {
            // Arrange
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqFileMapper = new Mock<IFileMapperRepository>();

            var file = new Models.File();
            file.FileId = new Guid("22222222-2222-2222-2222-222222222222");
            file.FileName = "Test2.txt";
            file.FileContent = Encoding.UTF8.GetBytes("Test2 content");
            file.StoredTime = DateTime.ParseExact("2015-09-05T22:57:31.7824054-04:00", "o", CultureInfo.InvariantCulture);
            file.FileType = FileMapperRepository.DefaultMediaType;

            moq.Setup(t => t.GetFile(It.IsAny<Guid>())).Returns(Task.FromResult(file));
            moqFileMapper.Setup(t => t.GetMappedOutputContentType(It.IsAny<string>()))
                .Returns(FileMapperRepository.DefaultMediaType);
            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object);
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

        [TestCategory("FileStoreSvc-UnitTests")]
        [TestMethod]
        public void GetFile_NoFileRetrieved_Failure()
        {
            // Arrange
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqFileMapper = new Mock<IFileMapperRepository>();

            Models.File file = null;
            moq.Setup(t => t.GetFile(It.IsAny<Guid>())).Returns(Task.FromResult(file));
            moqFileStreamRepo.Setup(m => m.GetFile(It.IsAny<Guid>())).Returns(file);

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object);
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
            
            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.NotFound);
        }

        [TestCategory("FileStoreSvc-UnitTests")]
        [TestMethod]
        public void GetFile_NoFileRetrievedEmptyName_Failure()
        {
            // Arrange
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqFileMapper = new Mock<IFileMapperRepository>();

            Models.File file = new File();
            moq.Setup(t => t.GetFile(It.IsAny<Guid>())).Returns(Task.FromResult((Models.File)null));
            moqFileStreamRepo.Setup(m => m.GetFile(It.IsAny<Guid>())).Returns(file);

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object);
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

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.NotFound);
        }

        [ExpectedException(typeof(NotSupportedException))]
        [TestCategory("FileStoreSvc-UnitTests")]
        [TestMethod]
        public void DeleteFile_ImproperGuid_FormatException()
        {
            // Arrange
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqFileMapper = new Mock<IFileMapperRepository>();

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object);

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
            var actionResult = controller.DeleteFile("333333333!@#@!@!@!33333333333333333333333").Result;

            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;

            // Assert
            Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.BadRequest);
        }

        [ExpectedException(typeof(NotSupportedException))]
        [TestCategory("FileStoreSvc-UnitTests")]
        [TestMethod]
        public void DeleteFile_UnknownException_InternalServerErrorFailure()
        {
            // Arrange
            var moq = new Mock<IFilesRepository>();
            moq.Setup(t => t.DeleteFile(It.IsAny<Guid>())).Throws(new Exception());
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqFileMapper = new Mock<IFileMapperRepository>();

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object);

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

            // Assert
            Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.InternalServerError);
        }

        [ExpectedException(typeof(NotSupportedException))]
        [TestCategory("FileStoreSvc-UnitTests")]
        [TestMethod]
        public void DeleteFile_NonExistentFile_NotFoundFailure()
        {
            // Arrange
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqFileMapper = new Mock<IFileMapperRepository>();

            var file = new File();
            file.FileId = new Guid("33333333-3333-3333-3333-333333333333");
            file.FileName = "Test3.txt";
            file.FileContent = Encoding.UTF8.GetBytes("Test3 content");
            file.StoredTime = DateTime.ParseExact("2015-09-05T22:57:31.7824054-04:00", "o", CultureInfo.InvariantCulture);
            file.FileType = "text/html";

            moq.Setup(t => t.DeleteFile(It.IsAny<Guid>())).Returns(Task.FromResult<Guid?>(null));

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object);
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

            // Assert
            Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.NotFound);
        }

        [ExpectedException(typeof(NotSupportedException))]
        [TestCategory("FileStoreSvc-UnitTests")]
        [TestMethod]
        public void DeleteFile_ProperRequest_Success()
        {
            // Arrange
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqFileMapper = new Mock<IFileMapperRepository>();

            var file = new File();
            file.FileId = new Guid("33333333-3333-3333-3333-333333333333");
            file.FileName = "Test3.txt";
            file.FileContent = Encoding.UTF8.GetBytes("Test3 content");
            file.StoredTime = DateTime.ParseExact("2015-09-05T22:57:31.7824054-04:00", "o", CultureInfo.InvariantCulture);
            file.FileType = "text/html";

            moq.Setup(t => t.DeleteFile(It.IsAny<Guid>())).Returns(Task.FromResult<Guid?>(Guid.Parse("33333333333333333333333333333333")));

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object);
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
