using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
        private readonly object[] StatusCodesAndExceptions =
        {
            new object[] {HttpStatusCode.BadRequest,             typeof(Http400BadRequestException)},
            new object[] {HttpStatusCode.Unauthorized,           typeof(Http401UnauthorizedException)},
            new object[] {HttpStatusCode.Forbidden,              typeof(Http403ForbiddenException)},
            new object[] {HttpStatusCode.NotFound,               typeof(Http404NotFoundException)},
            new object[] {HttpStatusCode.MethodNotAllowed,       typeof(Http405MethodNotAllowedException)},
            new object[] {HttpStatusCode.NotAcceptable,          typeof(Http406NotAcceptableException)},
            new object[] {HttpStatusCode.Conflict,               typeof(Http409ConflictException)},
            new object[] {HttpStatusCode.InternalServerError,    typeof(Http500InternalServerErrorException)},
            new object[] {HttpStatusCode.ServiceUnavailable,     typeof(Http503ServiceUnavailableException)}
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

        [Test, TestCaseSource(nameof(StatusCodesAndExceptions))]
        public void PostFile_AccessControlError_ExpectException(
            HttpStatusCode accessControlError,
            Type expectedException)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                IFile file = CreateFileWithRandomByteArray();
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.POST, accessControlError);

                Assert.Throws(expectedException, () => { _filestore.PostFile(file, _user); },
                    "PostFile should return a {0} error!", accessControlError);
            }
        }

        [Test, TestCaseSource(nameof(StatusCodesAndExceptions))]
        public void PutFile_AccessControlError_ExpectException(
            HttpStatusCode accessControlError,
            Type expectedException)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                IFile file = CreateAndAddFile();
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.PUT, accessControlError);

                Assert.Throws(expectedException, () => { _filestore.PutFile(file, file.Content.ToArray(), _user); },
                    "PutFile should return a {0} error!", accessControlError);
            }
        }

        [Test, TestCaseSource(nameof(StatusCodesAndExceptions))]
        public void GetFile_AccessControlError_ExpectException(
            HttpStatusCode accessControlError,
            Type expectedException)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                IFile file = CreateAndAddFile();
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.GET, accessControlError);

                Assert.Throws(expectedException, () => { _filestore.GetFile(file.Id, _user); },
                    "GetFile should return a {0} error!", accessControlError);
            }
        }

        [Test, TestCaseSource(nameof(StatusCodesAndExceptions))]
        public void DeleteFile_AccessControlError_ExpectException(
            HttpStatusCode accessControlError,
            Type expectedException)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                IFile file = CreateAndAddFile();
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.DELETE, accessControlError);

                Assert.Throws(expectedException, () => { _filestore.DeleteFile(file.Id, _user); },
                    "DeleteFile should return a {0} error!", accessControlError);
            }
        }

        [Test, TestCaseSource(nameof(StatusCodesAndExceptions))]
        public void GetFileMetadata_AccessControlError_ExpectException(
            HttpStatusCode accessControlError,
            Type expectedException)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                IFile file = CreateAndAddFile();
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.HEAD, accessControlError);

                Assert.Throws(expectedException, () => { _filestore.GetFileMetadata(file.Id, _user); },
                    "GetFileMetadata should return a {0} error!", accessControlError);
            }
        }

    }
}
