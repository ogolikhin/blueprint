using Common;
using CustomAttributes;
using Helper;
using Model;
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
        [TestRail(164611)]
        [Description("POST a file using multipart mime. Verify that the file exists in FileStore.")]
        public void PostNovaFile_MultiPartMime_FileExists(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: create a fake file with a random byte array.
            var file = FileStoreTestHelper.CreateNovaFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Execute: Add the file to Filestore.
            var storedFile = Helper.FileStore.AddFile(file, _user, useMultiPartMime: true);

            FileStoreTestHelper.AssertNovaFilesAreIdentical(file, storedFile, compareIds: false);

            // Verify: Assert that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = Helper.FileStore.GetNovaFile(storedFile.Guid, _user);

            FileStoreTestHelper.AssertNovaFilesAreIdentical(storedFile, returnedFile);
        }


        [TestCase((uint)0, "0KB_File.txt", "text/plain")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        [TestCase((uint)2048, "2KB_File.txt", "text/plain")]
        [TestCase((uint)1048576, "1MB_File.txt", "text/plain")]
        [TestRail(164612)]
        [Description("POST a file without using multipart mime. Verify that the file exists in FileStore.")]
        public void PostNovaFile_NoMultiPartMime_FileExists(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: create a fake file with a random byte array.
            var file = FileStoreTestHelper.CreateNovaFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Execute: Add the file to Filestore.
            var storedFile = Helper.FileStore.AddFile(file, _user, useMultiPartMime: false);

            FileStoreTestHelper.AssertNovaFilesAreIdentical(file, storedFile);

            // Verify: Assert that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = Helper.FileStore.GetNovaFile(storedFile.Guid, _user);

            FileStoreTestHelper.AssertNovaFilesAreIdentical(storedFile, returnedFile);
        }

        [TestCase((uint)0, "0KB_File.txt", "text/plain")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        [TestRail(164613)]
        [Description("POST a file with a future expiry time. Verify that the file exists in FileStore")]
        public void PostNovaFile_ExpireTimeInFuture_FileExists(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: create a fake file with a random byte array.
            var file = FileStoreTestHelper.CreateNovaFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            var futureDate = DateTime.Now.AddDays(1);

            // Execute: Add the file to Filestore.
            var storedFile = Helper.FileStore.AddFile(file, _user, expireTime: futureDate);

            FileStoreTestHelper.AssertNovaFilesAreIdentical(file, storedFile, compareIds: false);

            // Verify: Assert that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = Helper.FileStore.GetNovaFile(storedFile.Guid, _user);

            FileStoreTestHelper.AssertNovaFilesAreIdentical(storedFile, returnedFile);
        }

        [TestCase((uint)8192, "8KB_File.txt", "text/plain", (uint)1024)]
        [TestRail(164614)]
        [Description("POST a file in chunks while using multipart mime. Verify that an unauthorized or bad exception is returned.")]
        public void PostNovaFile_UsingChunksMultiPartMime_MethodNotAllowedException(uint fileSize, string fakeFileName, string fileType, uint chunkSize)
        {
            // Setup: create a fake file with a random byte array.
            var file = FileStoreTestHelper.CreateNovaFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            Assert.Throws<Http405MethodNotAllowedException>(() =>
            {
                // Execute: Add the file to Filestore.
                Helper.FileStore.AddFile(file, _user, useMultiPartMime: true, chunkSize: chunkSize);
            }, "FileStore should return a Http405MethodNotAllowedException error when PUT method is used by enabling Chunk option at the event of Adding File to FileStore.");
        }

        [TestCase((uint)8192, "8KB_File.txt", "text/plain", (uint)1024)]
        [TestRail(164615)]
        [Description("POST a file in chunks without using multipart mime.Verify that an unauthorized or bad exception is returned.")]
        public void PostNovaFile_UsingChunksNoMultiPartMime_MethodNotAllowedException(uint fileSize, string fakeFileName, string fileType, uint chunkSize)
        {
            // Setup: create a fake file with a random byte array.
            var file = FileStoreTestHelper.CreateNovaFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            Assert.Throws<Http405MethodNotAllowedException>(() =>
            {
                // Execute: Add the file to Filestore.
                Helper.FileStore.AddFile(file, _user, useMultiPartMime: false, chunkSize: chunkSize);
            }, "FileStore should return a Http405MethodNotAllowedException error when PUT method is used by enabling Chunk option at the event of Adding File to FileStore.");
        }

        [TestCase((uint)2048, "2KB_File.txt", "text/plain")]
        [TestRail(234423)]
        [Description("Add file to FileStore, delete it (it set Expiration Date to now), try to get file - it should return 404.")]
        public void GetNovaFile_DeletedFile_Returns404(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup:
            var file = FileStoreTestHelper.CreateAndAddFile(fileSize, fakeFileName, fileType, Helper.FileStore, _user);

            Helper.FileStore.DeleteFile(file.Guid, _user);// TODO: if FileStore delete works as expected it is equivalent of setting ExpirationDate to Now - so test 234423 is equivalent of 234445

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.FileStore.GetNovaFile(file.Guid, _user));

            // Verify:
            const string expectedMessage = "File with ID:{0} does not exist";
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.NotFound,
                I18NHelper.FormatInvariant(expectedMessage, file.Guid));
        }

        [TestCase((uint)0, "0KB_File.txt", "text/plain")]
        [TestRail(234445)]
        [Description("Add file to FileStore, set the file Expiration Date to 1 hour ago, try to get file - it should return 404.")]
        public void GetNovaFile_FileWithExpiredDateInThePast_Returns404(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup:
            var file = FileStoreTestHelper.CreateNovaFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            var expireTime = DateTime.UtcNow.AddHours(-1);
            var storedFile = Helper.FileStore.AddFile(file, _user, expireTime: expireTime);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.FileStore.GetNovaFile(file.Guid, _user));

            // Verify:
            const string expectedMessage = "File with ID:{0} does not exist";
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.NotFound,
                I18NHelper.FormatInvariant(expectedMessage, storedFile.Guid));
        }
    }
}
