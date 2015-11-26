using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileStore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        [Ignore]
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

        #region DeleteFile

        [TestMethod]
        public async Task DeleteFile_QueryReturnsId_ReturnsId()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlFilesRepository(cxn.Object);
            var guid = new Guid("12345123451234512345123451234512");
            Guid? result = guid;
            cxn.SetupExecuteAsync(
                "DeleteFile",
                new Dictionary<string, object> { { "FileId", guid } },
                1,
                new Dictionary<string, object> { { "ExpiredTime", DateTime.UtcNow } });

            // Act
            Guid? id = await repository.DeleteFile(guid);

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
            cxn.SetupExecuteAsync(
                "DeleteFile",
                new Dictionary<string, object> { { "FileId", guid } },
                1,
					 new Dictionary<string, object> { { "ExpiredTime", DateTime.UtcNow } });

			// Act
			Guid? id = await repository.DeleteFile(guid);

            // Assert
            cxn.Verify();
            Assert.IsNotNull(id);
        }

        #endregion DeleteFile
    }
}
