using System;
using System.Threading;
using Common;
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

            // Execute: Now delete the file.
            _filestore.DeleteFile(storedFile.Id, _user);
            
            const int SLEEP_MS = 50;
            const int MAX_ATTEMPTS = 5;

            // We believe there may be a timing issue where you can still get the file for a very small time after we delete it (milliseconds),
            // so we're trying to get it several times and sleeping in between retries.
            for (int attempt = 0; attempt < MAX_ATTEMPTS; ++attempt)
            {
                returnedFile = null;

                Exception ex = ExceptionHelper.Catch(() =>
                {
                    returnedFile = _filestore.GetFile(storedFile.Id, _user);
                });

                if (ex == null)
                {
                    // We found the file after it was deleted.  Sleep and try again in case it's a SQL timing issue.
                    if (attempt < MAX_ATTEMPTS)
                    {
                        Logger.WriteWarning(
                            "The file was found after deleting it.  Sleeping for {0}ms before trying to get the file again...", SLEEP_MS);
                        Thread.Sleep(SLEEP_MS);
                    }
                }
                else if (ex is Http404NotFoundException)
                {
                    // This is the exception we expect to get.
                    break;
                }
                else
                {
                    Assert.Fail("When getting a file that was deleted we should get a 404 error, but instead we got: {0}\n{1}",
                        ex.Message, ex.StackTrace);
                }
            }

            Assert.Null(returnedFile, "The '{0}' file was found after we deleted it!", file.FileName);
        }
    }
}
