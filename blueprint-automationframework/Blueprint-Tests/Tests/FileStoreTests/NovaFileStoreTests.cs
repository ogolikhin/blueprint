using CustomAttributes;
using Helper;
using Model;
using Model.NovaModel;
using NUnit.Framework;
using TestCommon;

namespace FileStoreTests
{
    [TestFixture]
    [Category(Categories.FileStore)]
    public class NovaFileStoreTests : TestBase
    {
        private IUser _user = null;

        [SetUp]
        public void ClassSetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);
        }

        [TearDown]
        public void ClassTearDown()
        {
            Helper?.Dispose();
        }

        [TestCase((uint)0, "0KB_File.txt", "text/plain")]
        [TestCase((uint)10, "10B_File.txt", "text/plain")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        [TestCase((uint)2048, "2KB_File.txt", "text/plain")]
        [Description("POST a file using multipart mime. Verify that the file exists in FileStore.")]
        [Ignore(IgnoreReasons.UnderDevelopment)]
        public void BPPostFile_MultiPartMime_FileExists(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: create a fake file with a random byte array.
            INovaFile file = FileStoreTestHelper.CreateNovaFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Execute: Add the file to Filestore.
            var storedFile = Helper.FileStore.NovaAddFile(file, _user, useMultiPartMime: true);

            FileStoreTestHelper.AssertNovaFilesAreIdentical(file, storedFile, compareIds: false);

            // Verify: Assert that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = Helper.FileStore.NovaGetFile(storedFile.Guid, _user);

            FileStoreTestHelper.AssertNovaFilesAreIdentical(storedFile, returnedFile);
        }
    }
}
