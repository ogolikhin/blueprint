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
        [TestRail(153889)]
        [Description("DELETE a file and set a future expiry date for the file. Verify that the file still exists.")]
        public void DeleteFile_ExpiryDateInFuture_FileStillExists(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: Create and add file to FileStore.
            var storedFile = FileStoreTestHelper.CreateAndAddFile(fileSize, fakeFileName, fileType, Helper.FileStore, _user);

            // Execute: Delete file with future expiry date.
            Helper.FileStore.DeleteFile(storedFile.Guid, _user, DateTime.Now.AddDays(1));

            var returnedFile = Helper.FileStore.GetFile(storedFile.Guid, _user);

            // Verify: Assert that the file still exists after deleting with a future expire time
            FileStoreTestHelper.AssertFilesAreIdentical(storedFile, returnedFile);
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        [TestRail(153890)]
        [Description("DELETE a file immediately without an expiry date. Verify that the file is deleted.")]
        public void DeleteFile_Immediately_FileIsDeleted(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: Create and add file to FileStore.
            var storedFile = FileStoreTestHelper.CreateAndAddFile(fileSize, fakeFileName, fileType, Helper.FileStore, _user);

            // Execute: Delete the file immediately.
            Helper.FileStore.DeleteFile(storedFile.Guid, _user);

            // Verify: Assert that the file was deleted.
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.FileStore.GetFile(storedFile.Guid, _user);
            }, "The file still exists after it was deleted!");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        [TestRail(153891)]
        [Description("DELETE a file with an expiry date in the past. Verify that the file is deleted.")]
        public void DeleteFile_ExpiryDateInPast_FileIsDeleted(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: Create and add file to FileStore.
            var storedFile = FileStoreTestHelper.CreateAndAddFile(fileSize, fakeFileName, fileType, Helper.FileStore, _user);

            // Execute: Delete the file with an expiry date in the past.
            Helper.FileStore.DeleteFile(storedFile.Guid, _user, DateTime.Now.AddDays(-1));

            // Verify: Assert that the file was deleted.
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.FileStore.GetFile(storedFile.Guid, _user);
            }, "The file still exists after it was deleted!");
        }
    }
}
