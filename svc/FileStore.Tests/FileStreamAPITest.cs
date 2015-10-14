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
using FileStore.Models;

namespace FileStore.Tests
{
    [TestClass]
    public class FileStreamAPITest
    {
        [TestCategory("FileStreamSvc-UnitTests")]
        [TestMethod]
        public void GetFileFromFileStream_ImproperGuid_FormatException()
        {
            // Arrange
            var moq = new Mock<IFileStreamRepository>();

            FileStreamAPI fsapi = new FileStreamAPI(moq.Object);
            try
            {
                // Act 
                File file = fsapi.GetFile(File.ConvertFileId("333333333!@#@!@!@!33333333333333333333333"), null).Result;
            }
            catch (FormatException)
            {
                // assert
                return;
            }
            Assert.Fail("No exception was thrown.");

        }
         
        [TestCategory("FileStreamSvc-UnitTests")]
        [TestMethod]
        public void GetFileFromFileStream_NonExistentFile_NotFoundFailure()
        {
            // Arrange
            var moq = new Mock<IFileStreamRepository>();

            moq.Setup(t => t.GetFileStreamAsync(It.IsAny<Guid>())).Returns(Task.FromResult<byte[]>(null));

            FileStreamAPI fsapi = new FileStreamAPI(moq.Object);

            // Act
            File result = fsapi.GetFile(File.ConvertFileId("4a18f649c6654bb5b101abad5e8289b5"), null).Result;

            // Assert
            Assert.IsTrue(result == null);
        }

        [TestCategory("FileStreamSvc-UnitTests")]
        [TestMethod]
        public void GetFileFromFileStream_ProperRequest_Success()
        {
            // Arrange
    
            var moq = new Mock<IFileStreamRepository>();

            moq.Setup(t => t.GetFileStreamAsync(It.IsAny<Guid>())).Returns(Task.FromResult<byte[]>(Encoding.UTF8.GetBytes("Test2 content")));

            FileStreamAPI fsapi = new FileStreamAPI(moq.Object);

            // Act
            File result = fsapi.GetFile(File.ConvertFileId("4a18f649c6654bb5b101abad5e8289b5"), null).Result;
           

            // Assert
            Assert.IsTrue(result != null);

            Assert.IsTrue(Encoding.UTF8.GetString(result.FileContent) == "Test2 content");
            Assert.IsTrue(result.FileType == "application/octet-stream");
        }
    
    }
}
