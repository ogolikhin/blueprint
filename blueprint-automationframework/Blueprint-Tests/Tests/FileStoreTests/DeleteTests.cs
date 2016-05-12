using System;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using NUnit.Framework;
using Utilities;

namespace FileStoreTests
{
    [TestFixture]
    [Category(Categories.FileStore)]
    public class DeleteTests
    {
        private IAdminStore _adminStore;
        private IFileStore _filestore;
        private IUser _user;

        #region Setup and Cleanup

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
            if (_filestore != null)
            {
                // Delete all the files that were added.
                foreach (var file in _filestore.Files.ToArray())
                {
                    _filestore.DeleteFile(file.Id, _user);
                }
            }

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
                // Delete the user we created.
                _user.DeleteUser(deleteFromDatabase: true);
                _user = null;
            }
        }

        #endregion Setup and Cleanup

        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        [TestCase((uint)2048, "2KB_File.txt", "text/plain")]
        [TestCase((uint)4096, "4KB_File.txt", "text/plain")]
        [TestCase((uint)8192, "8KB_File.txt", "text/plain")]
        public void DeleteFileWithFutureExpiryDate_VerifyFileStillExists(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: Create and add file to FileStore.
            var storedFile = FileStoreTestHelper.CreateAndAddFile(fileSize, fakeFileName, fileType, _filestore, _user);

            // Delete file with future expiry date.
            _filestore.DeleteFile(storedFile.Id, _user, DateTime.Now.AddDays(1));

            var returnedFile = _filestore.GetFile(storedFile.Id, _user);

            // Verify that the file still exists after deleting with a future expire time
            FileStoreTestHelper.AssertFilesAreIdentical(storedFile, returnedFile);
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        public void DeleteFileImmediately_VerifyFileIsDeleted(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: Create and add file to FileStore.
            var storedFile = FileStoreTestHelper.CreateAndAddFile(fileSize, fakeFileName, fileType, _filestore, _user);

            // Delete the file immediately.
            _filestore.DeleteFile(storedFile.Id, _user);

            // Verify the file was deleted.
            Assert.Throws<Http404NotFoundException>(() =>
            {
                _filestore.GetFile(storedFile.Id, _user);
            }, "The file still exists after it was deleted!");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        public void DeleteFileWithPastExpiryDate_VerifyFileIsDeleted(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: Create and add file to FileStore.
            var storedFile = FileStoreTestHelper.CreateAndAddFile(fileSize, fakeFileName, fileType, _filestore, _user);

            // Delete the file with an expiry date in the past.
            _filestore.DeleteFile(storedFile.Id, _user, DateTime.Now.AddDays(-1));

            // Verify the file was deleted.
            Assert.Throws<Http404NotFoundException>(() =>
            {
                _filestore.GetFile(storedFile.Id, _user);
            }, "The file still exists after it was deleted!");
        }
    }
}
