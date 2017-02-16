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
        private const string NOVAFILE_PATH = RestPaths.Svc.FileStore.NOVAFILES;
        private const string MAXATTACHMENTFILESIZE = "MaxAttachmentFileSize";

        private IUser _adminUser = null;

        #region Setup and Cleanup

        [SetUp]
        public void ClassSetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);
        }

        [TearDown]
        public void ClassTearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region 200 OK Tests

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
            var storedFile = Helper.FileStore.AddFile(file, _adminUser, useMultiPartMime: true);

            FileStoreTestHelper.AssertNovaFilesAreIdentical(file, storedFile, compareIds: false);

            // Verify: Assert that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = Helper.FileStore.GetNovaFile(storedFile.Guid, _adminUser);

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
            var storedFile = Helper.FileStore.AddFile(file, _adminUser, useMultiPartMime: false);

            FileStoreTestHelper.AssertNovaFilesAreIdentical(file, storedFile);

            // Verify: Assert that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = Helper.FileStore.GetNovaFile(storedFile.Guid, _adminUser);

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

            var futureDate = DateTime.UtcNow.AddDays(1);

            // Execute: Add the file to Filestore.
            var storedFile = Helper.FileStore.AddFile(file, _adminUser, expireTime: futureDate);

            FileStoreTestHelper.AssertNovaFilesAreIdentical(file, storedFile, compareIds: false);

            // Verify: Assert that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = Helper.FileStore.GetNovaFile(storedFile.Guid, _adminUser);

            FileStoreTestHelper.AssertNovaFilesAreIdentical(storedFile, returnedFile);
        }

        #endregion 200 OK Tests

        #region 404 Not Found Tests

        [TestCase((uint)2048, "2KB_File.txt", "text/plain")]
        [TestRail(234423)]
        [Description("Add file to FileStore, delete it, try to get file - it should return 404.")]
        public void GetNovaFile_DeletedFile_Returns404(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup:
            var file = FileStoreTestHelper.CreateAndAddFile(fileSize, fakeFileName, fileType, Helper.FileStore, _adminUser);

            Helper.FileStore.DeleteFile(file.Guid, _adminUser);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.FileStore.GetNovaFile(file.Guid, _adminUser));

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
            var storedFile = Helper.FileStore.AddFile(file, _adminUser, expireTime: expireTime);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.FileStore.GetNovaFile(file.Guid, _adminUser));

            // Verify:
            const string expectedMessage = "File with ID:{0} does not exist";
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.NotFound,
                I18NHelper.FormatInvariant(expectedMessage, storedFile.Guid));
        }

        #endregion 404 Not Found Tests

        #region 405 Method Not Allowed Tests

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
                Helper.FileStore.AddFile(file, _adminUser, useMultiPartMime: true, chunkSize: chunkSize);
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
                Helper.FileStore.AddFile(file, _adminUser, useMultiPartMime: false, chunkSize: chunkSize);
            }, "FileStore should return a Http405MethodNotAllowedException error when PUT method is used by enabling Chunk option at the event of Adding File to FileStore.");
        }

        #endregion 405 Method Not Allowed Tests

        #region 409 Conflict Tests

        [TestCase("LargerThanMaxFileSizeFile.txt", "text/plain")]
        [TestRail(246532)]
        [Description("POST a file larger than MaxFileSize set on ApplicationSetttings. Verify that 409 Conflict exception gets returned.")]
        public void PostNovaFile_UsingFileLargerThanMaxFileSize_409Conflict(string fakeFileName, string fileType)
        {
            // Setup: get the default MaxFileSize and create a file larger than the MaxFileSize
            var maxAttachmentFileSizeInBytes = TestHelper.GetApplicationSetting(MAXATTACHMENTFILESIZE);

            var largerThanMaxFileSize = maxAttachmentFileSizeInBytes.ToInt32Invariant() + 1;

            var file = FileStoreTestHelper.CreateNovaFileWithRandomByteArray((uint)largerThanMaxFileSize, fakeFileName, fileType);

            var futureDate = DateTime.UtcNow.AddDays(1);

            // Execute: Add the file larger than the MaxFileSize
            var ex = Assert.Throws<Http409ConflictException>(() =>
            {
                Helper.FileStore.AddFile(file, _adminUser, expireTime: futureDate, useMultiPartMime: false);
            }, "GET {0} call should return 409 Conflict when adding file larger than MaxFileSize!", NOVAFILE_PATH);

            // Verify: Check that 409 conflict with expected error
            int maxAttachmentFileSizeInMegabytes = maxAttachmentFileSizeInBytes.ToInt32Invariant() / 1024 / 1024;

            string expectedMessage = I18NHelper.FormatInvariant("The file exceeds {0} MB.", maxAttachmentFileSizeInMegabytes);

            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.ExceedsLimit, expectedMessage);
        }

        #endregion 409 Conflict Tests
    }
}
