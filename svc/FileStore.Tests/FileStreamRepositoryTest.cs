using System;
using FileStore.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace FileStore.Tests
{
    [TestClass]
    public class FileStreamRepositoryTest
    {
        [TestCategory("FileStoreSvc-UnitTests")]
        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void GetFile_EmptyGuid()
        {
            //Arrange
            var mockConfigRepo = new Mock<IConfigRepository>();
            var mockContentReadStream = new Mock<IContentReadStream>();
            var fileStreamRepository = new FileStreamRepository(mockConfigRepo.Object, mockContentReadStream.Object);

            //Act
            fileStreamRepository.GetFile(Guid.Empty);
        }

        [TestCategory("FileStoreSvc-UnitTests")]
        [TestMethod]
        public void GetFile_BadFileGuid()
        {
            //Arrange
            var mockConfigRepo = new Mock<IConfigRepository>();
            var mockContentReadStream = new Mock<IContentReadStream>();
            var fileStreamRepository = new FileStreamRepository(mockConfigRepo.Object, mockContentReadStream.Object);

            //Act
            var file = fileStreamRepository.GetFile(Guid.NewGuid());

            //Assert
            Assert.IsNull(file, "Invalid Guid returned a valid file");
        }

        [TestCategory("FileStoreSvc-UnitTests")]
        [TestMethod]
        public void GetFile_GoodFileGuid()
        {
            //Arrange
            var mockConfigRepo = new Mock<IConfigRepository>();
            var mockContentReadStream = new Mock<IContentReadStream>();
            mockContentReadStream.Setup(s => s.Length).Returns(100);
            mockContentReadStream.Setup(s => s.FileName).Returns("ABC.txt");
            mockContentReadStream.Setup(s => s.FileType).Returns("image/bmp");
            var fileStreamRepository = new FileStreamRepository(mockConfigRepo.Object, mockContentReadStream.Object);
            var guid = Guid.NewGuid();
            object[] expectedObjects = {guid, 100, "ABC.txt", "image/bmp" };

            //Act
            var file = fileStreamRepository.GetFile(guid);
            object[] actualObjects = {file.FileId, (int)file.FileSize, file.FileName, file.FileType};

            //Assert
            CollectionAssert.AreEquivalent(expectedObjects, actualObjects);
        }

        [TestCategory("FileStoreSvc-UnitTests")]
        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void HeadFile_EmptyGuid()
        {
            //Arrange
            var mockConfigRepo = new Mock<IConfigRepository>();
            var mockContentReadStream = new Mock<IContentReadStream>();
            var fileStreamRepository = new FileStreamRepository(mockConfigRepo.Object, mockContentReadStream.Object);

            //Act
            fileStreamRepository.HeadFile(Guid.Empty);
        }

        [TestCategory("FileStoreSvc-UnitTests")]
        [TestMethod]
        public void HeadFile_GoodFileGuid()
        {
            //Arrange
            var mockConfigRepo = new Mock<IConfigRepository>();
            var mockContentReadStream = new Mock<IContentReadStream>();
            mockContentReadStream.Setup(s => s.Length).Returns(100);
            mockContentReadStream.Setup(s => s.FileName).Returns("ABC.txt");
            mockContentReadStream.Setup(s => s.FileType).Returns("image/bmp");
            var fileStreamRepository = new FileStreamRepository(mockConfigRepo.Object, mockContentReadStream.Object);
            var guid = Guid.NewGuid();
            object[] expectedObjects = { guid, 100, "ABC.txt", "image/bmp" };

            //Act
            var file = fileStreamRepository.HeadFile(guid);
            object[] actualObjects = { file.FileId, (int)file.FileSize, file.FileName, file.FileType };

            //Assert
            CollectionAssert.AreEquivalent(expectedObjects, actualObjects);
        }
    }
}
