using System;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
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

namespace FileStore.Controllers
{
	[TestClass]
	public class FilesControllerTests
	{
	    private const int DefaultChunkSize = 1048576; // 1mb chunk size
        #region Post unit tests
		[TestMethod]
		public async Task PostFile_MultipartSingleFile_Success()
		{
			//Arrange
			var guid = Guid.NewGuid();
			var moq = new Mock<IFilesRepository>();
			var moqFileStreamRepo = new Mock<IFileStreamRepository>();
			var moqFileMapper = new Mock<IFileMapperRepository>();
			var moqConfigRepo = new Mock<IConfigRepository>();

            moq.Setup(t => t.PostFileHead(It.IsAny<File>())).ReturnsAsync(guid);

			string fileName4Upload = "\"UploadTest.txt\"";
			string fileContent4Upload = "This is the content of the uploaded test file";

			MultipartFormDataContent multiPartContent = new MultipartFormDataContent("----MyGreatBoundary");
			ByteArrayContent byteArrayContent = new ByteArrayContent(Encoding.UTF8.GetBytes(fileContent4Upload));
			byteArrayContent.Headers.Add("Content-Type", "multipart/form-data");
			multiPartContent.Add(byteArrayContent, "this is the name of the content", fileName4Upload);

		    moqConfigRepo.Setup(t => t.FileChunkSize).Returns(DefaultChunkSize);

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object, moqConfigRepo.Object)
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
            var actionResult = await controller.PostFileHttpContext(context.Object, null);

			//Assert
			System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);

			var content = response.Content;
            var fileContent4Download = await content.ReadAsStringAsync();
			var contentType = content.Headers.ContentType;

			// Assert
			Assert.IsTrue(response.IsSuccessStatusCode);
		}

		[TestMethod]
		public async Task PostFile_MultipartMultipleFiles_BadRequestFailure()
		{
			//Arrange
			var guid = Guid.NewGuid();
			var moq = new Mock<IFilesRepository>();
            moq.Setup(t => t.PostFileHead(It.IsAny<File>())).ReturnsAsync(guid);
			var moqFileStreamRepo = new Mock<IFileStreamRepository>();
			var moqFileMapper = new Mock<IFileMapperRepository>();
			var moqConfigRepo = new Mock<IConfigRepository>();

			string fileName4Upload = "\"UploadTest.txt\"";
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

            var context = await SetupMultipartPost(multiPartContent1);


			controller.Configuration.Routes.MapHttpRoute(
				 name: "DefaultApi",
				 routeTemplate: "files/{id}",
				 defaults: new { id = RouteParameter.Optional });

			// Act
			// 1. Upload file
            var actionResult = await controller.PostFileHttpContext(context.Object, null);

			//Assert
			System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);

			var content = response.Content;

			// Assert
			Assert.IsTrue(response.StatusCode == HttpStatusCode.BadRequest);
		}

		[TestMethod]
        public async Task PostFile_NonMultipart_BadRequestFailure()
		{
			//Arrange
			var guid = Guid.NewGuid();
			var moq = new Mock<IFilesRepository>();
            moq.Setup(t => t.PostFileHead(It.IsAny<File>())).ReturnsAsync(guid);
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
            var actionResult = await controller.PostFile(null);

			//Assert
			System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);

			// Assert
			Assert.IsTrue(response.StatusCode == HttpStatusCode.BadRequest);
		}

		[TestMethod]
		public async Task PostFile_MultipartRepoThrowsException_InternalServerErrorFailure()
		{
			//Arrange
			var moq = new Mock<IFilesRepository>();
			moq.Setup(t => t.PostFileHead(It.IsAny<File>())).Throws(new Exception());
			var moqFileStreamRepo = new Mock<IFileStreamRepository>();
			var moqFileMapper = new Mock<IFileMapperRepository>();
			var moqConfigRepo = new Mock<IConfigRepository>();

			string fileName4Upload = "\"UploadTest.txt\"";
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

            var context = await SetupMultipartPost(multiPartContent);

			controller.Configuration.Routes.MapHttpRoute(
				 name: "DefaultApi",
				 routeTemplate: "files/{id}",
				 defaults: new { id = RouteParameter.Optional });

			// Act
			// 1. Upload file
            var actionResult = await controller.PostFileHttpContext(context.Object, null);

			//Assert
			System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);

			// Assert
			Assert.IsTrue(response.StatusCode == HttpStatusCode.InternalServerError);
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

        #endregion Post unit tests

        #region Put unit tests
        [TestMethod]
	    public void PutFile_FileNotFound()
	    {
            //Arrange
            var guid = Guid.NewGuid();
            var moq = new Mock<IFilesRepository>();
            var moqFileStreamRepo = new Mock<IFileStreamRepository>();
            var moqFileMapper = new Mock<IFileMapperRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var httpContent = new StringContent("my file");

            moq.Setup(t => t.GetFileHead(It.IsAny<Guid>())).Returns(Task.FromResult((File)null));
            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object, moqConfigRepo.Object)
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
            var actionResult = controller.PutFileHttpContext(guid.ToString(), new HttpContextWrapper(HttpContext.Current)).Result;

            //Assert
            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.NotFound);
        }

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
            var moqFileMapper = new Mock<IFileMapperRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqHttpContextWrapper = new Mock<HttpContextWrapper>(HttpContext.Current);
            var file = new File {ChunkCount = 1, FileId = guid};
            var paramFileChunk = new FileChunk();
            var httpContent = "my file";
            HttpContent content = new ByteArrayContent(Encoding.UTF8.GetBytes(httpContent));
            var stream = await content.ReadAsStreamAsync();

            moq.Setup(t => t.PostFileHead(It.IsAny<File>())).Returns(Task.FromResult<Guid>(guid));
            moq.Setup(t => t.GetFileHead(It.IsAny<Guid>())).Returns(Task.FromResult(file));
            moq.Setup(t => t.PostFileChunk(It.IsAny<FileChunk>()))
                .Callback<FileChunk>((chunk)=>paramFileChunk = chunk).
                Returns(Task.FromResult(3));
            moq.Setup(t => t.UpdateFileHead(It.IsAny<Guid>(), It.IsAny<long>(), It.IsAny<int>())).Returns(Task.FromResult(0));

            moqHttpContextWrapper.Setup(c => c.Request.GetBufferlessInputStream()).Returns(stream);

            moqConfigRepo.Setup(t => t.FileChunkSize).Returns(DefaultChunkSize);

            var controller = new FilesController(moq.Object, moqFileStreamRepo.Object, moqFileMapper.Object, moqConfigRepo.Object)
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
            var actionResult = controller.PutFileHttpContext(guid.ToString(), moqHttpContextWrapper.Object).Result;

            //Assert
            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;

            // Assert
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
            Assert.IsTrue(paramFileChunk.ChunkNum == file.ChunkCount + 2);
        }
        #endregion Put unit tests

        [TestMethod]
        public async Task HeadFile_GetHeadForExistentFile_Success()
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
				FileType = "text/html",
                ChunkCount = 1
			};

            moq.Setup(t => t.GetFileHead(It.IsAny<Guid>())).ReturnsAsync(file);

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
            var actionResult = await controller.GetFileHead("33333333333333333333333333333333");

			System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);
			var content = response.Content;
            var fileContent = await content.ReadAsStringAsync();
			var contentType = content.Headers.ContentType;
			var fileName = content.Headers.ContentDisposition.FileName;
			var storedTime = response.Headers.GetValues("Stored-Date");

			// Assert
			Assert.IsTrue(response.IsSuccessStatusCode);
			Assert.IsTrue(fileName == "Test3.txt");
			Assert.IsTrue(storedTime.First() == "2015-09-05T22:57:31.7824054-04:00");
		}

		[TestMethod]
        public async Task HeadFile_GetHeadForNonExistentFile_Failure()
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
            var actionResult = await controller.GetFileHead("33333333333333333333333333333333");

			System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);

			// Assert
			Assert.IsTrue(response.StatusCode == HttpStatusCode.NotFound);
		}

		[TestMethod]
        public async Task HeadFile_ImproperGuid_FormatException()
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
            var actionResult = await controller.GetFileHead("333333333!@#@!@!@!33333333333333333333333");

			System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);

			// Assert
			Assert.IsTrue(response.StatusCode == HttpStatusCode.BadRequest);
		}

		[TestMethod]
        public async Task HeadFile_UnknownException_InternalServerErrorFailure()
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
            var actionResult = await controller.GetFileHead("33333333333333333333333333333333");

			System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);

			// Assert
			Assert.IsTrue(response.StatusCode == HttpStatusCode.InternalServerError);
		}

		[TestMethod]
        public async Task GetFile_ImproperGuid_FormatException()
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
            var actionResult = await controller.GetFileContent("333333333!@#@!@!@!33333333333333333333333");

			System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);

			// Assert
			Assert.IsTrue(response.StatusCode == HttpStatusCode.BadRequest);
		}

		[TestMethod]
        public async Task GetFile_UnknownException_InternalServerErrorFailure()
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
            var actionResult = await controller.GetFileContent("33333333333333333333333333333333");

			System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);

			// Assert
			Assert.IsTrue(response.StatusCode == HttpStatusCode.InternalServerError);
		}

		[TestMethod]
        public async Task GetFile_NonExistentFile_NotFoundFailure()
		{
			// Arrange
			var moq = new Mock<IFilesRepository>();
            moq.Setup(t => t.GetFileHead(It.IsAny<Guid>())).ReturnsAsync(null);
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
            var actionResult = await controller.GetFileContent("33333333333333333333333333333333");

			System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);

			// Assert
			Assert.IsTrue(response.StatusCode == HttpStatusCode.NotFound);
		}
        
        [TestMethod]
        public async Task GetFile_ProperRequest_Success()
		{
			// Arrange
			var moq = new Mock<IFilesRepository>();
			var moqFileStreamRepo = new Mock<IFileStreamRepository>();
			var moqFileMapper = new Mock<IFileMapperRepository>();
			var moqConfigRepo = new Mock<IConfigRepository>();
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
                StoredTime = DateTime.ParseExact("2015-09-05T22:57:31.7824054-04:00", "o", CultureInfo.InvariantCulture),
				FileType = FileMapperRepository.DefaultMediaType,
                FileSize = fileChunk.ChunkSize,
                ChunkCount = 1
            };

            var moqDbConnection = new Mock<DbConnection>();

            moq.Setup(t => t.CreateConnection()).Returns(moqDbConnection.Object);

            moq.Setup(t => t.GetFileHead(It.IsAny<Guid>())).ReturnsAsync(file);
            moq.Setup(t => t.GetFileInfo(It.IsAny<Guid>())).Returns(file);

            moq.Setup(t => t.ReadChunkContent(moqDbConnection.Object, file.FileId, 1)).Returns(fileChunk.ChunkContent);

            moqFileMapper.Setup(t => t.GetMappedOutputContentType(It.IsAny<string>()))
				 .Returns(FileMapperRepository.DefaultMediaType);
		    moqConfigRepo.Setup(t => t.FileChunkSize).Returns(1*1024*1024);

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

		[TestMethod]
        public async Task GetFile_NoFileRetrieved_Failure()
		{
			// Arrange
			var moq = new Mock<IFilesRepository>();
			var moqFileStreamRepo = new Mock<IFileStreamRepository>();
			var moqFileMapper = new Mock<IFileMapperRepository>();
			var moqConfigRepo = new Mock<IConfigRepository>();

            moq.Setup(t => t.GetFileHead(It.IsAny<Guid>())).ReturnsAsync((File)null);
            // #DEBUG

            //moqFileStreamRepo.Setup(m => m.GetFileContent(It.IsAny<Guid>())).Returns((System.IO.Stream)null);

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
            var actionResult = await controller.GetFileContent("22222222222222222222222222222222");

			System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);

			// Assert
			Assert.IsTrue(response.StatusCode == HttpStatusCode.NotFound);
		}

		[TestMethod]
        public async Task GetFile_NoFileRetrievedEmptyName_Failure()
		{
			// Arrange
			var moq = new Mock<IFilesRepository>();
			var moqFileStreamRepo = new Mock<IFileStreamRepository>();
			var moqFileMapper = new Mock<IFileMapperRepository>();
			var moqConfigRepo = new Mock<IConfigRepository>();

			File file = new File();
            moq.Setup(t => t.GetFileHead(It.IsAny<Guid>())).ReturnsAsync((File)null);
            
            // #DEBUG
            // moqFileStreamRepo.Setup(m => m.GetFileContent(It.IsAny<Guid>())).Returns((System.IO.Stream)null);

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
            var actionResult = await controller.GetFileContent("22222222222222222222222222222222");

			System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = await actionResult.ExecuteAsync(cancellationToken);

			// Assert
			Assert.IsTrue(response.StatusCode == HttpStatusCode.NotFound);
		}
	}
}
