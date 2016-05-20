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
    public class DeleteTests : TestBase
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
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        [TestCase((uint)2048, "2KB_File.txt", "text/plain")]
        [TestCase((uint)4096, "4KB_File.txt", "text/plain")]
        [TestCase((uint)8192, "8KB_File.txt", "text/plain")]
        public void DeleteFileWithFutureExpiryDate_VerifyFileStillExists(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: Create and add file to FileStore.
            var storedFile = FileStoreTestHelper.CreateAndAddFile(fileSize, fakeFileName, fileType, Helper.FileStore, _user);

            // Delete file with future expiry date.
            Helper.FileStore.DeleteFile(storedFile.Id, _user, DateTime.Now.AddDays(1));

            var returnedFile = Helper.FileStore.GetFile(storedFile.Id, _user);

            // Verify that the file still exists after deleting with a future expire time
            FileStoreTestHelper.AssertFilesAreIdentical(storedFile, returnedFile);
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        public void DeleteFileImmediately_VerifyFileIsDeleted(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: Create and add file to FileStore.
            var storedFile = FileStoreTestHelper.CreateAndAddFile(fileSize, fakeFileName, fileType, Helper.FileStore, _user);

            // Delete the file immediately.
            Helper.FileStore.DeleteFile(storedFile.Id, _user);

            // Verify the file was deleted.
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.FileStore.GetFile(storedFile.Id, _user);
            }, "The file still exists after it was deleted!");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        public void DeleteFileWithPastExpiryDate_VerifyFileIsDeleted(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: Create and add file to FileStore.
            var storedFile = FileStoreTestHelper.CreateAndAddFile(fileSize, fakeFileName, fileType, Helper.FileStore, _user);

            // Delete the file with an expiry date in the past.
            Helper.FileStore.DeleteFile(storedFile.Id, _user, DateTime.Now.AddDays(-1));

            // Verify the file was deleted.
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.FileStore.GetFile(storedFile.Id, _user);
            }, "The file still exists after it was deleted!");
        }
    }
}
