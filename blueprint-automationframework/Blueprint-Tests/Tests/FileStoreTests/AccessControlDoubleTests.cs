using System.Linq;
using System.Net;
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
    [Category(Categories.InjectsErrorsIntoAccessControl)]
    [Category(Categories.CannotRunInParallel)]
    public class AccessControlDoubleTests : TestBase
    {
        private IUser _user;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            Helper?.Dispose();
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
            return FileStoreTestHelper.CreateAndAddFile(1024, "1KB_File.txt", "text/plain", Helper.FileStore, _user);
        }

        #endregion Private helper functions

        #region Success tests

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107472)]
        [Description("Configure the AccessControlDouble to return error HTTP Status Codes for: DELETE, GET, HEAD & POST requests.  " +
            "POST a file to FileStore.  Verify it returns 201 Created.")]
        public void PostFile_AccessControlErrorAllMethodsExceptPUT_ExpectSuccess(HttpStatusCode accessControlError)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                IFile file = CreateFileWithRandomByteArray();

                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.DELETE, accessControlError);
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.GET, accessControlError);
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.HEAD, accessControlError);
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.POST, accessControlError);

                Assert.DoesNotThrow(() => { Helper.FileStore.PostFile(file, _user); },
                    "PostFile should NOT call DELETE, GET, HEAD or POST on AccessControl so it shouldn't fail if AccessControl returns a {0} error for any of those methods!", accessControlError);
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107473)]
        [Description("POST the first chunk of a file to FileStore.  Configure the AccessControlDouble to return error HTTP Status Codes for: DELETE, GET, HEAD & POST requests.  " +
            "PUT a chunk to the file in FileStore.  Verify it returns 200 OK.")]
        public void PutFile_AccessControlErrorAllMethodsExceptPUT_ExpectSuccess(HttpStatusCode accessControlError)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                IFile file = CreateAndAddFile();

                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.DELETE, accessControlError);
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.GET, accessControlError);
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.HEAD, accessControlError);
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.POST, accessControlError);

                Assert.DoesNotThrow(() => { Helper.FileStore.PutFile(file, file.Content.ToArray(), _user); },
                    "PutFile should NOT call DELETE, GET, HEAD or POST on AccessControl so it shouldn't fail if AccessControl returns a {0} error for any of those methods!", accessControlError);
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107474)]
        [Description("POST a file to FileStore.  Configure the AccessControlDouble to return error HTTP Status Codes for: DELETE, GET, HEAD & POST requests.  " +
            "GET the file from FileStore.  Verify it returns 200 OK.")]
        public void GetFile_AccessControlErrorAllMethodsExceptPUT_ExpectSuccess(HttpStatusCode accessControlError)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                IFile file = CreateAndAddFile();

                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.DELETE, accessControlError);
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.GET, accessControlError);
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.HEAD, accessControlError);
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.POST, accessControlError);

                Assert.DoesNotThrow(() => { Helper.FileStore.GetFile(file.Guid, _user); },
                    "GetFile should NOT call DELETE, GET, HEAD or POST on AccessControl so it shouldn't fail if AccessControl returns a {0} error for any of those methods!", accessControlError);
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107475)]
        [Description("POST a file to FileStore.  Configure the AccessControlDouble to return error HTTP Status Codes for: DELETE, GET, HEAD & POST requests.  " +
            "DELETE the file from FileStore.  Verify it returns 200 OK.")]
        public void DeleteFile_AccessControlErrorAllMethodsExceptPUT_ExpectSuccess(HttpStatusCode accessControlError)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                IFile file = CreateAndAddFile();

                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.DELETE, accessControlError);
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.GET, accessControlError);
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.HEAD, accessControlError);
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.POST, accessControlError);

                Assert.DoesNotThrow(() => { Helper.FileStore.DeleteFile(file.Guid, _user); },
                    "DeleteFile should NOT call DELETE, GET, HEAD or POST on AccessControl so it shouldn't fail if AccessControl returns a {0} error for any of those methods!", accessControlError);
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107476)]
        [Description("POST a file to FileStore.  Configure the AccessControlDouble to return error HTTP Status Codes for: DELETE, GET, HEAD & POST requests.  " +
            "Get the metadata (HEAD) of the file from FileStore.  Verify it returns 200 OK.")]
        public void GetFileMetadata_AccessControlErrorAllMethodsExceptPUT_ExpectSuccess(HttpStatusCode accessControlError)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                IFile file = CreateAndAddFile();

                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.DELETE, accessControlError);
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.GET, accessControlError);
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.HEAD, accessControlError);
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.POST, accessControlError);

                Assert.DoesNotThrow(() => { Helper.FileStore.GetFileMetadata(file.Guid, _user); },
                    "GetFileMetadata should NOT call DELETE, GET, HEAD or POST on AccessControl so it shouldn't fail if AccessControl returns a {0} error for any of those methods!", accessControlError);
            }
        }

        #endregion Success tests

        #region Error tests

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107477)]
        [Description("Configure the AccessControlDouble to return error HTTP Status Codes for: PUT requests.  " +
            "Try to POST a file to FileStore.  Verify it returns 401 Unauthorized.")]
        public void PostFile_AccessControlErrorPUT_Expect401Error(HttpStatusCode accessControlError)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                IFile file = CreateFileWithRandomByteArray();

                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.PUT, accessControlError);

                Assert.Throws<Http401UnauthorizedException>(() => { Helper.FileStore.PostFile(file, _user); },
                    "PostFile should return a 401 error if AccessControl returns a {0} error for PUT requests!", accessControlError);
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107478)]
        [Description("POST the first chunk of a file to FileStore.  Configure the AccessControlDouble to return error HTTP Status Codes for: PUT requests.  " +
            "Try to PUT a chunk to the file on FileStore.  Verify it returns 401 Unauthorized.")]
        public void PutFile_AccessControlErrorPUT_Expect401Error(HttpStatusCode accessControlError)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                IFile file = CreateAndAddFile();

                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.PUT, accessControlError);

                Assert.Throws< Http401UnauthorizedException>(() => { Helper.FileStore.PutFile(file, file.Content.ToArray(), _user); },
                    "PutFile should return a 401 error if AccessControl returns a {0} error for PUT requests!", accessControlError);
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107479)]
        [Description("POST a file to FileStore.  Configure the AccessControlDouble to return error HTTP Status Codes for: PUT requests.  " +
            "Try to GET the file from FileStore.  Verify it returns 401 Unauthorized.")]
        public void GetFile_AccessControlErrorPUT_Expect401Error(HttpStatusCode accessControlError)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                IFile file = CreateAndAddFile();

                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.PUT, accessControlError);

                Assert.Throws<Http401UnauthorizedException>(() => { Helper.FileStore.GetFile(file.Guid, _user); },
                    "GetFile should return a 401 error if AccessControl returns a {0} error for PUT requests!", accessControlError);
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107480)]
        [Description("POST a file to FileStore.  Configure the AccessControlDouble to return error HTTP Status Codes for: PUT requests.  " +
            "Try to DELETE the file on FileStore.  Verify it returns 401 Unauthorized.")]
        public void DeleteFile_AccessControlErrorPUT_Expect401Error(HttpStatusCode accessControlError)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                IFile file = CreateAndAddFile();

                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.PUT, accessControlError);

                Assert.Throws<Http401UnauthorizedException>(() => { Helper.FileStore.DeleteFile(file.Guid, _user); },
                    "DeleteFile should return a 401 error if AccessControl returns a {0} error for PUT requests!", accessControlError);
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107481)]
        [Description("POST a file to FileStore.  Configure the AccessControlDouble to return error HTTP Status Codes for: PUT requests.  " +
            "Try to get the file metadata (HEAD) from FileStore.  Verify it returns 401 Unauthorized.")]
        public void GetFileMetadata_AccessControlErrorPUT_Expect401Error(HttpStatusCode accessControlError)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                IFile file = CreateAndAddFile();

                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.PUT, accessControlError);

                Assert.Throws<Http401UnauthorizedException>(() => { Helper.FileStore.GetFileMetadata(file.Guid, _user); },
                    "GetFileMetadata should return a 401 error if AccessControl returns a {0} error for PUT requests!", accessControlError);
            }
        }

        #endregion Error tests
    }
}
