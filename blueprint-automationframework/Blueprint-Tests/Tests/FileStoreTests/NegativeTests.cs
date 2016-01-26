using System;
using System.Linq;
using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using NUnit.Framework;
using Utilities;

namespace FileStoreTests
{
    [TestFixture]
    [Category(Categories.Filestore)]
    public class NegativeTests
    {
        private IAdminStore _adminStore;
        private IFileStore _filestore;
        private IUser _user;
        private IUser _userWithInvalidToken;

        [SetUp]
        public void TestSetUp()
        {
            _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            _filestore = FileStoreFactory.GetFileStoreFromTestConfig();
            _user = UserFactory.CreateUserAndAddToDatabase();
            _userWithInvalidToken = UserFactory.CreateUserAndAddToDatabase();

            // Get a valid token for the user.
            ISession session = _adminStore.AddSession(_user.Username, _user.Password);
            _user.SetToken(session.SessionId);
            _userWithInvalidToken.SetToken(session.SessionId);  // This is needed to initialize the Token object, but tests will overwrite the token.

            Assert.IsFalse(string.IsNullOrWhiteSpace(_user.Token.AccessControlToken), "The user didn't get an Access Control token!");
        }

        [TearDown]
        public void TestTearDown()
        {
            if (_filestore != null)
            {
                // Delete all the files that were added.
                foreach (var file in _filestore.Files.ToArray())
                {
                    _filestore.DeleteFile(file.Id, _user);
                }
            }

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

            if (_userWithInvalidToken != null)
            {
                _userWithInvalidToken.DeleteUser(deleteFromDatabase: true);
                _userWithInvalidToken = null;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", typeof(Http401UnauthorizedException))]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "a", typeof(Http401UnauthorizedException))]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "", typeof(Http400BadRequestException))]
        public void PostWithInvalidSessionToken_VerifyUnauthorizedOrBadRequest(
            uint fileSize, 
            string fakeFileName, 
            string fileType, 
            string accessControlToken,
            Type exceptionType)
        {
            ThrowIf.ArgumentNull(exceptionType, nameof(exceptionType));

            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Replace token with invalid token
            _userWithInvalidToken.Token.AccessControlToken = accessControlToken;

            // Assert that exception is thrown
            Assert.Throws(exceptionType, () =>
            {
                _filestore.AddFile(file, _userWithInvalidToken);
            }, I18NHelper.FormatInvariant("Did not throw {0} as expected", exceptionType.Name));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", (uint)512, "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", typeof(Http401UnauthorizedException))]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", (uint)512, "a", typeof(Http401UnauthorizedException))]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", (uint)512, "", typeof(Http400BadRequestException))]
        public void PutWithInvalidSessionToken_VerifyUnauthorizedOrBadRequest(
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
            IFile postedFile = _filestore.PostFile(file, _user);

            byte[] rem = fileBytes.Skip((int)chunkSize).ToArray();
            chunk = rem.Take((int)chunkSize).ToArray();

            // Replace token with invalid token
            _userWithInvalidToken.Token.AccessControlToken = accessControlToken;

            // Assert that exception is thrown for subsequent PUT request with invalid token
            Assert.Throws(exceptionType, () =>
            {
                _filestore.PutFile(postedFile, chunk, _userWithInvalidToken);
            }, I18NHelper.FormatInvariant("Did not throw {0} as expected", exceptionType.Name));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", typeof(Http401UnauthorizedException))]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "a", typeof(Http401UnauthorizedException))]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "", typeof(Http400BadRequestException))]
        public void GetWithInvalidSessionToken_VerifyUnauthorizedOrBadRequest(
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
            var storedFile = _filestore.AddFile(file, _user, useMultiPartMime: true);

            // Replace token with invalid token
            _userWithInvalidToken.Token.AccessControlToken = accessControlToken;

            // Assert that exception is thrown
            // Note: Empty authorization cookie returns 401 Unauthorized
            //       Empty authorization session header returns 400 Bad Request
            Assert.Throws(exceptionType, () =>
            {
                _filestore.GetFile(storedFile.Id, _userWithInvalidToken); 
            }, I18NHelper.FormatInvariant("Did not throw {0} as expected", exceptionType.Name));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", typeof(Http401UnauthorizedException))]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "a", typeof(Http401UnauthorizedException))]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "", typeof(Http400BadRequestException))]
        public void GetHeadWithInvalidSessionToken_VerifyUnauthorizedOrBadRequest(
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
            var storedFile = _filestore.AddFile(file, _user, useMultiPartMime: true);

            // Replace token with invalid token
            _userWithInvalidToken.Token.AccessControlToken = accessControlToken;

            // Assert that exception is thrown
            Assert.Throws(exceptionType, () =>
            {
                _filestore.GetFileMetadata(storedFile.Id, _userWithInvalidToken); 
            }, I18NHelper.FormatInvariant("Did not throw {0} as expected", exceptionType.Name));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", typeof(Http401UnauthorizedException))]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "a", typeof(Http401UnauthorizedException))]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "", typeof(Http400BadRequestException))]
        public void DeleteFileWithInvalidToken_VerifyUnauthorizedOrBadRequest(
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
            var storedFile = _filestore.AddFile(file, _user, useMultiPartMime: true);

            // Replace token with invalid token
            _userWithInvalidToken.Token.AccessControlToken = accessControlToken;

            // Assert that exception is thrown
            Assert.Throws(exceptionType, () =>
            {
                _filestore.DeleteFile(storedFile.Id, _userWithInvalidToken); 
            }, I18NHelper.FormatInvariant("Did not throw {0} as expected", exceptionType.Name));
        }

        [TestCase((uint)1024, "1KB_File.txt", "")]
        public void PostWithNoContentTypeHeader_VerifyBadRequest(
                uint fileSize,
                string fakeFileName,
                string fileType)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Assert that bad request exception is thrown
            Assert.Throws<Http400BadRequestException>(() =>
            {
                _filestore.AddFile(file, _user);
            }, "Did not throw HTTP Status Code 400 (Bad Request) as expected");
        }

        [TestCase((uint)1024, "", "text/plain")]
        public void PostWithNoFileName_VerifyBadRequest(
                uint fileSize,
                string fakeFileName,
                string fileType)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Assert that bad request exception is thrown
            Assert.Throws<Http400BadRequestException>(() =>
            {
                _filestore.AddFile(file, _user);
            }, "Did not throw HTTP Status Code 400 (Bad Request) as expected");
        }
    }
}
