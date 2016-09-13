using CustomAttributes;
using Helper;
using Model;
using NUnit.Framework;
using TestCommon;

namespace FileStoreTests
{
    [TestFixture]
    [Category(Categories.FileStore)]
    public class HeadTests : TestBase
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
        [TestRail(153894)]
        [Description("GET HEAD for a file. Verify that the file metadata is identical to the metadata of the file that was added.")]
        public void GetFileHead_ReturnedFileMetaDataIdenticalToAddedFile(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: Create and add file to FileStore.
            var storedFile = FileStoreTestHelper.CreateAndAddFile(fileSize, fakeFileName, fileType, Helper.FileStore, _user);

            // Execute: Get file meta-data.
            var returnedFile = Helper.FileStore.GetFileMetadata(storedFile.Guid, _user);

            // Verify: Assert that the meta-data is the same as the file that we added.
            FileStoreTestHelper.AssertFilesMetadataAreIdentical(storedFile, returnedFile);
        }
    }
}
