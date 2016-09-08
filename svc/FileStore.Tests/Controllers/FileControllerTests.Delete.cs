using FileStore.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using sl = ServiceLibrary.Repositories.ConfigControl;

namespace FileStore.Controllers
{
    public partial class FilesControllerTests
    {
        [TestCategory("FileStoreTests.Delete")]
        [TestMethod]
        public void DeleteFile_FormatException_BadRequest()
        {
            //Arrange
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqLog = new Mock<sl.IServiceLogRepository>();
            moq.Setup(t => t.DeleteFile(It.IsAny<Guid>(), DateTime.UtcNow)).Throws(new FormatException());

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


            //ct
            var result = controller.DeleteFile("");

            //Assert
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestResult), "Result should be BadRequestResult");
        }

        [TestCategory("FileStoreTests.Delete")]
        [TestMethod]
        public void DeleteFile_Exception_InternalServerError()
        {
            //Arrange
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqLog = new Mock<sl.IServiceLogRepository>();

            moq.Setup(t => t.DeleteFile(It.IsAny<Guid>(), It.IsAny<DateTime?>())).Throws(new Exception());

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

            //Assert
            var result = controller.DeleteFile(Guid.NewGuid().ToString("N"));

            //Act
            Assert.IsInstanceOfType(result.Result, typeof(ExceptionResult), "Result should be ExceptionResult");

        }

        [TestCategory("FileStoreTests.Delete")]
        [TestMethod]
        public void DeleteFile_FileNotFound()
        {
            //Arrange
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqLog = new Mock<sl.IServiceLogRepository>();

            moq.Setup(t => t.DeleteFile(It.IsAny<Guid>(), It.IsAny<DateTime?>())).Returns(Task.FromResult((Guid?)null));

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

            //Assert
            var result = controller.DeleteFile(Guid.NewGuid().ToString("N"));

            //Act
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult), "Result should be NotFound");

        }

        [TestCategory("FileStoreTests.Delete")]
        [TestMethod]
        public void DeleteFile_FileFound_Successfull()
        {
            //Arrange
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqLog = new Mock<sl.IServiceLogRepository>();
            var guid = Guid.NewGuid();

            moq.Setup(t => t.DeleteFile(It.IsAny<Guid>(), It.IsAny<DateTime?>())).Returns(Task.FromResult((Guid?)guid));

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

            //Assert
            var result = controller.DeleteFile(guid.ToString("N"));

            //Act
            var okNegotiatedContentResult = result.Result as OkNegotiatedContentResult<string>;

            Assert.IsNotNull(okNegotiatedContentResult);
            Assert.AreEqual(guid.ToString("N"), okNegotiatedContentResult.Content, "Guid returned is not same as supplied");

        }

        [TestCategory("FileStoreTests.Delete")]
        [TestMethod]
        public void DeleteFile_FileFoundDateSupplied_Successfull()
        {
            //Arrange
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqLog = new Mock<sl.IServiceLogRepository>();
            var guid = Guid.NewGuid();

            moq.Setup(t => t.DeleteFile(It.IsAny<Guid>(), It.IsAny<DateTime?>())).Returns(Task.FromResult((Guid?)guid));

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

            //Assert
            var result = controller.DeleteFile(guid.ToString("N"), DateTime.Now);

            //Act
            var okNegotiatedContentResult = result.Result as OkNegotiatedContentResult<string>;

            Assert.IsNotNull(okNegotiatedContentResult);
            Assert.AreEqual(guid.ToString("N"), okNegotiatedContentResult.Content, "Guid returned is not same as supplied");

        }

        [TestCategory("FileStoreTests.Delete")]
        [TestMethod]
        public void DeleteFile_FileFoundHistoricalDateSupplied_Successfull()
        {
            //Arrange
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqLog = new Mock<sl.IServiceLogRepository>();
            var guid = Guid.NewGuid();

            moq.Setup(t => t.DeleteFile(It.IsAny<Guid>(), It.IsAny<DateTime?>())).Returns(Task.FromResult((Guid?)guid));

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

            //Assert
            var result = controller.DeleteFile(guid.ToString("N"), DateTime.Now.AddDays(-10));

            //Act
            var okNegotiatedContentResult = result.Result as OkNegotiatedContentResult<string>;

            Assert.IsNotNull(okNegotiatedContentResult);
            Assert.AreEqual(guid.ToString("N"), okNegotiatedContentResult.Content, "Guid returned is not same as supplied");

        }

        [TestCategory("FileStoreTests.Delete")]
        [TestMethod]
        public void DeleteFile_FileFoundHFutureDateSupplied_Successfull()
        {
            //Arrange
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqLog = new Mock<sl.IServiceLogRepository>();
            var guid = Guid.NewGuid();

            moq.Setup(t => t.DeleteFile(It.IsAny<Guid>(), It.IsAny<DateTime?>())).Returns(Task.FromResult((Guid?)guid));

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

            //Assert
            var result = controller.DeleteFile(guid.ToString("N"), DateTime.Now.AddDays(10));

            //Act
            var okNegotiatedContentResult = result.Result as OkNegotiatedContentResult<string>;

            Assert.IsNotNull(okNegotiatedContentResult);
            Assert.AreEqual(guid.ToString("N"), okNegotiatedContentResult.Content, "Guid returned is not same as supplied");

        }
    }
}
