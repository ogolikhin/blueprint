using System.Collections.Generic;
using System.Linq;
using System.Net;
using CustomAttributes;
using Model;
using Model.Factories;
using NUnit.Framework;
using TestConfig;
using Utilities;
using Utilities.Facades;

namespace FileStoreTests
{
    [TestFixture]
    [Category(Categories.Filestore)]
    public class NegativeTests
    {
        private IAdminStore _adminStore;
        private IFileStore _filestore;
        private IUser _user;

        [SetUp]
        public void TestSetUp()
        {
            _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            _filestore = FileStoreFactory.GetFileStoreFromTestConfig();
            _user = UserFactory.CreateUserAndAddToDatabase();

            // Get a valid token for the user.
            ISession session = _adminStore.AddSession(_user.Username, _user.Password);
            _user.SetToken(session.SessionId);

            Assert.IsFalse(string.IsNullOrWhiteSpace(_user.Token.AccessControlToken), "The user didn't get an Access Control token!");
        }

        [TearDown]
        public void TestTearDown()
        {
            if (_adminStore != null)
            {
                // Delete all the sessions that were created.
                foreach (var session in _adminStore.Sessions.ToArray())
                {
                    _adminStore.DeleteSession(session);
                }
            }

            if (_user != null)
            {
                _user.DeleteUser(deleteFromDatabase: true);
                _user = null;
            }
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "")]
        public void PostWithInvalidSessionToken_VerifyUnauthorized(uint fileSize, string fakeFileName, string fileType, string accessControlToken)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelpers.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Replace token with invalid token
            _user.Token.AccessControlToken = accessControlToken;

            // Assert that unauthorized exception is thrown
            Assert.Throws<Http401UnauthorizedException>(() => { _filestore.AddFile(file, _user, useMultiPartMime: true); }, "Did not throw HTTP Status Code 401 (Unauthorized Exception) as expected");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", (uint)512, "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", (uint)512, "")]
        public void PutWithInvalidSessionToken_VerifyUnauthorized(uint fileSize, string fakeFileName, string fileType, uint chunkSize, string accessControlToken)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelpers.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            var queryParameters = new Dictionary<string, string>();
            var additionalHeaders = new Dictionary<string, string>();

            additionalHeaders.Add("Content-Type", file.FileType);
            additionalHeaders.Add("Content-Disposition", string.Format("form-data; name ={0}; filename={1}", "attachment", file.FileName));

            byte[] fileBytes = file.Content;
            byte[] chunk = fileBytes;

            if (chunkSize > 0 && fileBytes.Length > chunkSize)
            {
                chunk = fileBytes.Take((int)chunkSize).ToArray();
            }

            TestConfiguration testConfig = TestConfiguration.GetInstance();
            string address = testConfig.Services["FileStore"].Address;
            var restApi = new RestApiFacade(address, _user.Username, _user.Password, _user.Token?.AccessControlToken);

            var expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            var response = restApi.SendRequestAndGetResponse("svc/filestore/files", RestRequestMethod.POST, file.FileName, chunk, file.FileType, useMultiPartMime: true, additionalHeaders: additionalHeaders, queryParameters:queryParameters, expectedStatusCodes: expectedStatusCodes);

            file.Id = response.Content.Replace("\"", "");

            byte[] rem = fileBytes.Skip((int)chunkSize).ToArray();

            string path = string.Format("svc/filestore/files/{0}", file.Id);

            chunk = rem.Take((int)chunkSize).ToArray();

            // Replace token with invalid token
            _user.Token.AccessControlToken = accessControlToken;

            // Create new rest api instance to allow for a changed session token
            restApi = new RestApiFacade(address, _user.Username, _user.Password, _user.Token?.AccessControlToken);

            // Assert that unauthorized exception is thrown
            Assert.Throws<Http401UnauthorizedException>(() => {
                restApi.SendRequestAndGetResponse(path, RestRequestMethod.PUT, file.FileName, chunk,
                file.FileType, useMultiPartMime: true, additionalHeaders: additionalHeaders, queryParameters: queryParameters);
                }, "Did not throw HTTP Status Code 401 (Unauthorized Exception) as expected");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "")]
        public void GetWithInvalidSessionToken_VerifyUnauthorized(uint fileSize, string fakeFileName, string fileType, string accessControlToken)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelpers.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Add the file to Filestore.
            var expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            var storedFile = _filestore.AddFile(file, _user, useMultiPartMime: true, expectedStatusCodes: expectedStatusCodes);

            // Replace token with invalid token
            _user.Token.AccessControlToken = accessControlToken;

            // Assert that unauthorized exception is thrown
            Assert.Throws<Http401UnauthorizedException>(() => { _filestore.GetFile(storedFile.Id, _user); }, "Did not throw HTTP Status Code 401 (Unauthorized Exception) as expected");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "")]
        public void GetHeadWithInvalidSessionToken_VerifyUnauthorized(uint fileSize, string fakeFileName, string fileType, string accessControlToken)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelpers.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Add the file to Filestore.
            var expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            var storedFile = _filestore.AddFile(file, _user, useMultiPartMime: true, expectedStatusCodes: expectedStatusCodes);

            // Replace token with invalid token
            _user.Token.AccessControlToken = accessControlToken;

            // Assert that unauthorized exception is thrown
            Assert.Throws<Http401UnauthorizedException>(() => { _filestore.GetFileMetadata(storedFile.Id, _user); }, "Did not throw HTTP Status Code 401 (Unauthorized Exception) as expected");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "")]
        public void DeleteFileWithInvalidToken_VerifyUnauthorized(uint fileSize, string fakeFileName, string fileType, string accessControlToken)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelpers.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Add the file to Filestore.
            var expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            var storedFile = _filestore.AddFile(file, _user, useMultiPartMime: true, expectedStatusCodes: expectedStatusCodes);

            // Replace token with invalid token
            _user.Token.AccessControlToken = accessControlToken;

            // Assert that unauthorized exception is thrown
            Assert.Throws<Http401UnauthorizedException>(() => { _filestore.DeleteFile(storedFile.Id, _user); }, "Did not throw HTTP Status Code 401 (Unauthorized Exception) as expected");
        }
    }
}
