using System;
using CustomAttributes;
using Helper;
using Model;
using NUnit.Framework;
using TestCommon;
using Utilities;

namespace FileStoreTests
{
    [TestFixture]
    [Category(Categories.FileStore)]
    public class ScenarioTests : TestBase
    {
        private IUser _user;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            Helper.Dispose();
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
            var storedFile = Helper.FileStore.AddFile(file, _user, DateTime.Now.AddDays(1), chunkSize: chunkSize);

            FileStoreTestHelper.AssertFilesAreIdentical(file, storedFile, compareIds: false);

            // Verify that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = Helper.FileStore.GetFile(storedFile.Id, _user);

            FileStoreTestHelper.AssertFilesAreIdentical(storedFile, returnedFile);

            // Execute: Now delete the file.
            Helper.FileStore.DeleteFile(storedFile.Id, _user);

            const int maxAttempts = 5;
            const int sleepMs = 50;

            // We believe there may be a timing issue where you can still get the file for a very small time after we delete it (milliseconds),
            // so we're trying to get it several times and sleeping in between retries.
            ExceptionHelper.RetryIfExceptionNotThrown<Http404NotFoundException>(() =>
            {
                returnedFile = null;
                returnedFile = Helper.FileStore.GetFile(storedFile.Id, _user);
            }, maxAttempts, sleepMs, "The '{0}' file was found after we deleted it!", file.FileName);

            Assert.Null(returnedFile, "The '{0}' file was found after we deleted it!", file.FileName);
        }
    }
}
