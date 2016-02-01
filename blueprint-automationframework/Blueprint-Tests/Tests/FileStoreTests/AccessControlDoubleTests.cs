using System;
using System.Linq;
using System.Net;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using NUnit.Framework;
using Utilities;
using Utilities.Facades;

namespace FileStoreTests
{
    [TestFixture]
    [Category(Categories.Filestore)]
    [Category(Categories.InjectsErrorsIntoAccessControl)]
    [Explicit(IgnoreReasons.DeploymentNotReady)]
    public class AccessControlDoubleTests
    {
        private IAdminStore _adminStore;
        private IFileStore _filestore;
        private IUser _user;

        #region TestCaseSource data

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]   // It is used through reflection.
        private readonly object[] StatusCodes =
        {
            new object[] {HttpStatusCode.BadRequest},
            new object[] {HttpStatusCode.Unauthorized},
            new object[] {HttpStatusCode.Forbidden},
            new object[] {HttpStatusCode.NotFound},
            new object[] {HttpStatusCode.MethodNotAllowed},
            new object[] {HttpStatusCode.NotAcceptable},
            new object[] {HttpStatusCode.Conflict},
            new object[] {HttpStatusCode.InternalServerError},
            new object[] {HttpStatusCode.ServiceUnavailable}
        };

        #endregion TestCaseSource data

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            _filestore = FileStoreFactory.GetFileStoreFromTestConfig();
            _user = UserFactory.CreateUserAndAddToDatabase();

            // Get a valid token for the user.
            ISession session = _adminStore.AddSession(_user.Username, _user.Password);
            _user.SetToken(session.SessionId);

            Assert.IsFalse(string.IsNullOrWhiteSpace(_user.Token.AccessControlToken), "The user didn't get an Access Control token!");
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
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

        #endregion Setup and Cleanup

        #region Private helper functions

        /// <summary>
        /// Creates a randon file.
        /// </summary>
        /// <returns>A random file.</returns>
        private static IFile CreateFileWithRandomByteArray()
        {
            return FileStoreTestHelper.CreateFileWithRandomByteArray(1024, "1KB_File.txt", "text/plain");
        }

        /// <summary>
        /// Creates a random file and adds it to FileStore.
        /// </summary>
        /// <returns>The file we added to FileStore.</returns>
        private IFile CreateAndAddFile()
        {
            return FileStoreTestHelper.CreateAndAddFile(1024, "1KB_File.txt", "text/plain", _filestore, _user);
        }

        #endregion Private helper functions

        [Test, TestCaseSource(nameof(StatusCodes))]
        public void PostFile_AccessControlError_ExpectException(HttpStatusCode accessControlError)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                IFile file = CreateFileWithRandomByteArray();
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.POST, accessControlError);

                Assert.DoesNotThrow(() => { _filestore.PostFile(file, _user); },
                    "PostFile should not check AccessControl and therefore should not throw an exception!");
            }
        }

        [Test, TestCaseSource(nameof(StatusCodes))]
        public void PutFile_AccessControlError_ExpectException(HttpStatusCode accessControlError)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                IFile file = CreateAndAddFile();
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.PUT, accessControlError);

                Assert.Throws< Http401UnauthorizedException>(() => { _filestore.PutFile(file, file.Content.ToArray(), _user); },
                    "PutFile should return a {0} error!", accessControlError);
            }
        }

        [Test, TestCaseSource(nameof(StatusCodes))]
        public void GetFile_AccessControlError_ExpectException(HttpStatusCode accessControlError)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                IFile file = CreateAndAddFile();
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.GET, accessControlError);

                Assert.DoesNotThrow(() => { _filestore.GetFile(file.Id, _user); },
                    "GetFile should not check AccessControl and therefore should not throw an exception!");
            }
        }

        [Test, TestCaseSource(nameof(StatusCodes))]
        public void DeleteFile_AccessControlError_ExpectException(HttpStatusCode accessControlError)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                IFile file = CreateAndAddFile();
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.DELETE, accessControlError);

                Assert.DoesNotThrow(() => { _filestore.DeleteFile(file.Id, _user); },
                    "DeleteFile should not check AccessControl and therefore should not throw an exception!");
            }
        }

        [Test, TestCaseSource(nameof(StatusCodes))]
        public void GetFileMetadata_AccessControlError_ExpectException(HttpStatusCode accessControlError)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                IFile file = CreateAndAddFile();
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.HEAD, accessControlError);

                Assert.DoesNotThrow(() => { _filestore.GetFileMetadata(file.Id, _user); },
                    "GetFileMetadata should not check AccessControl and therefore should not throw an exception!");
            }
        }

    }
}
