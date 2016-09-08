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
    public class BasicTests : TestBase
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

        [TestCase((uint)0, "0KB_File.txt", "text/plain")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        [TestCase((uint)2048, "2KB_File.txt", "text/plain")]
        [TestCase((uint)4096, "4KB_File.txt", "text/plain")]
        [TestCase((uint)8192, "8KB_File.txt", "text/plain")]
        [TestCase((uint)1048576, "1MB_File.txt", "text/plain")]
        [TestCase((uint)1048577, "1MB_Plus1Byte_File.txt", "text/plain")]
        [TestRail(153871)]
        [Description("POST a file using multipart mime. Verify that the file exists in FileStore.")]
        public void PostFile_MultiPartMime_FileExists(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Execute: Add the file to Filestore.
            var storedFile = Helper.FileStore.AddFile(file, _user, useMultiPartMime: true);

            FileStoreTestHelper.AssertFilesAreIdentical(file, storedFile, compareIds: false);

            // Verify: Assert that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = Helper.FileStore.GetFile(storedFile.Guid, _user);

            FileStoreTestHelper.AssertFilesAreIdentical(storedFile, returnedFile);
        }

        [TestCase((uint)0, "0KB_File.txt", "text/plain")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        [TestCase((uint)2048, "2KB_File.txt", "text/plain")]
        [TestCase((uint)4096, "4KB_File.txt", "text/plain")]
        [TestCase((uint)8192, "8KB_File.txt", "text/plain")]
        [TestCase((uint)1048576, "1MB_File.txt", "text/plain")]
        [TestRail(153872)]
        [TestCase((uint)1048577, "1MB_Plus1Byte_File.txt", "text/plain")]
        [Description("POST a file without using multipart mime. Verify that the file exists in FileStore.")]
        public void PostFile_NoMultiPartMime_FileExists(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Execute: Add the file to Filestore.
            var storedFile = Helper.FileStore.AddFile(file, _user, useMultiPartMime: false);

            FileStoreTestHelper.AssertFilesAreIdentical(file, storedFile, compareIds: false);

            // Verify: Assert that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = Helper.FileStore.GetFile(storedFile.Guid, _user);

            FileStoreTestHelper.AssertFilesAreIdentical(storedFile, returnedFile);
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", (uint)512)]
        [TestCase((uint)2048, "2KB_File.txt", "text/plain", (uint)1024)]
        [TestCase((uint)4096, "4KB_File.txt", "text/plain", (uint)1024)]
        [TestCase((uint)8192, "8KB_File.txt", "text/plain", (uint)1024)]
        [TestCase((uint)1048576, "1MB_File.txt", "text/plain", (uint)10240)]
        [TestCase((uint)1048577, "1MB_Plus1Byte_File.txt", "text/plain", (uint)10240)]
        [TestRail(153873)]
        [Description("POST a file in chunks while using multipart mime. Verify that the file exists in FileStore.")]
        public void PostFile_UsingChunksMultiPartMime_FileExists(uint fileSize, string fakeFileName, string fileType, uint chunkSize)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Execute: Add the file to Filestore.
            var storedFile = Helper.FileStore.AddFile(file, _user, useMultiPartMime: true, chunkSize: chunkSize);

            FileStoreTestHelper.AssertFilesAreIdentical(file, storedFile, compareIds: false);

            // Verify: Assert that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = Helper.FileStore.GetFile(storedFile.Guid, _user);

            FileStoreTestHelper.AssertFilesAreIdentical(storedFile, returnedFile);
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", (uint)512)]
        [TestCase((uint)2048, "2KB_File.txt", "text/plain", (uint)1024)]
        [TestCase((uint)4096, "4KB_File.txt", "text/plain", (uint)1024)]
        [TestCase((uint)8192, "8KB_File.txt", "text/plain", (uint)1024)]
        [TestCase((uint)1048576, "1MB_File.txt", "text/plain", (uint)10240)]
        [TestCase((uint)1048577, "1MB_Plus1Byte_File.txt", "text/plain", (uint)10240)]
        [TestRail(153874)]
        [Description("POST a file in chunks without using multipart mime. Verify that the file exists in FileStore.")]
        public void PostFile_UsingChunksNoMultiPartMime_FileExists(uint fileSize, string fakeFileName, string fileType, uint chunkSize)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Execute: Add the file to Filestore.
            var storedFile = Helper.FileStore.AddFile(file, _user, useMultiPartMime: false, chunkSize: chunkSize);

            FileStoreTestHelper.AssertFilesAreIdentical(file, storedFile, compareIds: false);

            // Verify: Assert that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = Helper.FileStore.GetFile(storedFile.Guid, _user);

            FileStoreTestHelper.AssertFilesAreIdentical(storedFile, returnedFile);
        }

        [TestCase((uint)0, "0KB_File.txt", "text/plain")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        [TestRail(153875)]
        [Description("POST a file with a future expiry time. Verify that the file exists in FileStore")]
        public void PostFile_ExpireTimeInFuture_FileExists(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Execute: Add the file to Filestore.
            var storedFile = Helper.FileStore.AddFile(file, _user, DateTime.Now.AddDays(1));

            FileStoreTestHelper.AssertFilesAreIdentical(file, storedFile, compareIds: false);

            // Verify: Assert that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = Helper.FileStore.GetFile(storedFile.Guid, _user);

            FileStoreTestHelper.AssertFilesAreIdentical(storedFile, returnedFile);
        }

        [TestCase((uint)0, "0KB_File.txt", "text/plain")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        [TestRail(153876)]
        [Description("POST a file without an expiry time. Verify that the file exists.")]
        public void PostFile_NoExpireTime_FileExists(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Execute: Add the file to Filestore.
            var storedFile = Helper.FileStore.AddFile(file, _user);

            FileStoreTestHelper.AssertFilesAreIdentical(file, storedFile, compareIds: false);

            // Verify: Assert that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = Helper.FileStore.GetFile(storedFile.Guid, _user);

            FileStoreTestHelper.AssertFilesAreIdentical(storedFile, returnedFile);
        }

        [TestCase((uint)0, "0KB_File.txt", "text/plain")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        [TestRail(153877)]
        [Description("POST a file with an expiry time in the past. Verify that the file does not exist.")]
        public void PostFile_ExpireTimeInPast_FileNotFound(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Execute: Add the file to Filestore.
            var storedFile = Helper.FileStore.AddFile(file, _user, DateTime.Now.AddDays(-1));

            FileStoreTestHelper.AssertFilesAreIdentical(file, storedFile, compareIds: false);

            // Verify: Assert that the file doesn't exist in FileStore.
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.FileStore.GetFile(storedFile.Guid, _user);
            }, "The file still exists after it was deleted!");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", (uint)512)]
        [TestRail(153878)]
        [Description("POST a file in chunks with an expiry time in the past. Verify that the file does not exist.")]
        public void PostFile_UsingChunksExpireTimeInPast_FileNotFound(uint fileSize, string fakeFileName, string fileType, uint chunkSize)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Execute: Add the file to Filestore.
            var storedFile = Helper.FileStore.AddFile(file, _user, DateTime.Now.AddDays(-1), chunkSize: chunkSize);

            FileStoreTestHelper.AssertFilesAreIdentical(file, storedFile, compareIds: false);

            // Verify: Assert that the file doesn't exist in FileStore.
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.FileStore.GetFile(storedFile.Guid, _user);
            }, "The file still exists after it was deleted!");
        }
    }
}
