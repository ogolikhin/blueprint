using System;
using System.Linq;
using Common;
using CustomAttributes;
using Helper;
using Model;
using NUnit.Framework;
using TestCommon;
using Utilities;
using Utilities.Facades;

namespace FileStoreTests
{
    [TestFixture]
    [Category(Categories.FileStore)]
    public class NegativeTests : TestBase
    {
        const string SVC_FILES_PATH = "svc/filestore/files";

        private IUser _user;

        [SetUp]
        public void TestSetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);
        }

        [TearDown]
        public void TestTearDown()
        {
            Helper?.Dispose();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", typeof(Http401UnauthorizedException))]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "a", typeof(Http401UnauthorizedException))]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "", typeof(Http401UnauthorizedException))]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", null, typeof(Http400BadRequestException))]
        [TestRail(153895)]
        [Description("POST a file with an invalid session token. Verify that an unauthorized or bad request exception is returned.")]
        public void PostFile_InvalidSessionToken_UnauthorizedOrBadRequestException(
            uint fileSize,
            string fakeFileName,
            string fileType,
            string accessControlToken,
            Type exceptionType)
        {
            ThrowIf.ArgumentNull(exceptionType, nameof(exceptionType));

            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            IUser userWithInvalidToken = CreateUserWithInvalidToken(accessControlToken);

            // Execute & Verify: Assert that exception is thrown
            Assert.Throws(exceptionType, () =>
            {
                Helper.FileStore.AddFile(file, userWithInvalidToken);
            }, I18NHelper.FormatInvariant("Did not throw {0} as expected", exceptionType.Name));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", (uint)512, "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", typeof(Http401UnauthorizedException))]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", (uint)512, "a", typeof(Http401UnauthorizedException))]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", (uint)512, "", typeof(Http401UnauthorizedException))]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", (uint)512, null, typeof(Http400BadRequestException))]
        [TestRail(153896)]
        [Description("PUT a file with an invalid session token. Verify that an unauthorized or bad request exception is returned.")]
        public void PutFile_InvalidSessionToken_UnauthorizedOrBadRequestException(
            uint fileSize,
            string fakeFileName,
            string fileType,
            uint chunkSize,
            string accessControlToken,
            Type exceptionType)
        {
            ThrowIf.ArgumentNull(exceptionType, nameof(exceptionType));

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

            IUser userWithInvalidToken = CreateUserWithInvalidToken(accessControlToken);

            // Execute & Verify: Assert that exception is thrown for subsequent PUT request with invalid token
            Assert.Throws(exceptionType, () =>
            {
                Helper.FileStore.PutFile(postedFile, chunk, userWithInvalidToken);
            }, I18NHelper.FormatInvariant("Did not throw {0} as expected", exceptionType.Name));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", typeof(Http401UnauthorizedException))]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "a", typeof(Http401UnauthorizedException))]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "", typeof(Http401UnauthorizedException))]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", null, typeof(Http400BadRequestException))]
        [TestRail(153897)]
        [Description("GET a file with an invalid session token. Verify that an unauthorized or bad request exception is returned.")]
        public void GetFile_InvalidSessionToken_UnauthorizedOrBadRequestException(
            uint fileSize,
            string fakeFileName,
            string fileType,
            string accessControlToken,
            Type exceptionType)
        {
            ThrowIf.ArgumentNull(exceptionType, nameof(exceptionType));

            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Add the file to Filestore.
            var storedFile = Helper.FileStore.AddFile(file, _user, useMultiPartMime: true);

            IUser userWithInvalidToken = CreateUserWithInvalidToken(accessControlToken);

            // Execute & Verify: Assert that exception is thrown
            // Note: Empty authorization cookie returns 401 Unauthorized
            //       Empty authorization session header returns 400 Bad Request
            Assert.Throws(exceptionType, () =>
            {
                Helper.FileStore.GetFile(storedFile.Guid, userWithInvalidToken);
            }, I18NHelper.FormatInvariant("Did not throw {0} as expected", exceptionType.Name));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", typeof(Http401UnauthorizedException))]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "a", typeof(Http401UnauthorizedException))]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "", typeof(Http401UnauthorizedException))]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", null, typeof(Http400BadRequestException))]
        [TestRail(153898)]
        [Description("GET HEAD for a file with an invalid session token. Verify that an unauthorized or bad request exception is returned.")]
        public void GetFileHead_InvalidSessionToken_UnauthorizedOrBadRequestException(
            uint fileSize,
            string fakeFileName,
            string fileType,
            string accessControlToken,
            Type exceptionType)
        {
            ThrowIf.ArgumentNull(exceptionType, nameof(exceptionType));

            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Add the file to Filestore.
            var storedFile = Helper.FileStore.AddFile(file, _user, useMultiPartMime: true);

            IUser userWithInvalidToken = CreateUserWithInvalidToken(accessControlToken);

            // Execute & Verify: Assert that exception is thrown
            Assert.Throws(exceptionType, () =>
            {
                Helper.FileStore.GetFileMetadata(storedFile.Guid, userWithInvalidToken);
            }, I18NHelper.FormatInvariant("Did not throw {0} as expected", exceptionType.Name));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", typeof(Http401UnauthorizedException))]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "a", typeof(Http401UnauthorizedException))]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "", typeof(Http401UnauthorizedException))]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", null, typeof(Http400BadRequestException))]
        [TestRail(153899)]
        [Description("DELETE a file with an invalid session token. Verify that an unauthorized or bad exception is returned.")]
        public void DeleteFile_InvalidSessionToken_UnauthorizedOrBadRequestException(
            uint fileSize,
            string fakeFileName,
            string fileType,
            string accessControlToken,
            Type exceptionType)
        {
            ThrowIf.ArgumentNull(exceptionType, nameof(exceptionType));

            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Add the file to Filestore.
            var storedFile = Helper.FileStore.AddFile(file, _user, useMultiPartMime: true);

            IUser userWithInvalidToken = CreateUserWithInvalidToken(accessControlToken);

            // Execute & Verify: Assert that exception is thrown
            Assert.Throws(exceptionType, () =>
            {
                Helper.FileStore.DeleteFile(storedFile.Guid, userWithInvalidToken);
            }, I18NHelper.FormatInvariant("Did not throw {0} as expected", exceptionType.Name));
        }

        [TestCase]
        [TestRail(98734)]
        [Description("POST a file with invalid multipart mime data.  This test is specifically to get code coverage of the catch block in FilesController.PostFile().")]
        public void PostFile_BadMultiPartMime_InternalServerException()
        {
            // Setup: Create a fake file with a random byte array.
            const uint fileSize = 1024;
            const string fakeFileName = "1KB_File.txt";
            const string fileType = "multipart/form-data; boundary=-----------------------------28947758029299";

            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Execute: Post a file to FileStore with an invalid multipart-mime (i.e. the body doesn't contain multipart data).
            var ex = Assert.Throws<Http500InternalServerErrorException>(() =>
            {
                Helper.FileStore.PostFile(file, _user, useMultiPartMime: false);
            }, "FileStore should return a 500 Internal Server error when we pass an invalid multi-part mime.");

            // Verify: Exception should contain expected message.
            string expectedExceptionMessage = "No multipart boundary found.";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "'{0}' was not found in the exception message: {1}", expectedExceptionMessage, ex.RestResponse.Content);
        }

        [TestCase]
        [TestRail(98735)]
        [Description("PUT a file with invalid multipart mime data.  This test is specifically to get code coverage of the catch block in FilesController.PutFileHttpContext().")]
        public void PutFile_BadMultiPartMime_InternalServerException()
        {
            const uint fileSize = 1024;
            const string fakeFileName = "1KB_File.txt";
            const string postFileType = "text/plain";
            const uint chunkSize = 512;

            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, postFileType);

            byte[] fileBytes = file.Content.ToArray();
            byte[] chunk = fileBytes.Take((int)chunkSize).ToArray();

            // First POST the first chunk with a valid token.
            file.Content = chunk;
            IFile postedFile = Helper.FileStore.PostFile(file, _user);

            byte[] rem = fileBytes.Skip((int)chunkSize).ToArray();
            chunk = rem.Take((int)chunkSize).ToArray();

            postedFile.FileType = "multipart/form-data; boundary=-----------------------------28947758029299";

            // Execute: Put a file chunk to FileStore with invalid multipart-mime (i.e. the body doesn't contain multipart data).
            var ex = Assert.Throws<Http500InternalServerErrorException>(() =>
            {
                Helper.FileStore.PutFile(postedFile, chunk, _user);
            }, "FileStore should return a 500 Internal Server error when we pass an invalid multi-part mime.");

            // Verify: Exception should contain expected message.
            string expectedExceptionMessage = "No multipart boundary found.";
            Assert.That(ex.RestResponse.Content.Contains(expectedExceptionMessage),
                "'{0}' was not found in the exception message: {1}", expectedExceptionMessage, ex.RestResponse.Content);
        }

        [TestCase]
        [TestRail(101572)]
        [Description("POST 2 multipart mime files in one request.  This test is specifically to get code coverage of an if block in MultipartReader.ReadAndExecuteRequestAsync().")]
        public void PostFile_TwoMultiPartMimeFilesInOneRequest_BadRequestException()
        {
            const string contentType = "multipart/form-data; boundary=-----------------------------28947758029299";
            const string requestBody = @"-------------------------------28947758029299
Content-Disposition: form-data; name=""Empty_File.txt""; filename=""Empty_File.txt""
Content-Type: text/plain


-------------------------------28947758029299
Content-Disposition: form-data; name=""Empty_File.txt""; filename=""Empty_File.txt""
Content-Type: text/plain


-------------------------------28947758029299--";
            // Setup: Create a new REST API object
            var restApi = new RestApiFacade(Helper.FileStore.Address, _user.Token.AccessControlToken);

            // Execute & Verify: Assert that a bad request exception occurs when attempting to POST multiple files simultaneously
            Assert.Throws<Http400BadRequestException>(() =>
            {
                restApi.SendRequestBodyAndGetResponse(
                    SVC_FILES_PATH,
                    RestRequestMethod.POST,
                    requestBody,
                    contentType);
            }, "FileStore should return a 400 Bad Request error when we pass multiple files in the same request.");
        }

        [TestCase]
        [TestRail(101585)]
        [Description("POST a corrupt multipart mime request (i.e. with a missing double quote).  This test is specifically to get code coverage of an if block in MultipartReader.ReadAndExecuteRequestAsync().")]
        public void PostFile_CorruptMultiPartMime_BadRequestException()
        {
            const string contentType = "multipart/form-data; boundary=-----------------------------28947758029299";
            // The Content-Disposition line below is missing the "" after the filename, causing it to be invalid.
            const string requestBody = @"-------------------------------28947758029299
Content-Disposition: form-data; name=""Empty_File.txt""; filename=""Empty_File.txt
Content-Type: text/plain

";

            // Setup: Create a new REST API object
            var restApi = new RestApiFacade(Helper.FileStore.Address, _user.Token.AccessControlToken);

            // Execute & Verify: Assert that a bad request exception occurs when attempting to POST a corrupt multipart mime file
            Assert.Throws<Http400BadRequestException>(() =>
            {
                restApi.SendRequestBodyAndGetResponse(
                    SVC_FILES_PATH,
                    RestRequestMethod.POST,
                    requestBody,
                    contentType);
            }, "FileStore should return a 400 Bad Request error when we pass multiple files in the same request.");
        }

        [TestCase]
        [TestRail(101586)]
        [Description("POST a multipart mime request that starts with the end part.  This test is specifically to get code coverage of an if block in MultipartReader.ReadAndExecuteRequestAsync().")]
        public void PostFile_MultiPartMimeBodyStartingWithEndPart_BadRequestException()
        {
            const string contentType = "multipart/form-data; boundary=-----------------------------28947758029299";
            const string requestBody = "-------------------------------28947758029299\r\n"; // The end '\r\n' is important here.

            // Setup: Create a new REST API object
            var restApi = new RestApiFacade(Helper.FileStore.Address, _user.Token.AccessControlToken);

            // Execute & Verify: Assert that a bad request exception occurs when attempting to POST a multipart mime file that starts with the end part
            Assert.Throws<Http400BadRequestException>(() =>
            {
                restApi.SendRequestBodyAndGetResponse(
                    SVC_FILES_PATH,
                    RestRequestMethod.POST,
                    requestBody,
                    contentType);
            }, "FileStore should return a 400 Bad Request error when we pass a multipart mime request that starts with the end part.");
        }

        [TestCase]
        [TestRail(101606)]
        [Description("POST a multipart mime request with no end part and that doesn't end with a CRLF.  This test is specifically to get code coverage of an if block in MultipartPartParser.MultipartPartParser().")]
        public void PostFile_MultiPartMimeNoEndPartAndNoCRLFAfterMimeHeader_InternalServerException()
        {
            const string contentType = "multipart/form-data; boundary=-----------------------------28947758029299";
            const string requestBody = @"-------------------------------28947758029299
Content-Disposition: form-data; name=""Empty_File.txt""; filename=""Empty_File.txt""
Content-Type: text/plain";

            // Setup: Create a new REST API object
            var restApi = new RestApiFacade(Helper.FileStore.Address, _user.Token.AccessControlToken);

            // Execute & Verify: Assert that an internal server error exception occurs when attempting to POST a multipart mime file
            // with no end part and doesn't end with a CRLF
            Assert.Throws<Http500InternalServerErrorException>(() =>
            {
                restApi.SendRequestBodyAndGetResponse(
                    SVC_FILES_PATH,
                    RestRequestMethod.POST,
                    requestBody,
                    contentType);
            }, "FileStore should return a 500 Internal Server error when we pass a multipart mime request with no end part and that doesn't end with a CRLF.");
        }

        [TestCase]
        [TestRail(98738)]
        [Description("PUT a file without Posting it first to get a 404 error.  This test is specifically to get code coverage of the NotFound condition in FilesController.ConstructHttpActionResult().")]
        public void PutFile_WithoutPostingFirst_NotFoundException()
        {
            const uint fileSize = 1;
            const string fakeFileName = "1KB_File.txt";
            const string postFileType = "text/plain";

            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, postFileType);

            // Assign a new default GUID instead of calling POST to get the GUID.
            file.Guid = (new Guid()).ToString();

            // Execute & Verify: Put a file chunk to FileStore with invalid multipart mime (i.e. the body doesn't contain multipart data).
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.FileStore.PutFile(file, file.Content.ToArray(), _user);
            }, "FileStore should return a 404 Not Found error when we PUT a file that doesn't exist.");
        }

        [TestCase((uint)1024, "1KB_File.txt", "")]
        [TestRail(153900)]
        [Description("POST a file with no content type header. Verify that a bad request exception is returned.")]
        public void PostFile_NoContentTypeHeader_BadRequestException(
                uint fileSize,
                string fakeFileName,
                string fileType)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Execute & Verify: Assert that bad request exception is thrown
            Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.FileStore.AddFile(file, _user);
            }, "Did not throw HTTP Status Code 400 (Bad Request) as expected");
        }

        [TestCase((uint)1024, "", "text/plain")]
        [TestRail(153901)]
        [Description("POST a file with no filename in the header. Verify that a bad request exception is returned.")]
        public void PostFile_NoFileName_BadRequestException(
                uint fileSize,
                string fakeFileName,
                string fileType)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Execute & Verify: Assert that bad request exception is thrown
            Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.FileStore.AddFile(file, _user);
            }, "Did not throw HTTP Status Code 400 (Bad Request) as expected");
        }

        /// <summary>
        /// Creates a new user, adds it to Blueprint and sets its token to the invalid token provided.
        /// </summary>
        /// <param name="invalidToken">An invalid token to set for this user.</param>
        /// <returns>The new user with an invalid token.</returns>
        private IUser CreateUserWithInvalidToken(string invalidToken)
        {
            IUser user = Helper.CreateUserAndAddToDatabase();
            user.SetToken(_user.Token.AccessControlToken);  // This is needed to initialize the Token object
            user.Token.AccessControlToken = invalidToken;
            return user;
        }
    }
}
