using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using CustomAttributes;
using Helper.Factories;
using Model;
using Model.Factories;
using NUnit.Framework;

namespace FileStoreTests
{
    [TestFixture]
    [Category(Categories.Filestore)]
    public class BasicTests
    {
        private readonly IBlueprintServer _server = BlueprintServerFactory.GetBlueprintServerFromTestConfig();
        private IFileStore _filestore;
        private IUser _user;

        [SetUp]
        public void SetUp()
        {
            _user = UserFactory.CreateUserAndAddToDatabase();
            _filestore = FileStoreFactory.CreateFileStore(_server.Address);
        }

        [TearDown]
        public void TearDown()
        {
            if (_user != null)
            {
                _user.DeleteUser(deleteFromDatabase: true);
                _user = null;
            }
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)2048, "2KB_File.txt", "text/plain", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)4096, "4KB_File.txt", "text/plain", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)8192, "8KB_File.txt", "text/plain", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        public void PostFileWithMultiMimeParts_OK(uint fileSize, string fakeFileName, string fileType)
        {
            PostFile(fileSize, fakeFileName, fileType, true);
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)2048, "2KB_File.txt", "text/plain", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)4096, "4KB_File.txt", "text/plain", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)8192, "8KB_File.txt", "text/plain", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        public void PostFileWithoutMultiMimeParts_OK(uint fileSize, string fakeFileName, string fileType)
        {
            PostFile(fileSize, fakeFileName, fileType);
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)2048, "2KB_File.txt", "text/plain", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)4096, "4KB_File.txt", "text/plain", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)8192, "8KB_File.txt", "text/plain", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        public void PostFileWithExpireTime_OK(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: create a fake file with a random byte array.
            string randomChunk = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(fileSize);
            byte[] fileContents = Encoding.ASCII.GetBytes(randomChunk);
            IFile file = FileFactory.CreateFile(fakeFileName, fileType, DateTime.Now, fileContents);

            // Add the file to Filestore.
            var expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            var storedFile = _filestore.AddFile(file, _user, Convert.ToDateTime(DateTime.Now.AddDays(1)), true, expectedStatusCodes:expectedStatusCodes);

            // Verify that the file was stored properly by getting it back and comparing it with original.
            expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
            var returnedFile = _filestore.GetFile(storedFile.Id, _user, expectedStatusCodes);

            Assert.AreEqual(file.Content, returnedFile.Content,
                "The file bytes returned from FileStore do not match the bytes we added!");
            Assert.AreEqual(fakeFileName, returnedFile.FileName,
                "The file returned filename does not match the filename of the sent file!");
            Assert.AreEqual(fileType, returnedFile.FileType,
                "The file type returned does not math the type sent!");

            expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
            _filestore.DeleteFile(storedFile.Id, _user, expectedStatusCodes:expectedStatusCodes);

            expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.NotFound };
            var deletedFile = _filestore.GetFile(storedFile.Id, _user, expectedStatusCodes);
            Assert.IsNull(deletedFile, "File was not deleted!");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)2048, "2KB_File.txt", "text/plain", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)4096, "4KB_File.txt", "text/plain", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)8192, "8KB_File.txt", "text/plain", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        public void PostFileWithDeleteExpireTime_OK(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: create a fake file with a random byte array.
            string randomChunk = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(fileSize);
            byte[] fileContents = Encoding.ASCII.GetBytes(randomChunk);
            IFile file = FileFactory.CreateFile(fakeFileName, fileType, DateTime.Now, fileContents);

            // Add the file to Filestore.
            var expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            var storedFile = _filestore.AddFile(file, _user, useMultiPartMime:true, expectedStatusCodes:expectedStatusCodes);

            // Verify that the file was stored properly by getting it back and comparing it with original.
            expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
            _filestore.GetFile(storedFile.Id, _user, expectedStatusCodes);

            expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
            _filestore.DeleteFile(storedFile.Id, _user, Convert.ToDateTime(DateTime.Now.AddDays(1)), expectedStatusCodes);

            expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
            var returnedFile = _filestore.GetFile(storedFile.Id, _user, expectedStatusCodes);

            // Assert that the file still exists after deleting with a future expire time
            Assert.AreEqual(file.Content, returnedFile.Content,
                "The file bytes returned from FileStore do not match the bytes we added!");
            Assert.AreEqual(fakeFileName, returnedFile.FileName,
                "The file returned filename does not match the filename of the sent file!");
            Assert.AreEqual(fileType, returnedFile.FileType,
                "The file type returned does not math the type sent!");

            // Delete again with no future expire time
            expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
            _filestore.DeleteFile(storedFile.Id, _user, expectedStatusCodes:expectedStatusCodes);

            expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.NotFound };
            var deletedFile = _filestore.GetFile(storedFile.Id, _user, expectedStatusCodes);
            Assert.IsNull(deletedFile, "File was not deleted!");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)2048, "2KB_File.txt", "text/plain", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)4096, "4KB_File.txt", "text/plain", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)8192, "8KB_File.txt", "text/plain", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        public void GetHeadOnly_OK(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: create a fake file with a random byte array.
            string randomChunk = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(fileSize);
            byte[] fileContents = Encoding.ASCII.GetBytes(randomChunk);
            IFile file = FileFactory.CreateFile(fakeFileName, fileType, DateTime.Now, fileContents);

            // Add the file to Filestore.
            var expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            var storedFile = _filestore.AddFile(file, _user, useMultiPartMime:true, expectedStatusCodes: expectedStatusCodes);

            // Verify that the file was stored properly by getting it back and comparing it with original.
            expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
            var returnedFile = _filestore.GetFileMetadata(storedFile.Id, _user,expectedStatusCodes);

            Assert.That(returnedFile.Content.Length == 0, "File content was returned but not expected!");
            Assert.AreEqual(fakeFileName, returnedFile.FileName,
                "The file returned filename does not match the filename of the sent file!");
            Assert.AreEqual("text/plain", returnedFile.FileType,
                "The file type returned does not match the type sent!");
        }

        [TestCase(Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        public void Status_OK()
        { 
            // Add the file to Filestore.
            var response = _filestore.GetStatus();

            Assert.That(response == HttpStatusCode.OK, "File store service status is not OK!");
        }

        private void PostFile(uint fileSize, string fakeFileName, string fileType, bool useMultiPartMime = false)
        {
            // Setup: create a fake file with a random byte array.
            string randomChunk = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(fileSize);
            byte[] fileContents = Encoding.ASCII.GetBytes(randomChunk);
            IFile file = FileFactory.CreateFile(fakeFileName, fileType, DateTime.Now, fileContents);

            // Add the file to Filestore.
            var expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            var storedFile = _filestore.AddFile(file, _user, useMultiPartMime: useMultiPartMime, expectedStatusCodes: expectedStatusCodes);

            // Verify that the file was stored properly by getting it back and comparing it with original.
            expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
            var returnedFile = _filestore.GetFile(storedFile.Id, _user, expectedStatusCodes);

            Assert.AreEqual(file.Content, returnedFile.Content,
                "The file bytes returned from FileStore do not match the bytes we added!");
            Assert.AreEqual(fakeFileName, returnedFile.FileName,
                "The file returned filename does not match the filename of the sent file!");
            Assert.AreEqual(fileType, returnedFile.FileType,
                "The file type returned does not math the type sent!");

            expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
            _filestore.DeleteFile(storedFile.Id, _user, expectedStatusCodes: expectedStatusCodes);

            expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.NotFound };
            var deletedFile = _filestore.GetFile(storedFile.Id, _user, expectedStatusCodes);
            Assert.IsNull(deletedFile, "The file was not deleted!");
        }
    }
}
