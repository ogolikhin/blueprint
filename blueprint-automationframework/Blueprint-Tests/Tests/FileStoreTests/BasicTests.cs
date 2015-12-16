using System;
using System.Collections.Generic;
using System.Net;
using CustomAttributes;
using Helper.Factories;
using Model;
using NUnit.Framework;

namespace FileStoreTests
{
    [TestFixture]
    [Category(Categories.Filestore)]
    public class BasicTests
    {
        private IFileStore _filestore;
        private IUser _user;

        [SetUp]
        public void SetUp()
        {
            _user = UserFactory.CreateUserAndAddToDatabase();
            _filestore = FileStoreFactory.GetFileStoreFromTestConfig();
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

        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        [TestCase((uint)2048, "2KB_File.txt", "text/plain")]
        [TestCase((uint)4096, "4KB_File.txt", "text/plain")]
        [TestCase((uint)8192, "8KB_File.txt", "text/plain")]
        public void PostFileWithMultiMimeParts_OK(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelpers.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Add the file to Filestore.
            var expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            var storedFile = _filestore.AddFile(file, _user, useMultiPartMime: true, expectedStatusCodes: expectedStatusCodes);

            // Verify that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = _filestore.GetFile(storedFile.Id, _user);

            Assert.AreEqual(file.Content, returnedFile.Content,
                "The file bytes returned from FileStore do not match the bytes we added!");
            Assert.AreEqual(fakeFileName, returnedFile.FileName,
                "The file returned filename does not match the filename of the sent file!");
            Assert.AreEqual(fileType, returnedFile.FileType,
                "The file type returned does not math the type sent!");

            _filestore.DeleteFile(storedFile.Id, _user);

            expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.NotFound };
            var deletedFile = _filestore.GetFile(storedFile.Id, _user, expectedStatusCodes);
            Assert.IsNull(deletedFile, "The file was not deleted!");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        [TestCase((uint)2048, "2KB_File.txt", "text/plain")]
        [TestCase((uint)4096, "4KB_File.txt", "text/plain")]
        [TestCase((uint)8192, "8KB_File.txt", "text/plain")]
        public void PostFileWithoutMultiMimeParts_OK(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelpers.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Add the file to Filestore.
            var expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            var storedFile = _filestore.AddFile(file, _user, expectedStatusCodes: expectedStatusCodes);

            // Verify that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = _filestore.GetFile(storedFile.Id, _user);

            Assert.AreEqual(file.Content, returnedFile.Content,
                "The file bytes returned from FileStore do not match the bytes we added!");
            Assert.AreEqual(fakeFileName, returnedFile.FileName,
                "The file returned filename does not match the filename of the sent file!");
            Assert.AreEqual(fileType, returnedFile.FileType,
                "The file type returned does not math the type sent!");

            _filestore.DeleteFile(storedFile.Id, _user);

            expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.NotFound };
            var deletedFile = _filestore.GetFile(storedFile.Id, _user, expectedStatusCodes);
            Assert.IsNull(deletedFile, "The file was not deleted!");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", (uint)512)]
        [TestCase((uint)2048, "2KB_File.txt", "text/plain", (uint)1024)]
        [TestCase((uint)4096, "4KB_File.txt", "text/plain", (uint)1024)]
        [TestCase((uint)8192, "8KB_File.txt", "text/plain", (uint)1024)]
        public void PostFileWithMultiMimePartsUsingPut_OK(uint fileSize, string fakeFileName, string fileType, uint chunkSize)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelpers.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Add the file to Filestore.
            var expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            var storedFile = _filestore.AddFile(file, _user, useMultiPartMime: true, chunkSize: chunkSize, expectedStatusCodes: expectedStatusCodes);

            // Verify that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = _filestore.GetFile(storedFile.Id, _user);

            Assert.AreEqual(file.Content, returnedFile.Content,
                "The file bytes returned from FileStore do not match the bytes we added!");
            Assert.AreEqual(fakeFileName, returnedFile.FileName,
                "The file returned filename does not match the filename of the sent file!");
            Assert.AreEqual(fileType, returnedFile.FileType,
                "The file type returned does not math the type sent!");

            _filestore.DeleteFile(storedFile.Id, _user);

            expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.NotFound };
            var deletedFile = _filestore.GetFile(storedFile.Id, _user, expectedStatusCodes);
            Assert.IsNull(deletedFile, "The file was not deleted!");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", (uint)512)]
        [TestCase((uint)2048, "2KB_File.txt", "text/plain", (uint)1024)]
        [TestCase((uint)4096, "4KB_File.txt", "text/plain", (uint)1024)]
        [TestCase((uint)8192, "8KB_File.txt", "text/plain", (uint)1024)]
        public void PostFileWithoutMultiMimePartsUsingPut_OK(uint fileSize, string fakeFileName, string fileType, uint chunkSize)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelpers.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Add the file to Filestore.
            var expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            var storedFile = _filestore.AddFile(file, _user, chunkSize: chunkSize, expectedStatusCodes: expectedStatusCodes);

            // Verify that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = _filestore.GetFile(storedFile.Id, _user);

            Assert.AreEqual(file.Content, returnedFile.Content,
                "The file bytes returned from FileStore do not match the bytes we added!");
            Assert.AreEqual(fakeFileName, returnedFile.FileName,
                "The file returned filename does not match the filename of the sent file!");
            Assert.AreEqual(fileType, returnedFile.FileType,
                "The file type returned does not math the type sent!");

            _filestore.DeleteFile(storedFile.Id, _user);

            expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.NotFound };
            var deletedFile = _filestore.GetFile(storedFile.Id, _user, expectedStatusCodes);
            Assert.IsNull(deletedFile, "The file was not deleted!");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        [TestCase((uint)2048, "2KB_File.txt", "text/plain")]
        [TestCase((uint)4096, "4KB_File.txt", "text/plain")]
        [TestCase((uint)8192, "8KB_File.txt", "text/plain")]
        public void PostFileWithExpireTime_OK(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelpers.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Add the file to Filestore.
            var expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            var storedFile = _filestore.AddFile(file, _user, DateTime.Now.AddDays(1), useMultiPartMime:true, expectedStatusCodes: expectedStatusCodes);

            // Verify that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = _filestore.GetFile(storedFile.Id, _user);

            Assert.AreEqual(file.Content, returnedFile.Content,
                "The file bytes returned from FileStore do not match the bytes we added!");
            Assert.AreEqual(fakeFileName, returnedFile.FileName,
                "The file returned filename does not match the filename of the sent file!");
            Assert.AreEqual(fileType, returnedFile.FileType,
                "The file type returned does not math the type sent!");

            _filestore.DeleteFile(storedFile.Id, _user);

            expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.NotFound };
            var deletedFile = _filestore.GetFile(storedFile.Id, _user, expectedStatusCodes);
            Assert.IsNull(deletedFile, "File was not deleted!");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        public void PostFileWithExpireTimeInPast_VerifyFileNotFound(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelpers.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Add the file to Filestore.
            var expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            var storedFile = _filestore.AddFile(file, _user, DateTime.Now.AddDays(-1), useMultiPartMime: true, expectedStatusCodes: expectedStatusCodes);

            // Verify that the file was stored properly by getting it back and comparing it with original.
            expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.NotFound};
            var returnedFile = _filestore.GetFile(storedFile.Id, _user, expectedStatusCodes);

            Assert.IsNull(returnedFile, "File was not deleted!");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        [TestCase((uint)2048, "2KB_File.txt", "text/plain")]
        [TestCase((uint)4096, "4KB_File.txt", "text/plain")]
        [TestCase((uint)8192, "8KB_File.txt", "text/plain")]
        public void DeleteFileWithFutureExpiryDate_VerifyFileStillExists(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelpers.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Add the file to Filestore.
            var expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            var storedFile = _filestore.AddFile(file, _user, useMultiPartMime: true, expectedStatusCodes: expectedStatusCodes);

            // Verify that the file was stored properly by getting it back and comparing it with original.
            _filestore.GetFile(storedFile.Id, _user);

            _filestore.DeleteFile(storedFile.Id, _user, DateTime.Now.AddDays(1));

            var returnedFile = _filestore.GetFile(storedFile.Id, _user);

            // Assert that the file still exists after deleting with a future expire time
            Assert.AreEqual(file.Content, returnedFile.Content,
                "The file bytes returned from FileStore do not match the bytes we added!");
            Assert.AreEqual(fakeFileName, returnedFile.FileName,
                "The file returned filename does not match the filename of the sent file!");
            Assert.AreEqual(fileType, returnedFile.FileType,
                "The file type returned does not math the type sent!");

            // Delete again with no future expire time
            _filestore.DeleteFile(storedFile.Id, _user);

            expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.NotFound };
            var deletedFile = _filestore.GetFile(storedFile.Id, _user, expectedStatusCodes);
            Assert.IsNull(deletedFile, "File was not deleted!");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        public void DeleteFileImmediately_VerifyFileIsDeleted(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelpers.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Add the file to Filestore.
            var expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            var storedFile = _filestore.AddFile(file, _user, useMultiPartMime: true, expectedStatusCodes: expectedStatusCodes);

            // Verify that the file was stored properly by getting it back.
            _filestore.GetFile(storedFile.Id, _user);

            // Delete immediately
            _filestore.DeleteFile(storedFile.Id, _user);

            expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.NotFound };
            var deletedFile = _filestore.GetFile(storedFile.Id, _user, expectedStatusCodes);
            Assert.IsNull(deletedFile, "File was not deleted!");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        public void DeleteFileWithPassedExpiryDate_VerifyFileDoesNotExist(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelpers.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Add the file to Filestore.
            var expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            var storedFile = _filestore.AddFile(file, _user, useMultiPartMime: true, expectedStatusCodes: expectedStatusCodes);

            // Verify that the file was stored properly by getting it back and comparing it with original.
            _filestore.GetFile(storedFile.Id, _user);

            _filestore.DeleteFile(storedFile.Id, _user, DateTime.Now.AddDays(-1));

            expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.NotFound };
            var returnedFile = _filestore.GetFile(storedFile.Id, _user, expectedStatusCodes);

            Assert.IsNull(returnedFile, "File was not deleted!");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        [TestCase((uint)2048, "2KB_File.txt", "text/plain")]
        [TestCase((uint)4096, "4KB_File.txt", "text/plain")]
        [TestCase((uint)8192, "8KB_File.txt", "text/plain")]
        public void GetHeadOnly_OK(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelpers.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Add the file to Filestore.
            var expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            var storedFile = _filestore.AddFile(file, _user, useMultiPartMime:true, expectedStatusCodes: expectedStatusCodes);

            // Verify that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = _filestore.GetFileMetadata(storedFile.Id, _user);

            Assert.AreEqual(fakeFileName, returnedFile.FileName,
                "The file returned filename does not match the filename of the sent file!");
            Assert.AreEqual("text/plain", returnedFile.FileType,
                "The file type returned does not match the type sent!");
        }

        [Test]
        public void Status_OK()
        { 
            // Add the file to Filestore.
            var response = _filestore.GetStatus();

            Assert.That(response == HttpStatusCode.OK, "File store service status is not OK!");
        }
    }
}
