using System.Linq;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using NUnit.Framework;
using Utilities;

namespace FileStoreTests
{
    [TestFixture]
    [Category(Categories.FileStore)]
    public class CookiePositiveTests
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
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        public void PostWithValidCookieSessionToken_VerifyBadRequest(
            uint fileSize, 
            string fakeFileName, 
            string fileType)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Assert that bad request exception is thrown
            Assert.Throws<Http400BadRequestException>(() =>
            {
                _filestore.AddFile(file, _user, sendAuthorizationAsCookie: true);
            }, "HTTP Status Code 400 (Bad Request) was expected because POST does not support authorization cookies!");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", (uint)512)]
        public void PutWithValidCookieSessionToken_VerifyBadRequest(
            uint fileSize, 
            string fakeFileName, 
            string fileType, 
            uint chunkSize)
        {
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

            // Assert that bad request exception is thrown for subsequent PUT request with invalid token
            Assert.Throws<Http400BadRequestException>(() =>
            {
                _filestore.PutFile(postedFile, chunk, _user, sendAuthorizationAsCookie: true);
            }, "HTTP Status Code 400 (Bad Request) was expected because PUT does not support authorization cookies!");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        public void GetWithValidCookieSessionToken_VerifyAuthorized(
            uint fileSize, 
            string fakeFileName, 
            string fileType)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Add the file to Filestore.
            var storedFile = _filestore.AddFile(file, _user, useMultiPartMime: true);

            // Assert that an exception is not thrown
            Assert.DoesNotThrow(() =>
            {
                _filestore.GetFile(storedFile.Id, _user, sendAuthorizationAsCookie: true); 
            }, "GET supports authentication cookies but threw an exception!");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        public void GetHeadWithValidCookieSessionToken_VerifyBadRequest(
            uint fileSize, 
            string fakeFileName, 
            string fileType)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Add the file to Filestore.
            var storedFile = _filestore.AddFile(file, _user, useMultiPartMime: true);

            // Assert that bad request exception is thrown
            Assert.Throws<Http400BadRequestException>(() =>
            {
                _filestore.GetFileMetadata(storedFile.Id, _user, sendAuthorizationAsCookie: true);
            }, "HTTP Status Code 400 (Bad Request) was expected because HEAD does not support authorization cookies!");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain")]
        public void DeleteFileWithValidCookieToken_VerifyBadRequest(
            uint fileSize, 
            string fakeFileName, 
            string fileType)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Add the file to Filestore.
            var storedFile = _filestore.AddFile(file, _user);

            // Assert that bad request exception is thrown
            Assert.Throws<Http400BadRequestException>(() =>
            {
                _filestore.DeleteFile(storedFile.Id, _user, sendAuthorizationAsCookie: true);
            }, "HTTP Status Code 400 (Bad Request) was expected because DELETE does not support authorization cookies!");
        }
    }
}
