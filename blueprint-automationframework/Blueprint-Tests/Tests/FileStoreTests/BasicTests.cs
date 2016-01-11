using System;
using System.Collections.Generic;
using System.Net;
using CustomAttributes;
using Model;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;

namespace FileStoreTests
{
    [TestFixture]
    [Category(Categories.Filestore)]
    public class BasicTests
    {
        private IAdminStore _adminStore;
        private IFileStore _filestore;
        private IUser _user;

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            _filestore = FileStoreFactory.GetFileStoreFromTestConfig();
            _user = UserFactory.CreateUserAndAddToDatabase();

            // Get a valid token for the user.
            ISession session = _adminStore.AddSession(_user.Username, _user.Password);
            _user.SetToken(session.SessionId);

            Assert.IsFalse(string.IsNullOrWhiteSpace(_user.Token.AccessControlToken), "The user didn't get an Access Control token!");
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            if (_adminStore != null)
            {
                // Delete all the sessions that were created.
                foreach (var session in _adminStore.Sessions.ToArray())
                {
                    _adminStore.DeleteSession(session);
                }
            }

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
        public void PostFileWithMultiMimeParts_VerifyFileExists(uint fileSize, string fakeFileName, string fileType)
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
        public void PostFileWithoutMultiMimeParts_VerifyFileExists(uint fileSize, string fakeFileName, string fileType)
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
        public void PostFileWithExpireTimeThenDeleteFile_VerifyFileWasAddedAndDeleted(uint fileSize, string fakeFileName, string fileType, uint chunkSize)
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
        public void PostFileThenDeleteWithFutureExpireTime_VerifyFileWasAddedButNotDeleted_ThenDeleteImmediately(uint fileSize, string fakeFileName, string fileType, uint chunkSize)
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
