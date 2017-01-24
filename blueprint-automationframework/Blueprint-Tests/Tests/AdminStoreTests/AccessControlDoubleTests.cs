using System;
using System.Net;
using CustomAttributes;
using Helper;
using Model;
using NUnit.Framework;
using TestCommon;
using Utilities;
using Utilities.Facades;
using Utilities.Factories;

namespace AdminStoreTests
{
    [TestFixture]
    [Category(Categories.AdminStore)]
    [Category(Categories.InjectsErrorsIntoAccessControl)]
    [Category(Categories.CannotRunInParallel)]
    public class AccessControlDoubleTests : TestBase
    {
        private IUser _user;

        #region Setup and Cleanup

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAddToDatabase();
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region Success tests

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107437)]
        [Description("Configure the AccessControlDouble to return error HTTP Status Codes for: DELETE, GET, HEAD & PUT requests.  " +
            "POST a new session to AdminStore.  Verify it returns 200 OK.")]
        public void PostSession_AccessControlErrorAllMethodsExceptPOST_ExpectSuccess(HttpStatusCode accessControlError)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.DELETE, accessControlError);
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.GET, accessControlError);
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.HEAD, accessControlError);
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.PUT, accessControlError);

                Assert.DoesNotThrow(() => { Helper.AdminStore.AddSession(_user.Username, _user.Password); },
                    "AddSession should NOT return an error if AccessControl returns a {0} error!", accessControlError);
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107438)]
        [Description("Configure the AccessControlDouble to return error HTTP Status Codes for: GET, HEAD & POST requests.  " +
            "Try to delete an existing session from AdminStore.  Verify it returns 200 OK.")]
        public void DeleteSession_AccessControlErrorAllMethodsExceptDELETEandPUT_ExpectSuccess(HttpStatusCode accessControlError)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                var session = Helper.AdminStore.AddSession(_user.Username, _user.Password);

                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.GET, accessControlError);
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.HEAD, accessControlError);
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.POST, accessControlError);

                Assert.DoesNotThrow(() => { Helper.AdminStore.DeleteSession(session); },
                    "DeleteSession should NOT return an error if AccessControl returns a {0} error!", accessControlError);
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107439)]
        [Description("Configure the AccessControlDouble to return error HTTP Status Codes for: DELETE, GET, HEAD & POST requests.  " +
            "Try to get an existing session from AdminStore.  Verify it returns 200 OK.")]
        [Ignore(IgnoreReasons.UnderDevelopmentDev)]    // AdminStore.GetSession() isn't implemented!!
        public void GetSession_AccessControlErrorAllMethodsExceptPUT_ExpectSuccess(HttpStatusCode accessControlError)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                var session = Helper.AdminStore.AddSession(_user.Username, _user.Password);

                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.DELETE, accessControlError);
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.GET, accessControlError);
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.HEAD, accessControlError);
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.POST, accessControlError);

                Assert.DoesNotThrow(() => { Helper.AdminStore.GetSession(session.UserId); },
                    "GetSession should NOT return an error if AccessControl returns a {0} error!", accessControlError);
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107440)]
        [Description("Configure the AccessControlDouble to return error HTTP Status Codes for: DELETE, GET, HEAD, POST & PUT requests.  " +
            "Try to get the config.js file from AdminStore.  Verify it returns 200 OK.")]
        public void GetConfigJs_AccessControlErrorAllMethods_ExpectSuccess(HttpStatusCode accessControlError)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                var session = Helper.AdminStore.AddSession(_user.Username, _user.Password);

                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.DELETE, accessControlError);
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.GET, accessControlError);
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.HEAD, accessControlError);
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.POST, accessControlError);
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.PUT, accessControlError);

                Assert.DoesNotThrow(() => { Helper.AdminStore.GetConfigJs(session); },
                    "GetConfigJs should NOT return an error if AccessControl returns a {0} error!", accessControlError);
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107441)]
        [Description("Configure the AccessControlDouble to return error HTTP Status Codes for: DELETE, GET, HEAD & POST requests.  " +
            "Try to get the Login User for a valid session token.  Verify it returns 200 OK.")]
        public void GetLoginUser_AccessControlErrorAllMethodsExceptPUT_ExpectSuccess(HttpStatusCode accessControlError)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                var session = Helper.AdminStore.AddSession(_user.Username, _user.Password);

                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.DELETE, accessControlError);
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.GET, accessControlError);
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.HEAD, accessControlError);
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.POST, accessControlError);

                Assert.DoesNotThrow(() => { Helper.AdminStore.GetLoginUser(session.SessionId); },
                    "GetLoginUser should NOT return an error if AccessControl returns a {0} error!", accessControlError);
            }
        }

        #endregion Success tests

        #region Error tests

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107442)]
        [Description("Configure the AccessControlDouble to return error HTTP Status Codes for: POST requests.  " +
            "Try to POST a new session to AdminStore.  Verify it returns a 500 Internal Server error.")]
        public void PostSession_AccessControlErrorPOST_Expect500Error(HttpStatusCode accessControlError)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.POST, accessControlError);

                Assert.Throws<Http500InternalServerErrorException>(() => { Helper.AdminStore.AddSession(_user.Username, _user.Password); },   // XXX: Why 500 error instead of 401??
                    "AddSession should return a 500 error if AccessControl returns a {0} error for POST requests!", accessControlError);
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107443)]
        [Description("Configure the AccessControlDouble to return error HTTP Status Codes for: DELETE requests.  " +
            "Try to delete an existing session from AdminStore.  Verify it returns the same error code that the AccessControlDouble is set to return.")]
        public void DeleteSession_AccessControlErrorDELETE_ExpectError(HttpStatusCode accessControlError)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                var session = Helper.AdminStore.AddSession(_user.Username, _user.Password);

                // Get the expected exception type that matches the HttpStatusCode error we're injecting.
                Type expectedException = WebExceptionFactory.Create((int)accessControlError, accessControlError.ToString()).GetType();

                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.DELETE, accessControlError);

                Assert.Throws(expectedException, () => { Helper.AdminStore.DeleteSession(session); },
                    "DeleteSession should return a {0} error if AccessControl returns a {0} error for DELETE requests!", accessControlError);   // XXX: Why does this just re-throw the same error as AccessControl?
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107444)]
        [Description("Configure the AccessControlDouble to return error HTTP Status Codes for: PUT requests.  " +
            "Try to delete an existing session from AdminStore.  Verify it returns a 401 Unauthorized error.")]
        public void DeleteSession_AccessControlErrorPUT_Expect401Error(HttpStatusCode accessControlError)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                var session = Helper.AdminStore.AddSession(_user.Username, _user.Password);

                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.PUT, accessControlError);

                Assert.Throws<Http401UnauthorizedException>(() => { Helper.AdminStore.DeleteSession(session); },
                    "DeleteSession should return a 401 error if AccessControl returns a {0} error for PUT requests!", accessControlError);
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107445)]
        [Description("Configure the AccessControlDouble to return error HTTP Status Codes for: PUT requests.  " +
            "Try to get a session token for the specified User ID.  Verify it returns a 401 Unauthorized error.")]
        [Ignore(IgnoreReasons.UnderDevelopmentDev)]    // AdminStore.GetSession() isn't implemented!!
        public void GetSession_AccessControlErrorPUT_Expect401Error(HttpStatusCode accessControlError)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                var session = Helper.AdminStore.AddSession(_user.Username, _user.Password);

                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.PUT, accessControlError);

                Assert.Throws<Http401UnauthorizedException>(() => { Helper.AdminStore.GetSession(session.UserId); },
                    "GetSession should return a 401 error if AccessControl returns a {0} error for PUT requests!", accessControlError);
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107446)]
        [Description("Configure the AccessControlDouble to return error HTTP Status Codes for: DELETE requests.  " +
            "Try to get license transactions from AdminStore.  Verify it returns a 401 Unauthorized error.")]
        public void GetLicenseTransactions_AccessControlErrorDELETE_Expect401Error(HttpStatusCode accessControlError)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                Helper.AdminStore.AddSession(_user.Username, _user.Password);

                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.DELETE, accessControlError);   // XXX: Why does GET /license/transactions make a DELETE request to AccessControl???

                Assert.Throws<Http401UnauthorizedException>(() => { Helper.AdminStore.GetLicenseTransactions(_user, numberOfDays: 5); },
                    "GetLicenseTransactions should return a 401 error if AccessControl returns a {0} error for DELETE requests!", accessControlError);
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107447)]
        [Description("Configure the AccessControlDouble to return error HTTP Status Codes for: GET requests.  " +
            "Try to get license transactions from AdminStore.  Verify it returns a 401 Unauthorized error.")]
        public void GetLicenseTransactions_AccessControlErrorGET_Expect401Error(HttpStatusCode accessControlError)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                Helper.AdminStore.AddSession(_user.Username, _user.Password);

                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.GET, accessControlError);

                Assert.Throws<Http401UnauthorizedException>(() => { Helper.AdminStore.GetLicenseTransactions(_user, numberOfDays: 5); },
                    "GetLicenseTransactions should return a 401 error if AccessControl returns a {0} error for GET requests!", accessControlError);
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107448)]
        [Description("Configure the AccessControlDouble to return error HTTP Status Codes for: HEAD requests.  " +
            "Try to get license transactions from AdminStore.  Verify it returns a 401 Unauthorized error.")]
        public void GetLicenseTransactions_AccessControlErrorHEAD_Expect401Error(HttpStatusCode accessControlError)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                Helper.AdminStore.AddSession(_user.Username, _user.Password);

                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.HEAD, accessControlError);

                Assert.Throws<Http401UnauthorizedException>(() => { Helper.AdminStore.GetLicenseTransactions(_user, numberOfDays: 5); },
                    "GetLicenseTransactions should return a 401 error if AccessControl returns a {0} error for HEAD requests!", accessControlError);
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107449)]
        [Description("Configure the AccessControlDouble to return error HTTP Status Codes for: POST requests.  " +
            "Try to get license transactions from AdminStore.  Verify it returns a 401 Unauthorized error.")]
        public void GetLicenseTransactions_AccessControlErrorPOST_Expect401Error(HttpStatusCode accessControlError)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                Helper.AdminStore.AddSession(_user.Username, _user.Password);

                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.POST, accessControlError);

                Assert.Throws<Http401UnauthorizedException>(() => { Helper.AdminStore.GetLicenseTransactions(_user, numberOfDays: 5); },
                    "GetLicenseTransactions should return a 401 error if AccessControl returns a {0} error for POST requests!", accessControlError);
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107450)]
        [Description("Configure the AccessControlDouble to return error HTTP Status Codes for: PUT requests.  " +
            "Try to get license transactions from AdminStore.  Verify it returns a 401 Unauthorized error.")]
        public void GetLicenseTransactions_AccessControlErrorPUT_Expect401Error(HttpStatusCode accessControlError)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                Helper.AdminStore.AddSession(_user.Username, _user.Password);

                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.PUT, accessControlError);

                Assert.Throws<Http401UnauthorizedException>(() => { Helper.AdminStore.GetLicenseTransactions(_user, numberOfDays: 5); },
                    "GetLicenseTransactions should return a 401 error if AccessControl returns a {0} error for PUT requests!", accessControlError);
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107451)]
        [Description("Configure the AccessControlDouble to return error HTTP Status Codes for: PUT requests.  " +
            "Try to get the Login User for an existing session token.  Verify it returns a 401 Unauthorized error.")]
        public void GetLoginUser_AccessControlErrorPUT_Expect401Error(HttpStatusCode accessControlError)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                var session = Helper.AdminStore.AddSession(_user.Username, _user.Password);

                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.PUT, accessControlError);

                Assert.Throws<Http401UnauthorizedException>(() => { Helper.AdminStore.GetLoginUser(session.SessionId); },
                    "GetLoginUser should return a 401 error if AccessControl returns a {0} error for PUT requests!", accessControlError);
            }
        }

        #endregion Error tests
    }
}
