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

        private static IHttpWebClient CreateHttpWebClientClient(HttpWebResponse response)
        {
            var clientMock = new Mock<IHttpWebClient>();

            clientMock
                .Setup(m => m.GetHttpWebResponseAsync(It.IsAny<HttpWebRequest>()))
                .Returns(Task.FromResult(response));

            return clientMock.Object;
        }
    }
}
