using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Data.SqlClient;
using System.Net.Http.Headers;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using System.Net;
using System.Web;
using System.Data.Common;
using System.Data;
using FileStore.Models;
using FileStore.Repositories;
using File = FileStore.Models.File;

namespace FileStore.Repositories
{
    [TestClass]
    public class FileStreamRepositoryTests
    {
        [TestMethod]
        public void GetFileStream_Content_Success()
        {
            // This tests the legacy file stream retrieval logic 

            // Arrange
            var moqFSRepo = new Mock<IFileStreamRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqFileMapper = new Mock<IFileMapperRepository>();

            int legacyFileChunkSize = 125; 
            var contentString = GetRandomString(125);
            // set the size of the content to force two loops to retrieve total of 250 bytes 
            byte[] fileStreamContent = Encoding.UTF8.GetBytes(contentString + contentString);
            
            var file = new File
            {
                FileId = new Guid("22222222-2222-2222-2222-222222222222"),
                FileName = "Test2.txt",
                StoredTime = DateTime.ParseExact("2015-09-05T22:57:31.7824054-04:00", "o", CultureInfo.InvariantCulture),
                FileType = FileMapperRepository.DefaultMediaType,
                FileSize = fileStreamContent.Length,
                ChunkCount = 1
            };

            var moqDbConnection = new Mock<DbConnection>();

            moqFSRepo.Setup(t => t.CreateConnection()).Returns(moqDbConnection.Object);

            moqFSRepo.Setup(t => t.GetFileHead(It.IsAny<Guid>())).Returns(file);

            moqFSRepo.Setup(t => t.ReadChunkContent(moqDbConnection.Object, It.IsAny<Guid>(), It.IsAny<long>(), It.IsAny<long>())).Returns(fileStreamContent.Take<byte>(125).ToArray<byte>());

            moqFileMapper.Setup(t => t.GetMappedOutputContentType(It.IsAny<string>()))
                 .Returns(FileMapperRepository.DefaultMediaType);

            moqConfigRepo.Setup(t => t.LegacyFileChunkSize).Returns(legacyFileChunkSize);

            // Act

            string mappedContentType = moqFileMapper.Object.GetMappedOutputContentType(file.FileType);

            FileStreamPushStream fsPushStream = new FileStreamPushStream();

            fsPushStream.Initialize(moqFSRepo.Object, moqConfigRepo.Object, file.FileId);

            HttpContent responseContent = new PushStreamContent(fsPushStream.WriteToStream, new MediaTypeHeaderValue(mappedContentType));

            Task<Stream> response = responseContent.ReadAsStreamAsync();

            string originalContent = Encoding.UTF8.GetString(fileStreamContent);
            string resultContent = string.Empty;

            using (var memoryStream = new MemoryStream())
            {
                response.Result.CopyTo(memoryStream);
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
