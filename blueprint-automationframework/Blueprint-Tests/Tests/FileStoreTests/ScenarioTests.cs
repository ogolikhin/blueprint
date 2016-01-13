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
    [Category(Categories.Filestore)]
    public class ScenarioTests
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
                _user.DeleteUser(deleteFromDatabase: true);
                _user = null;
            }
        }

        #endregion Setup and Cleanup

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", (uint)512)]
        [TestCase((uint)2048, "2KB_File.txt", "text/plain", (uint)1024)]
        [TestCase((uint)4096, "4KB_File.txt", "text/plain", (uint)1024)]
        [TestCase((uint)8192, "8KB_File.txt", "text/plain", (uint)1024)]
        public void PostFileInChunksWithFutureExpireTimeThenDeleteFile_VerifyFileWasAddedAndDeleted(uint fileSize, string fakeFileName, string fileType, uint chunkSize)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Add the file to Filestore.
            var storedFile = _filestore.AddFile(file, _user, DateTime.Now.AddDays(1), chunkSize: chunkSize);

            FileStoreTestHelper.AssertFilesAreIdentical(file, storedFile, compareIds: false);

            // Verify that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = _filestore.GetFile(storedFile.Id, _user);

            FileStoreTestHelper.AssertFilesAreIdentical(storedFile, returnedFile);

            // Now delete the file.
            _filestore.DeleteFile(storedFile.Id, _user);

            // Verify the file was deleted.
            Assert.Throws<Http404NotFoundException>(() =>
            {
                _filestore.GetFile(storedFile.Id, _user);
            }, "The file still exists after it was deleted!");
        }
    }
}
