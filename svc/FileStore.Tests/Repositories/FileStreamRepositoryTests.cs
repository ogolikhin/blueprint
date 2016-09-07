using System;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using File = FileStore.Models.File;

namespace FileStore.Repositories
{
    [TestClass]
    public class FileStreamRepositoryTests
    {
        [TestMethod]
        public async Task GetFileStream_Content_Success()
        {
            // This tests the legacy file stream retrieval logic 

            // Arrange
            var moqFSRepo = new Mock<IFileStreamRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();

            int legacyFileChunkSize = 125; 
            var contentString = GetRandomString(125);
            // set the size of the content to force two loops to retrieve total of 250 bytes 
            byte[] fileStreamContent = Encoding.UTF8.GetBytes(contentString + contentString);
            
            var file = new File
            {
                FileId = new Guid("22222222-2222-2222-2222-222222222222"),
                FileName = "Test2.txt",
                StoredTime = I18NHelper.DateTimeParseExactInvariant("2015-09-05T22:57:31.7824054-04:00", "o"),
                FileType = "application/octet-stream",
                FileSize = fileStreamContent.Length,
                ChunkCount = 1
            };

            var moqDbConnection = new Mock<DbConnection>();

            moqFSRepo.Setup(t => t.CreateConnection()).Returns(moqDbConnection.Object);
            moqFSRepo.Setup(t => t.GetFileHead(It.IsAny<Guid>())).Returns(file);
            moqFSRepo.Setup(t => t.ReadChunkContent(moqDbConnection.Object, It.IsAny<Guid>(), It.IsAny<long>(), It.IsAny<long>())).Returns(fileStreamContent.Take<byte>(125).ToArray<byte>());
            moqConfigRepo.Setup(t => t.LegacyFileChunkSize).Returns(legacyFileChunkSize);

            // Act
            FileStreamPushStream fsPushStream = new FileStreamPushStream();

            fsPushStream.Initialize(moqFSRepo.Object, moqConfigRepo.Object, file.FileId);

            HttpContent responseContent = new PushStreamContent((Func<Stream, HttpContent, TransportContext, Task>) fsPushStream.WriteToStream, new MediaTypeHeaderValue(file.ContentType));

            Stream response = await responseContent.ReadAsStreamAsync();

            string originalContent = Encoding.UTF8.GetString(fileStreamContent);
            string resultContent = string.Empty;

            using (var memoryStream = new MemoryStream())
            {
                response.CopyTo(memoryStream);
                resultContent = Encoding.UTF8.GetString(memoryStream.ToArray());
            }
 
            // Assert
            Assert.IsTrue(originalContent.Equals(resultContent));
        }

        private string GetRandomString(int length)
        {
            string result = string.Empty;

            if (length < 1) length = 1;
            // each string is 11 chars 
            // combine to make a string of size length 
            int loop = ((int)(length / 11)) + 1;
            for (int i = 0; i < loop; i++)
            { 
                string path = Path.GetRandomFileName();
                path = path.Replace(".", ""); // Remove period.
                result += path;
            }

            return result.Substring(0, length);
        }
    }
}
