using CustomAttributes;
using Helper;
using Model;
using Model.NovaModel;
using NUnit.Framework;
using System;
using TestCommon;
using Utilities;

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
        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        [TestCase((uint)2048, "2KB_File.txt", "text/plain")]
        [TestCase((uint)1048576, "1MB_File.txt", "text/plain")]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("POST a file using multipart mime. Verify that the file exists in FileStore.")]
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


        [TestCase((uint)0, "0KB_File.txt", "text/plain")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        [TestCase((uint)2048, "2KB_File.txt", "text/plain")]
        [TestCase((uint)1048576, "1MB_File.txt", "text/plain")]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("POST a file without using multipart mime. Verify that the file exists in FileStore.")]
        public void BPPostFile_NoMultiPartMime_FileExists(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: create a fake file with a random byte array.
            INovaFile file = FileStoreTestHelper.CreateNovaFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Execute: Add the file to Filestore.
            var storedFile = Helper.FileStore.NovaAddFile(file, _user, useMultiPartMime: false);

            FileStoreTestHelper.AssertNovaFilesAreIdentical(file, storedFile, compareIds: false);

            // Verify: Assert that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = Helper.FileStore.NovaGetFile(storedFile.Guid, _user);

            FileStoreTestHelper.AssertNovaFilesAreIdentical(storedFile, returnedFile);
        }

        [TestCase((uint)0, "0KB_File.txt", "text/plain")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("POST a file with a future expiry time. Verify that the file exists in FileStore")]
        public void BPPostFile_ExpireTimeInFuture_FileExists(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: create a fake file with a random byte array.
            INovaFile file = FileStoreTestHelper.CreateNovaFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            var futureDate = DateTime.Now.AddDays(1);

            // Execute: Add the file to Filestore.
            var storedFile = Helper.FileStore.NovaAddFile(file, _user, expireTime: futureDate);

            FileStoreTestHelper.AssertNovaFilesAreIdentical(file, storedFile, compareIds: false);

            // Verify: Assert that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = Helper.FileStore.NovaGetFile(storedFile.Guid, _user);

            FileStoreTestHelper.AssertNovaFilesAreIdentical(storedFile, returnedFile);
        }

        [TestCase((uint)8192, "8KB_File.txt", "text/plain", (uint)1024)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("POST a file in chunks while using multipart mime. Verify that an unauthorized or bad exception is returned.")]
        public void BPPostFile_UsingChunksMultiPartMime_MethodNotAllowedException(uint fileSize, string fakeFileName, string fileType, uint chunkSize)
        {
            // Setup: create a fake file with a random byte array.
            INovaFile file = FileStoreTestHelper.CreateNovaFileWithRandomByteArray(fileSize, fakeFileName, fileType);


            Assert.Throws<Http405MethodNotAllowedException>(() =>
            {
                // Execute: Add the file to Filestore.
                Helper.FileStore.NovaAddFile(file, _user, useMultiPartMime: true, chunkSize: chunkSize);
            }, "FileStore should return a Http405MethodNotAllowedException error when PUT method is used by enabling Chunk option at the event of Adding File to FileStore.");
        }

        [TestCase((uint)8192, "8KB_File.txt", "text/plain", (uint)1024)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("POST a file in chunks without using multipart mime.Verify that an unauthorized or bad exception is returned.")]
        public void BPPostFile_UsingChunksNoMultiPartMime_MethodNotAllowedException(uint fileSize, string fakeFileName, string fileType, uint chunkSize)
        {
            // Setup: create a fake file with a random byte array.
            INovaFile file = FileStoreTestHelper.CreateNovaFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            Assert.Throws<Http405MethodNotAllowedException>(() =>
            {
                // Execute: Add the file to Filestore.
                Helper.FileStore.NovaAddFile(file, _user, useMultiPartMime: false, chunkSize: chunkSize);
            }, "FileStore should return a Http405MethodNotAllowedException error when PUT method is used by enabling Chunk option at the event of Adding File to FileStore.");
        }
    }
}
