using CustomAttributes;
using Helper;
using Model;
using NUnit.Framework;
using TestCommon;

namespace FileStoreTests
{
    [TestFixture]
    [Category(Categories.FileStore)]
    public class FileTypeTests : TestBase
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

        [TestCase((uint)1024, "1KB_File.csv", "text/csv")]
        [TestCase((uint)2048, "2KB_File.html", "text/html")]
        [TestCase((uint)4096, "4KB_File.jpg", "image/jpeg")]
        [TestCase((uint)8192, "8KB_File.pdf", "application/pdf")]
        public void PostFileWithMultiPartMime_VerifyReturnedFileType(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Add the file to Filestore.
            var storedFile = Helper.FileStore.AddFile(file, _user, useMultiPartMime: true);

            FileStoreTestHelper.AssertFilesAreIdentical(file, storedFile, compareIds: false);

            // Verify that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = Helper.FileStore.GetFile(storedFile.Id, _user);

            FileStoreTestHelper.AssertFilesAreIdentical(storedFile, returnedFile);
        }

        [TestCase((uint)1024, "1KB_File.csv", "text/csv")]
        [TestCase((uint)2048, "2KB_File.html", "text/html")]
        [TestCase((uint)4096, "4KB_File.jpg", "image/jpeg")]
        [TestCase((uint)8192, "8KB_File.pdf", "application/pdf")]
        public void PostFileWithoutMultiPartMime_VerifyReturnedFileType(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Add the file to Filestore.
            var storedFile = Helper.FileStore.AddFile(file, _user, useMultiPartMime: false);

            FileStoreTestHelper.AssertFilesAreIdentical(file, storedFile, compareIds: false);

            // Verify that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = Helper.FileStore.GetFile(storedFile.Id, _user);

            FileStoreTestHelper.AssertFilesAreIdentical(storedFile, returnedFile);
        }
    }
}
