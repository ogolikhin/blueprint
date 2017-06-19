using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories.Files;
using System.Text;

namespace ServiceLibrary.Repositories
{
    [TestClass]
    public class FileRepositoryTests
    {
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "ServiceLibrary.Repositories.Files.FileRepository")]
        [TestMethod]
        public void Constructor_NoHttpWebClient_ThrowsArgumentNullException()
        {
            // Arrange
            ArgumentNullException exception = null;

            try
            {
                // Act
                new FileRepository(null);
            }
            catch (ArgumentNullException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
        }

        [TestMethod]
        public async Task GetFileAsync_UnauthenticatedAccess_ThrowsAuthenticationExceptionException()
        {
            // Arrange
            var fileId = Guid.NewGuid();
            var httpWebClient = CreateHttpWebClientClient(CreateHttpWebResponse(HttpStatusCode.Unauthorized));
            var fileRepository = new FileRepository(httpWebClient);
            AuthenticationException exception = null;

            // Act
            try
            {
                await fileRepository.GetFileAsync(fileId);
            }
            catch (AuthenticationException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorCodes.UnauthorizedAccess, exception.ErrorCode);
        }

        [TestMethod]
        public async Task GetFileAsync_FileNotFound_ThrowsResourceNotFoundException()
        {
            // Arrange
            var fileId = Guid.NewGuid();
            var httpWebClient = CreateHttpWebClientClient(CreateHttpWebResponse(HttpStatusCode.NotFound));
            var fileRepository = new FileRepository(httpWebClient);
            ResourceNotFoundException exception = null;

            // Act
            try
            {
                await fileRepository.GetFileAsync(fileId);
            }
            catch (ResourceNotFoundException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorCodes.ResourceNotFound, exception.ErrorCode);
        }

        [TestMethod]
        public async Task GetFileAsync_FileStoreServiceUnavailable_ThrowsResourceNotFoundException()
        {
            // Arrange
            var fileId = Guid.NewGuid();
            var httpWebClient = CreateHttpWebClientClient(CreateHttpWebResponse(HttpStatusCode.ServiceUnavailable));
            var fileRepository = new FileRepository(httpWebClient);
            ResourceNotFoundException exception = null;

            // Act
            try
            {
                await fileRepository.GetFileAsync(fileId);
            }
            catch (ResourceNotFoundException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorCodes.ResourceNotFound, exception.ErrorCode);
        }

        [TestMethod]
        public async Task GetFileAsync_FileStoreError_ThrowsException()
        {
            // Arrange
            var fileId = Guid.NewGuid();
            var httpWebClient = CreateHttpWebClientClient(CreateHttpWebResponse(HttpStatusCode.InternalServerError));
            var fileRepository = new FileRepository(httpWebClient);
            Exception exception = null;

            // Act
            try
            {
                await fileRepository.GetFileAsync(fileId);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
        }

        [TestMethod]
        public async Task GetFileAsync_ExistingFile_ReturnsCorrectFileInfo()
        {
            // Arrange
            var fileId = Guid.NewGuid();
            const string fileName = "test.zip";
            const string type = "application/zip";
            const int size = 2936320;
            const int chunkCount = 3;
            var storedDate = new DateTime(2017, 1, 1, 1, 1, 1);
            var httpWebClient = CreateHttpWebClientClient(CreateFileHttpWebResponse(fileName, type, size, chunkCount, storedDate));
            var fileRepository = new FileRepository(httpWebClient);

            // Act
            var file = await fileRepository.GetFileAsync(fileId);

            // Assert
            Assert.AreEqual(fileName, file.Info.Name);
            Assert.AreEqual(type, file.Info.Type);
            Assert.AreEqual(size, file.Info.Size);
            Assert.AreEqual(storedDate, file.Info.StoredDate);
            Assert.AreEqual(chunkCount, file.Info.ChunkCount);
        }

        [TestMethod]
        public async Task GetFileAsync_ExistingFile_ReturnsContentStream()
        {
            // Arrange
            var fileId = Guid.NewGuid();
            const string fileName = "test.zip";
            const string type = "application/zip";
            const int size = 2936320;
            const int chunkCount = 3;
            var storedDate = new DateTime(2017, 1, 1, 1, 1, 1);
            var httpWebClient = CreateHttpWebClientClient(CreateFileHttpWebResponse(fileName, type, size, chunkCount, storedDate));
            var fileRepository = new FileRepository(httpWebClient);

            // Act
            var file = await fileRepository.GetFileAsync(fileId);

            // Assert
            Assert.IsNotNull(file.ContentStream);
        }

        [TestMethod]
        public async Task GetFileAsync_FileWithNoName_ReturnsDefaultFileName()
        {
            // Arrange
            var fileId = Guid.NewGuid();
            const string fileName = ".zip";
            const string type = "application/zip";
            const int size = 2936320;
            const int chunkCount = 3;
            var storedDate = new DateTime(2017, 1, 1, 1, 1, 1);
            var httpWebClient = CreateHttpWebClientClient(CreateFileHttpWebResponse(fileName, type, size, chunkCount, storedDate));
            var fileRepository = new FileRepository(httpWebClient);

            // Act
            var file = await fileRepository.GetFileAsync(fileId);

            // Assert
            Assert.AreEqual("BlueprintFile.zip", file.Info.Name);
        }

        [TestMethod]
        public async Task GetFileAsync_FileWithNoNameOrExtension_ReturnsDefaultFileName()
        {
            // Arrange
            var fileId = Guid.NewGuid();
            var fileName = string.Empty;
            const string type = "application/zip";
            const int size = 2936320;
            const int chunkCount = 3;
            var storedDate = new DateTime(2017, 1, 1, 1, 1, 1);
            var httpWebClient = CreateHttpWebClientClient(CreateFileHttpWebResponse(fileName, type, size, chunkCount, storedDate));
            var fileRepository = new FileRepository(httpWebClient);

            // Act
            var file = await fileRepository.GetFileAsync(fileId);

            // Assert
            Assert.AreEqual("BlueprintFile", file.Info.Name);
        }

        [TestMethod]
        public async Task UploadFileAsync_InvalidArguments()
        {
            // Arrange
            var uri = new Uri("http://localhost");
            var httpWebClient = new HttpWebClient(uri, null);

            var fileRepository = new FileRepository(httpWebClient);
            ArgumentNullException argException = null;

            // Test "fileName" argument
            try
            {
                await fileRepository.UploadFileAsync(" ", "", null, null);
            }
            catch (ArgumentNullException ex)
            {
                argException = ex;
            }
            Assert.IsNotNull(argException);

            // Test "content" argument
            try
            {
                argException = null;

                await fileRepository.UploadFileAsync("TEST", "", null, null);
            }
            catch (ArgumentNullException ex)
            {
                argException = ex;
            }
            Assert.IsNotNull(argException);
        }

        [TestMethod]
        public async Task UploadFileAsync_AuthenticationException()
        {
            // Arrange
            AuthenticationException retException = null;
            string fileName = "Test.xml";
            string type = "xml";

            var response = CreateUploadFileHttpWebResponse(fileName, type, HttpStatusCode.Unauthorized);
            var httpWebClient = CreateUploadHttpClient(response);

            var fileRepository = new FileRepository(httpWebClient);

            // Act
            try
            {
                byte[] arr = Encoding.ASCII.GetBytes("Test...Test...");
                Stream content = new MemoryStream(arr);
                await fileRepository.UploadFileAsync("Test", "xml", content);
            }
            catch (AuthenticationException ex)
            {
                retException = ex;
            }
            Assert.IsNotNull(retException);
            
        }

        [TestMethod]
        public async Task UploadFileAsync_BadRequestException()
        {
            // Arrange
            BadRequestException retException = null;
            string fileName = "Test.xml";
            string type = "xml";

            var response = CreateUploadFileHttpWebResponse(fileName, type, HttpStatusCode.BadRequest);
            var httpWebClient = CreateUploadHttpClient(response);

            var fileRepository = new FileRepository(httpWebClient);

            // Act
            try
            {
                byte[] arr = Encoding.ASCII.GetBytes("Test...Test...");
                Stream content = new MemoryStream(arr);
                await fileRepository.UploadFileAsync("Test", "xml", content);
            }
            catch (BadRequestException ex)
            {
                retException = ex;
            }
            Assert.IsNotNull(retException);

        }

        [TestMethod]
        public async Task UploadFileAsync_ResourceNotFoundException()
        {
            // Arrange
            ResourceNotFoundException retException = null;
            string fileName = "Test.xml";
            string type = "xml";

            var response = CreateUploadFileHttpWebResponse(fileName, type, HttpStatusCode.NotFound);
            var httpWebClient = CreateUploadHttpClient(response);

            var fileRepository = new FileRepository(httpWebClient);

            // Act
            try
            {
                byte[] arr = Encoding.ASCII.GetBytes("Test...Test...");
                Stream content = new MemoryStream(arr);
                await fileRepository.UploadFileAsync("Test", "xml", content);
            }
            catch (ResourceNotFoundException ex)
            {
                retException = ex;
            }
            Assert.IsNotNull(retException);

        }

        [TestMethod]
        public async Task UploadFileAsync_DefaultException()
        {
            // Arrange
            Exception retException = null;
            string fileName = "Test.xml";
            string type = "xml";

            var response = CreateUploadFileHttpWebResponse(fileName, type, HttpStatusCode.OK);
            var httpWebClient = CreateUploadHttpClient(response);

            var fileRepository = new FileRepository(httpWebClient);

            // Act
            try
            {
                byte[] arr = Encoding.ASCII.GetBytes("Test...Test...");
                Stream content = new MemoryStream(arr);
                await fileRepository.UploadFileAsync("Test", "xml", content);
            }
            catch (Exception ex)
            {
                retException = ex;
            }
            Assert.IsNotNull(retException);

        }

        private static HttpWebResponse CreateHttpWebResponse(HttpStatusCode status)
        {
            var responseMock = new Mock<HttpWebResponse>();
            responseMock.Setup(m => m.StatusCode).Returns(status);

            return responseMock.Object;
        }

        private static HttpWebResponse CreateFileHttpWebResponse(string fileName, string type, long size, int chunkCount, DateTime storedDate)
        {
            var responseMock = new Mock<HttpWebResponse>();
            responseMock.Setup(m => m.StatusCode).Returns(HttpStatusCode.OK);
            responseMock.Setup(m => m.Headers).Returns
            (
                new WebHeaderCollection
                {
                    {
                        ServiceConstants.ContentDispositionHeader,
                        string.Format(CultureInfo.InvariantCulture, "attachment; filename=\"{0}\";fileNameStar=\"{0}\"", fileName)
                    },
                    {ServiceConstants.ContentTypeHeader, type},
                    {ServiceConstants.FileSizeHeader, size.ToString(CultureInfo.InvariantCulture)},
                    {ServiceConstants.FileChunkCountHeader, chunkCount.ToString(CultureInfo.InvariantCulture)},
                    {ServiceConstants.StoredDateHeader, storedDate.ToString("o", CultureInfo.InvariantCulture)}
                }
            );
            responseMock.Setup(m => m.GetResponseStream()).Returns(() =>
            {
                var streamMock = new Mock<Stream>();
                return streamMock.Object;
            });

            return responseMock.Object;
        }

        private static HttpWebResponse CreateUploadFileHttpWebResponse(string fileName, string type, HttpStatusCode code)
        {
            var responseMock = new Mock<HttpWebResponse>();
            responseMock.Setup(m => m.StatusCode).Returns(code);
            responseMock.Setup(m => m.Headers).Returns
            (
                new WebHeaderCollection
                {
                    {
                        ServiceConstants.ContentDispositionHeader,
                        string.Format(CultureInfo.InvariantCulture, "filename=\"{0}\"", fileName)
                    },
                    {ServiceConstants.ContentTypeHeader, type}
                }
            );
            responseMock.Setup(m => m.GetResponseStream()).Returns(() =>
            {
                var expected = "response content";
                var expectedBytes = Encoding.UTF8.GetBytes(expected);
                var responseStream = new MemoryStream();
                responseStream.Write(expectedBytes, 0, expectedBytes.Length);
                responseStream.Seek(0, SeekOrigin.Begin);
                return responseStream;
            });

            return responseMock.Object;
        }

        private static IHttpWebClient CreateHttpWebClientClient(HttpWebResponse response)
        {
            var clientMock = new Mock<IHttpWebClient>();

            clientMock
                .Setup(m => m.GetHttpWebResponseAsync(It.IsAny<HttpWebRequest>()))
                .Returns(Task.FromResult(response));
          
            return clientMock.Object;
        }

        private static IHttpWebClient CreateUploadHttpClient(HttpWebResponse response)
        {
            var clientMock = new Mock<IHttpWebClient>();

            clientMock
                .Setup(m => m.GetHttpWebResponseAsync(It.IsAny<HttpWebRequest>()))
                .Returns(Task.FromResult(response));

            clientMock
                .Setup(r => r.CreateHttpWebRequest(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() =>
                {
                    var request = WebRequest.CreateHttp("http://localhost");
                    request.Method = "POST";
 
                    return request;
                });

            return clientMock.Object;
        }

    }
}
