﻿using System.Linq;
using CustomAttributes;
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

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "")]
        public void PostWithInvalidSessionToken_VerifyUnauthorized(
            uint fileSize, 
            string fakeFileName, 
            string fileType, 
            string accessControlToken)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelpers.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Replace token with invalid token
            _userWithInvalidToken.Token.AccessControlToken = accessControlToken;

            // Assert that unauthorized exception is thrown
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                _filestore.AddFile(file, _userWithInvalidToken);
            }, "Did not throw HTTP Status Code 401 (Unauthorized Exception) as expected");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", (uint)512, "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", (uint)512, "")]
        public void PutWithInvalidSessionToken_VerifyUnauthorized(
            uint fileSize, 
            string fakeFileName, 
            string fileType, 
            uint chunkSize, 
            string accessControlToken)
        {
            Assert.That((chunkSize > 0) && (fileSize > chunkSize), "Invalid TestCase detected!  chunkSize must be > 0 and < fileSize.");

            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelpers.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            byte[] fileBytes = file.Content;
            byte[] chunk = fileBytes.Take((int)chunkSize).ToArray();

            // First POST the first chunk with a valid token.
            file.Content = chunk;
            IFile postedFile = _filestore.PostFile(file, _user);

            byte[] rem = fileBytes.Skip((int)chunkSize).ToArray();
            chunk = rem.Take((int)chunkSize).ToArray();

            // Replace token with invalid token
            _userWithInvalidToken.Token.AccessControlToken = accessControlToken;

            // Assert that unauthorized exception is thrown for subsequent PUT request with invalid token
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                _filestore.PutFile(postedFile, chunk, _userWithInvalidToken);
            }, "Did not throw HTTP Status Code 401 (Unauthorized Exception) as expected");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "")]
        public void GetWithInvalidSessionToken_VerifyUnauthorized(
            uint fileSize, 
            string fakeFileName, 
            string fileType, 
            string accessControlToken)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelpers.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Add the file to Filestore.
            var storedFile = _filestore.AddFile(file, _user, useMultiPartMime: true);

            // Replace token with invalid token
            _userWithInvalidToken.Token.AccessControlToken = accessControlToken;

            // Assert that unauthorized exception is thrown
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                _filestore.GetFile(storedFile.Id, _userWithInvalidToken); 
            }, "Did not throw HTTP Status Code 401 (Unauthorized Exception) as expected");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "")]
        public void GetHeadWithInvalidSessionToken_VerifyUnauthorized(
            uint fileSize, 
            string fakeFileName, 
            string fileType, 
            string accessControlToken)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelpers.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Add the file to Filestore.
            var storedFile = _filestore.AddFile(file, _user, useMultiPartMime: true);

            // Replace token with invalid token
            _userWithInvalidToken.Token.AccessControlToken = accessControlToken;

            // Assert that unauthorized exception is thrown
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                _filestore.GetFileMetadata(storedFile.Id, _userWithInvalidToken); 
            }, "Did not throw HTTP Status Code 401 (Unauthorized Exception) as expected");
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "")]
        public void DeleteFileWithInvalidToken_VerifyUnauthorized(
            uint fileSize, 
            string fakeFileName, 
            string fileType, 
            string accessControlToken)
        {
            // Setup: create a fake file with a random byte array.
            IFile file = FileStoreTestHelpers.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);

            // Add the file to Filestore.
            var storedFile = _filestore.AddFile(file, _user, useMultiPartMime: true);

            // Replace token with invalid token
            _userWithInvalidToken.Token.AccessControlToken = accessControlToken;

            // Assert that unauthorized exception is thrown
            Assert.Throws<Http401UnauthorizedException>(() =>
            {
                _filestore.DeleteFile(storedFile.Id, _userWithInvalidToken); 
            }, "Did not throw HTTP Status Code 401 (Unauthorized Exception) as expected");
        }
    }
}
