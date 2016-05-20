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
            Helper.Dispose();
        }

        #endregion Setup and Cleanup

        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        [TestCase((uint)2048, "2KB_File.txt", "text/plain")]
        [TestCase((uint)4096, "4KB_File.txt", "text/plain")]
        [TestCase((uint)8192, "8KB_File.txt", "text/plain")]
        public void GetHeadOnly_VerifyFileMetaDataIdenticalToAddedFile(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: Create and add file to FileStore.
            var storedFile = FileStoreTestHelper.CreateAndAddFile(fileSize, fakeFileName, fileType, Helper.FileStore, _user);

            // Get file meta-data.
            var returnedFile = Helper.FileStore.GetFileMetadata(storedFile.Id, _user);

            // Verify meta-data is the same as the file we added.
            FileStoreTestHelper.AssertFilesMetadataAreIdentical(storedFile, returnedFile);
        }
    }
}
