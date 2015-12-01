using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FileStore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Repositories;

namespace FileStore.Repositories
{
    [TestClass]
    public class SqlFilesRepositoryTests
    {
        #region Constuctor

        [TestMethod]
        public void Constructor_CreatesConnectionToFileStorage()
        {
            // Arrange

            // Act
            var repository = new SqlFilesRepository();

            // Assert
            Assert.AreEqual(ConfigRepository.Instance.FileStoreDatabase, repository.ConnectionWrapper.CreateConnection().ConnectionString);
        }

        #endregion Constructor

        #region PostFile
        [TestMethod]
        public async Task PostFile_QueryReturnsId_ReturnsId()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlFilesRepository(cxn.Object);
            File file = new File {FileName = "name", FileType = "type"};
            Guid? result = new Guid("12345678901234567890123456789012");
            cxn.SetupExecuteAsync(
                "InsertFileHead",
                new Dictionary<string, object> { { "FileName", file.FileName }, { "FileType", file.FileType }, { "FileId", null } },
                1,
                new Dictionary<string, object> { { "FileId", result } });

            // Act
            Guid? id = await repository.PostFileHead(file);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result, id);
        }

        #endregion PostFile

        #region HeadFile

        [TestMethod]
        public async Task GetHeadFile_QueryReturnsFile_ReturnsFirst()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlFilesRepository(cxn.Object);
            var guid = new Guid("99999999999999999999999999999999");
            File[] result = { new File { FileName = "name", FileType = "type" } };
            cxn.SetupQueryAsync(
                "ReadFileHead",
                new Dictionary<string, object> { { "FileId", guid } },
                result);

            // Act
            File file = await repository.GetFileHead(guid);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result.First(), file);
        }
        [TestMethod]
        public async Task HeadFile_QueryReturnsEmpty_ReturnsNull()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlFilesRepository(cxn.Object);
            var guid = new Guid("88888888888888888888888888888888");
            File[] result = { };
            cxn.SetupQueryAsync(
                "ReadFileHead",
                new Dictionary<string, object> { { "FileId", guid } },
                result);

            // Act
            File file = await repository.GetFileHead(guid);

            // Assert
            cxn.Verify();
            Assert.IsNull(file);
        }
        #endregion HeadFile

        #region GetFile

        [TestMethod]
        public async Task GetFile_QueryReturnsFile_ReturnsFirst()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlFilesRepository(cxn.Object);
            var guid = new Guid("33333333333333333333333333333333");
            File[] result = { new File { FileName = "nnnn", FileType = "tttt" } };
            cxn.SetupQueryAsync(
                "ReadFileHead",
                new Dictionary<string, object> { { "FileId", guid } },
                result);

            // Act
            File file = await repository.GetFileHead(guid);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result.First(), file);
        }

        [TestMethod]
        public async Task GetFile_QueryReturnsEmpty_ReturnsNull()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlFilesRepository(cxn.Object);
            var guid = new Guid("22222222222222222222222222222222");
            File[] result = { };
            cxn.SetupQueryAsync(
                "ReadFileHead",
                new Dictionary<string, object> { { "FileId", guid } },
                result);

            // Act
            File file = await repository.GetFileHead(guid);

            // Assert
            cxn.Verify();
            Assert.IsNull(file);
        }

        #endregion GetFile

        #region ReadChunkContent 

        [TestMethod]
        public async Task Read_File_Chunks_Success()
        {
            // This tests reading file chunks and pushing them to the output stream 

            // Arrange
            var moqFilesRepo = new Mock<IFilesRepository>();
            var moqConfigRepo = new Mock<IConfigRepository>();
            var moqFileMapper = new Mock<IFileMapperRepository>();

            int fileChunkSize = 125;
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
                ChunkCount = 2
            };

            var moqDbConnection = new Mock<DbConnection>();

            moqFilesRepo.Setup(t => t.CreateConnection()).Returns(moqDbConnection.Object);

            moqFilesRepo.Setup(t => t.GetFileHead(It.IsAny<Guid>())).ReturnsAsync(file);

            moqFilesRepo.Setup(t => t.ReadChunkContent(moqDbConnection.Object, It.IsAny<Guid>(), It.IsAny<int>())).Returns(fileStreamContent.Take<byte>(125).ToArray<byte>());

            moqFileMapper.Setup(t => t.GetMappedOutputContentType(It.IsAny<string>()))
                 .Returns(FileMapperRepository.DefaultMediaType);

            moqConfigRepo.Setup(t => t.LegacyFileChunkSize).Returns(fileChunkSize);

            moqFilesRepo.Setup(t => t.GetFileInfo(It.IsAny<Guid>())).Returns(file);


            // Act

            string mappedContentType = moqFileMapper.Object.GetMappedOutputContentType(file.FileType);

            SqlPushStream sqlPushStream = new SqlPushStream();

            sqlPushStream.Initialize(moqFilesRepo.Object, file.FileId);

            HttpContent responseContent = new PushStreamContent(sqlPushStream.WriteToStream, new MediaTypeHeaderValue(mappedContentType));

            System.IO.Stream response = await responseContent.ReadAsStreamAsync();

            string originalContent = Encoding.UTF8.GetString(fileStreamContent);
            string resultContent = string.Empty;

            using (var memoryStream = new System.IO.MemoryStream())
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
                string path = System.IO.Path.GetRandomFileName();
                path = path.Replace(".", ""); // Remove period.
                result += path;
            }


            return result.Substring(0, length);
        }


        #endregion

        #region DeleteFile

        [TestMethod]
        public async Task DeleteFile_QueryReturnsId_ReturnsId()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlFilesRepository(cxn.Object);
            var guid = new Guid("12345123451234512345123451234512");
            Guid? result = guid;
            cxn.SetupExecuteScalarAsync(
                "DeleteFile",
                new Dictionary<string, object> { { "FileId", guid } },
                1,
                new Dictionary<string, object> { { "ExpiredTime", DateTime.UtcNow } });

            // Act
            Guid? id = await repository.DeleteFile(guid, DateTime.UtcNow);

            // Assert
            cxn.Verify();
            Assert.AreEqual(result, id);
        }
        
        [TestMethod]
        public async Task DeleteFile_QueryReturnsNull_ReturnsNotNull()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlFilesRepository(cxn.Object);
            var guid = new Guid("34567345673456734567345673456734");
            cxn.SetupExecuteScalarAsync(
                "DeleteFile",
                new Dictionary<string, object> { { "FileId", guid } },
                1,
                     new Dictionary<string, object> { { "ExpiredTime", DateTime.UtcNow } });

            // Act
            Guid? id = await repository.DeleteFile(guid, DateTime.UtcNow);

            // Assert
            cxn.Verify();
            Assert.IsNotNull(id);
        }

        #endregion DeleteFile
    }
}
