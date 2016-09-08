using System.Linq;
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
    public class CookieWithInvalidSessionTokenTests : TestBase
    {
        private IUser _user;
        private IUser _userForCookieTests;

        [SetUp]
        public void TestSetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);

            // Get a valid token for the user for authorization tests.
            _userForCookieTests = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);
        }

        [TearDown]
        public void TestTearDown()
        {
            Helper?.Dispose();
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "a")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "")]
        [TestRail(153879)]
        [Description("POST a file with an invalid cookie session token. Verify that a bad request exception is returned.")]
        public void PostFile_InvalidCookieSessionToken_BadRequestException(
            uint fileSize,
            string fakeFileName,
            string fileType,
            string accessControlToken)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Replace token with invalid token
            _userForCookieTests.Token.AccessControlToken = accessControlToken;

            // Execute & Verify: Assert that bad request exception is thrown
            Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.FileStore.AddFile(file, _userForCookieTests, sendAuthorizationAsCookie: true);
            }, "HTTP Status Code 400 (Bad Request) was expected because POST does not support authorization cookies!");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", (uint)512, "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", (uint)512, "a")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", (uint)512, "")]
        [TestRail(153880)]
        [Description("PUT a file with an invalid cookie session token. Verify that a bad request exception is returned.")]
        public void PutFile_InvalidCookieSessionToken_BadRequestException(
            uint fileSize,
            string fakeFileName,
            string fileType,
            uint chunkSize,
            string accessControlToken)
        {
            Assert.That((chunkSize > 0) && (fileSize > chunkSize), "Invalid TestCase detected!  chunkSize must be > 0 and < fileSize.");

            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            byte[] fileBytes = file.Content.ToArray();
            byte[] chunk = fileBytes.Take((int)chunkSize).ToArray();

            // First POST the first chunk with a valid token.
            file.Content = chunk;
            IFile postedFile = Helper.FileStore.PostFile(file, _user);

            byte[] rem = fileBytes.Skip((int)chunkSize).ToArray();
            chunk = rem.Take((int)chunkSize).ToArray();

            // Replace token with invalid token
            _userForCookieTests.Token.AccessControlToken = accessControlToken;

            // Execute & Verify: Assert that bad request exception is thrown for subsequent PUT request with invalid token
            Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.FileStore.PutFile(postedFile, chunk, _userForCookieTests, sendAuthorizationAsCookie: true);
            }, "HTTP Status Code 400 (Bad Request) was expected because PUT does not support authorization cookies!");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "a")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "")]
        [TestRail(153881)]
        [Description("GET a file with an invalid cookie session token. Verify that an unauthorized exception is returned.")]
        public void GetFile_InvalidCookieSessionToken_UnauthorizedException(
            uint fileSize,
            string fakeFileName,
            string fileType,
            string accessControlToken)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Add the file to Filestore.
            var storedFile = Helper.FileStore.AddFile(file, _user, useMultiPartMime: true);

            // Replace token with invalid token
            _userForCookieTests.Token.AccessControlToken = accessControlToken;

            // Execute & Verify: Assert that unauthorized exception is thrown
            // Note: Empty authorization cookie returns 401 Unauthorized
            //       Empty authorization session header returns 400 Bad Request
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                Helper.FileStore.GetFile(storedFile.Guid, _userForCookieTests, sendAuthorizationAsCookie: true);
            }, "Did not throw HTTP Status Code 401 (Unauthorized) as expected");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "a")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "")]
        [TestRail(153882)]
        [Description("GET HEAD for a file with an invalid cookie session token. Verify that a bad request exception is returned.")]
        public void GetFileHead_InvalidCookieSessionToken_BadRequestException(
            uint fileSize,
            string fakeFileName,
            string fileType,
            string accessControlToken)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Add the file to Filestore.
            var storedFile = Helper.FileStore.AddFile(file, _user, useMultiPartMime: true);

            // Replace token with invalid token
            _userForCookieTests.Token.AccessControlToken = accessControlToken;

            // Execute & Verify: Assert that bad request exception is thrown
            Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.FileStore.GetFileMetadata(storedFile.Guid, _userForCookieTests, sendAuthorizationAsCookie: true);
            }, "HTTP Status Code 400 (Bad Request) was expected because HEAD does not support authorization cookies!");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "a")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "")]
        [TestRail(153883)]
        [Description("DELETE a file with an invalid cookie session token. Verify that a bad request exception is returned.")]
        public void DeleteFile_InvalidCookieToken_BadRequestException(
            uint fileSize,
            string fakeFileName,
            string fileType,
            string accessControlToken)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Add the file to Filestore.
            var storedFile = Helper.FileStore.AddFile(file, _user);

            // Replace token with invalid token
            _userForCookieTests.Token.AccessControlToken = accessControlToken;

            // Execute & Verify: Assert that bad request exception is thrown
            Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.FileStore.DeleteFile(storedFile.Guid, _userForCookieTests, sendAuthorizationAsCookie: true);
            }, "HTTP Status Code 400 (Bad Request) was expected because DELETE does not support authorization cookies!");
        }
    }
}
