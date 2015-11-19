using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Web.Http;
using System.Text;
using System.Linq;
using Moq;
using FileStore.Repositories;
using System.Threading.Tasks;
using System.Globalization;
using System.Net;
using FileStore.Models;

namespace FileStore.Controllers
{
	[TestClass]
	public class FilesControllerTests
	{
		[TestMethod]
		public void PostFile_MultipartSingleFile_Success()
		{
			//Arrange
			var guid = Guid.NewGuid();
			var moq = new Mock<IFilesRepository>();
			var moqFileStreamRepo = new Mock<IFileStreamRepository>();
			var moqFileMapper = new Mock<IFileMapperRepository>();
			var moqConfigRepo = new Mock<IConfigRepository>();

			moq.Setup(t => t.PostFileHead(It.IsAny<File>())).Returns(Task.FromResult<Guid?>(guid));

			string fileName4Upload = "UploadTest.txt";
			string fileContent4Upload = "This is the content of the uploaded test file";

			MultipartFormDataContent multiPartContent = new MultipartFormDataContent("----MyGreatBoundary");
			ByteArrayContent byteArrayContent = new ByteArrayContent(Encoding.UTF8.GetBytes(fileContent4Upload));
			byteArrayContent.Headers.Add("Content-Type", "multipart/form-data");
			multiPartContent.Add(byteArrayContent, "this is the name of the content", fileName4Upload);

			var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object, moqConfigRepo.Object)
			{
				Request = new HttpRequestMessage
				{
					RequestUri = new Uri("http://localhost/files"),
					Content = multiPartContent
				},
				Configuration = new HttpConfiguration()
			};

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

		[TestMethod]
		public void PostFile_MultipartMultipleFiles_BadRequestFailure()
		{
			//Arrange
			var guid = Guid.NewGuid();
			var moq = new Mock<IFilesRepository>();
			moq.Setup(t => t.PostFileHead(It.IsAny<File>())).Returns(Task.FromResult<Guid?>(guid));
			var moqFileStreamRepo = new Mock<IFileStreamRepository>();
			var moqFileMapper = new Mock<IFileMapperRepository>();
			var moqConfigRepo = new Mock<IConfigRepository>();

			string fileName4Upload = "UploadTest.txt";
			string fileContent4Upload = "This is the content of the uploaded test file";

			var multiPartContent1 = new MultipartFormDataContent("----MyGreatBoundary");
			ByteArrayContent byteArrayContent1 = new ByteArrayContent(Encoding.UTF8.GetBytes(fileContent4Upload));
			ByteArrayContent byteArrayContent2 = new ByteArrayContent(Encoding.UTF8.GetBytes(fileContent4Upload));
			byteArrayContent1.Headers.Add("Content-Type", "multipart/form-data");
			multiPartContent1.Add(byteArrayContent1, "this is the name of the content", fileName4Upload);
			multiPartContent1.Add(byteArrayContent2, "this is the name of the content", fileName4Upload);

			var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object, moqConfigRepo.Object)
			{
				Request = new HttpRequestMessage
				{
					RequestUri = new Uri("http://localhost/files"),
					Content = multiPartContent1
				},
				Configuration = new HttpConfiguration()
			};

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
			Assert.IsTrue(response.StatusCode == HttpStatusCode.BadRequest);
		}

		[TestMethod]
		public void PostFile_NonMultipart_BadRequestFailure()
		{
			//Arrange
			var guid = Guid.NewGuid();
			var moq = new Mock<IFilesRepository>();
			moq.Setup(t => t.PostFileHead(It.IsAny<File>())).Returns(Task.FromResult<Guid?>(guid));
			var moqFileStreamRepo = new Mock<IFileStreamRepository>();
			var moqFileMapper = new Mock<IFileMapperRepository>();
			var moqConfigRepo = new Mock<IConfigRepository>();

			var httpContent = new StringContent("my file");
			var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object, moqConfigRepo.Object)
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
			var actionResult = controller.PostFile().Result;

			//Assert
			System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
			HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;

			// Assert
			Assert.IsTrue(response.StatusCode == HttpStatusCode.BadRequest);
		}

		[TestMethod]
		public void PostFile_MultipartRepoThrowsException_InternalServerErrorFailure()
		{
			//Arrange
			var moq = new Mock<IFilesRepository>();
			moq.Setup(t => t.PostFile(It.IsAny<File>())).Throws(new Exception());
			var moqFileStreamRepo = new Mock<IFileStreamRepository>();
			var moqFileMapper = new Mock<IFileMapperRepository>();
			var moqConfigRepo = new Mock<IConfigRepository>();

			string fileName4Upload = "UploadTest.txt";
			string fileContent4Upload = "This is the content of the uploaded test file";

			MultipartFormDataContent multiPartContent = new MultipartFormDataContent("----MyGreatBoundary");
			ByteArrayContent byteArrayContent = new ByteArrayContent(Encoding.UTF8.GetBytes(fileContent4Upload));
			byteArrayContent.Headers.Add("Content-Type", "multipart/form-data");
			multiPartContent.Add(byteArrayContent, "this is the name of the content", fileName4Upload);

			var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object)
			{
				Request = new HttpRequestMessage
				{
					RequestUri = new Uri("http://localhost/files"),
					Content = multiPartContent
				},
				Configuration = new HttpConfiguration()
			};

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
			Assert.IsTrue(response.StatusCode == HttpStatusCode.InternalServerError);
		}

		[TestMethod]
		public void HeadFile_GetHeadForExistentFile_Success()
		{
			// Arrange
			var moq = new Mock<IFilesRepository>();
			var moqFileStreamRepo = new Mock<IFileStreamRepository>();
			var moqFileMapper = new Mock<IFileMapperRepository>();
			var moqConfigRepo = new Mock<IConfigRepository>();

			var file = new File
			{
				FileId = new Guid("33333333-3333-3333-3333-333333333333"),
				FileName = "Test3.txt",
				StoredTime = DateTime.ParseExact("2015-09-05T22:57:31.7824054-04:00", "o", CultureInfo.InvariantCulture),
				FileType = "text/html"
			};

			moq.Setup(t => t.GetFileHead(It.IsAny<Guid>())).Returns(Task.FromResult(file));

			var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object, moqConfigRepo.Object)
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

		[TestMethod]
		public void HeadFile_GetHeadForNonExistentFile_Failure()
		{
			// Arrange
			var moq = new Mock<IFilesRepository>();
			var moqFileStreamRepo = new Mock<IFileStreamRepository>();
			var moqFileMapper = new Mock<IFileMapperRepository>();
			var moqConfigRepo = new Mock<IConfigRepository>();

			var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object, moqConfigRepo.Object)
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
			var actionResult = controller.GetFile("33333333333333333333333333333333").Result;

			System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
			HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;

			// Assert
			Assert.IsTrue(response.StatusCode == HttpStatusCode.NotFound);
		}

		[TestMethod]
		public void HeadFile_ImproperGuid_FormatException()
		{
			// Arrange
			var moq = new Mock<IFilesRepository>();
			var moqFileStreamRepo = new Mock<IFileStreamRepository>();
			var moqFileMapper = new Mock<IFileMapperRepository>();
			var moqConfigRepo = new Mock<IConfigRepository>();

			var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object, moqConfigRepo.Object)
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
			var actionResult = controller.GetFile("333333333!@#@!@!@!33333333333333333333333").Result;

			System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
			HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;

			// Assert
			Assert.IsTrue(response.StatusCode == HttpStatusCode.BadRequest);
		}

		[TestMethod]
		public void HeadFile_UnknownException_InternalServerErrorFailure()
		{
			// Arrange
			var moq = new Mock<IFilesRepository>();
			moq.Setup(t => t.GetFileHead(It.IsAny<Guid>())).Throws(new Exception());
			var moqFileStreamRepo = new Mock<IFileStreamRepository>();
			var moqFileMapper = new Mock<IFileMapperRepository>();
			var moqConfigRepo = new Mock<IConfigRepository>();

			var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object, moqConfigRepo.Object)
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
			var actionResult = controller.GetFile("33333333333333333333333333333333").Result;

			System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
			HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;

			// Assert
			Assert.IsTrue(response.StatusCode == HttpStatusCode.InternalServerError);
		}

		[TestMethod]
		public void GetFile_ImproperGuid_FormatException()
		{
			// Arrange
			var moq = new Mock<IFilesRepository>();
			var moqFileStreamRepo = new Mock<IFileStreamRepository>();
			var moqFileMapper = new Mock<IFileMapperRepository>();
			var moqConfigRepo = new Mock<IConfigRepository>();

			var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object, moqConfigRepo.Object)
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
			var actionResult = controller.GetFile("333333333!@#@!@!@!33333333333333333333333").Result;

			System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
			HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;

			// Assert
			Assert.IsTrue(response.StatusCode == HttpStatusCode.BadRequest);
		}

		[TestMethod]
		public void GetFile_UnknownException_InternalServerErrorFailure()
		{
			// Arrange
			var moq = new Mock<IFilesRepository>();
			moq.Setup(t => t.GetFileHead(It.IsAny<Guid>())).Throws(new Exception());
			var moqFileStreamRepo = new Mock<IFileStreamRepository>();
			var moqFileMapper = new Mock<IFileMapperRepository>();
			var moqConfigRepo = new Mock<IConfigRepository>();

			var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object, moqConfigRepo.Object)
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
			var actionResult = controller.GetFile("33333333333333333333333333333333").Result;

			System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
			HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;

			// Assert
			Assert.IsTrue(response.StatusCode == HttpStatusCode.InternalServerError);
		}

		[TestMethod]
		public void GetFile_NonExistentFile_NotFoundFailure()
		{
			// Arrange
			var moq = new Mock<IFilesRepository>();
			moq.Setup(t => t.GetFileHead(It.IsAny<Guid>())).Returns(Task.FromResult<File>(null));
			var moqFileStreamRepo = new Mock<IFileStreamRepository>();
			var moqFileMapper = new Mock<IFileMapperRepository>();
			var moqConfigRepo = new Mock<IConfigRepository>();

			var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object, moqConfigRepo.Object)
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
			var actionResult = controller.GetFile("33333333333333333333333333333333").Result;

			System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
			HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;

			// Assert
			Assert.IsTrue(response.StatusCode == HttpStatusCode.NotFound);
		}

		[TestMethod]
		public void GetFile_ProperRequest_Success()
		{
			// Arrange
			var moq = new Mock<IFilesRepository>();
			var moqFileStreamRepo = new Mock<IFileStreamRepository>();
			var moqFileMapper = new Mock<IFileMapperRepository>();
			var moqConfigRepo = new Mock<IConfigRepository>();

			var file = new File
			{
				FileId = new Guid("22222222-2222-2222-2222-222222222222"),
				FileName = "Test2.txt",
				StoredTime = DateTime.ParseExact("2015-09-05T22:57:31.7824054-04:00", "o", CultureInfo.InvariantCulture),
				FileType = FileMapperRepository.DefaultMediaType
			};

			moq.Setup(t => t.GetFileHead(It.IsAny<Guid>())).Returns(Task.FromResult(file));
			moqFileMapper.Setup(t => t.GetMappedOutputContentType(It.IsAny<string>()))
				 .Returns(FileMapperRepository.DefaultMediaType);
			var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object, moqConfigRepo.Object)
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
		public void GetFile_NoFileRetrieved_Failure()
		{
			// Arrange
			var moq = new Mock<IFilesRepository>();
			var moqFileStreamRepo = new Mock<IFileStreamRepository>();
			var moqFileMapper = new Mock<IFileMapperRepository>();
			var moqConfigRepo = new Mock<IConfigRepository>();

			moq.Setup(t => t.GetFileHead(It.IsAny<Guid>())).Returns(Task.FromResult((File)null));
			moqFileStreamRepo.Setup(m => m.GetFile(It.IsAny<Guid>())).Returns((File)null);

			var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object, moqConfigRepo.Object)
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
			var actionResult = controller.GetFile("22222222222222222222222222222222").Result;

			System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
			HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;

			// Assert
			Assert.IsTrue(response.StatusCode == HttpStatusCode.NotFound);
		}

		[TestMethod]
		public void GetFile_NoFileRetrievedEmptyName_Failure()
		{
			// Arrange
			var moq = new Mock<IFilesRepository>();
			var moqFileStreamRepo = new Mock<IFileStreamRepository>();
			var moqFileMapper = new Mock<IFileMapperRepository>();
			var moqConfigRepo = new Mock<IConfigRepository>();

			File file = new File();
			moq.Setup(t => t.GetFileHead(It.IsAny<Guid>())).Returns(Task.FromResult((File)null));
			moqFileStreamRepo.Setup(m => m.GetFile(It.IsAny<Guid>())).Returns(file);

			var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object, moqConfigRepo.Object)
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
			var actionResult = controller.GetFile("22222222222222222222222222222222").Result;

			System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
			HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;

			// Assert
			Assert.IsTrue(response.StatusCode == HttpStatusCode.NotFound);
		}
	}
}
