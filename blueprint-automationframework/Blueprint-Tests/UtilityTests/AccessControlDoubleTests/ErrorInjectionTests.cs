using System;
using System.Net;
using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using NUnit.Framework;
using Utilities;
using Utilities.Facades;
using Utilities.Factories;

namespace AccessControlDoubleTests
{
    /// <summary>
    /// These tests don't test any Blueprint functionality.  They only test whether the AccessControlDouble is functioning properly.
    /// However, because some of the tests need to add sessions into AccessControl, these tests do depend on AccessControl to work correctly.
    /// </summary>
    [TestFixture]
    [Category(Categories.AccessControlDouble)]
    [Category(Categories.CannotRunInParallel)]
    [Category(Categories.InjectsErrorsIntoAccessControl)]
    [Category(Categories.UtilityTest)]
    public class ErrorInjectionTests
    {
        private readonly IAccessControl _accessControl = AccessControlFactory.GetAccessControlFromTestConfig();

        [TearDown]
        public void TearDown()
        {
            Logger.WriteTrace("TearDown() is deleting all sessions created by the tests...");

            // Delete all sessions created by the tests.
            foreach (var session in _accessControl.Sessions.ToArray())
            {
                _accessControl.DeleteSession(session);
            }
        }

        #region StartInjectingErrors tests

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107486)]
        [Description("Start Injecting Errors for POST requests.  Try to add a session and verify the injected error is returned.")]
        public void StartInjectingErrorsForPOST_AddSession_ExpectError(HttpStatusCode statusCode)
        {
            ISession randomSession = SessionFactory.CreateRandomSession();

            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                // Get the expected exception type that matches the HttpStatusCode error we're injecting.
                Type expectedException = WebExceptionFactory.Create((int)statusCode, statusCode.ToString()).GetType();

                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.POST, statusCode);

                Assert.Throws(expectedException, () => { _accessControl.AddSession(randomSession); },
                    "AddSession should return a {0} error!", statusCode);
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107487)]
        [Description("Add a session.  Start Injecting Errors for PUT requests.  Try to authorize a session and verify the injected error is returned.")]
        public void StartInjectingErrorsForPUT_AuthorizeOperation_ExpectError(HttpStatusCode statusCode)
        {
            ISession randomSession = SessionFactory.CreateRandomSession();
            ISession createdSession = _accessControl.AddSession(randomSession);

            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                // Get the expected exception type that matches the HttpStatusCode error we're injecting.
                Type expectedException = WebExceptionFactory.Create((int)statusCode, statusCode.ToString()).GetType();

                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.PUT, statusCode);

                Assert.Throws(expectedException, () => { _accessControl.AuthorizeOperation(createdSession); },
                    "AuthorizeOperation should return a {0} error!", statusCode);
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107488)]
        [Description("Add a session.  Start Injecting Errors for DELETE requests.  Try to delete a session and verify the injected error is returned.")]
        public void StartInjectingErrorsForDELETE_DeleteSession_ExpectError(HttpStatusCode statusCode)
        {
            ISession randomSession = SessionFactory.CreateRandomSession();
            ISession createdSession = _accessControl.AddSession(randomSession);

            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                // Get the expected exception type that matches the HttpStatusCode error we're injecting.
                Type expectedException = WebExceptionFactory.Create((int)statusCode, statusCode.ToString()).GetType();

                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.DELETE, statusCode);

                Assert.Throws(expectedException, () => { _accessControl.DeleteSession(createdSession); },
                    "DeleteSession should return a {0} error!", statusCode);
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107489)]
        [Description("Add a session.  Start Injecting Errors for GET requests.  Try to get a session and verify the injected error is returned.")]
        public void StartInjectingErrorsForGET_GetSession_ExpectError(HttpStatusCode statusCode)
        {
            ISession randomSession = SessionFactory.CreateRandomSession();
            ISession createdSession = _accessControl.AddSession(randomSession);

            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                // Get the expected exception type that matches the HttpStatusCode error we're injecting.
                Type expectedException = WebExceptionFactory.Create((int)statusCode, statusCode.ToString()).GetType();

                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.GET, statusCode);

                Assert.Throws(expectedException, () => { _accessControl.GetSession(createdSession.UserId); },
                    "GetSession should return a {0} error!", statusCode);
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107490)]
        [Description("Start Injecting Errors for GET requests.  Try to get /status and verify the injected error is returned.")]
        public void StartInjectingErrorsForGET_GetStatus_ExpectError(HttpStatusCode statusCode)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                // Get the expected exception type that matches the HttpStatusCode error we're injecting.
                Type expectedException = WebExceptionFactory.Create((int)statusCode, statusCode.ToString()).GetType();

                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.GET, statusCode);

                Assert.Throws(expectedException, () => { _accessControl.GetStatus(); },
                    "GetStatus should return a {0} error!", statusCode);
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107491)]
        [Description("Try to start Injecting Errors for OPTIONS requests.  Verify a 404 error is returned.")]
        public static void StartInjectingErrorsForInvalidRequestType_VerifyAccessControlDoubleReturns404(HttpStatusCode statusCode)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                Assert.Throws<Http404NotFoundException>(() =>
                {
                    accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.OPTIONS, statusCode);
                }, "We should get a 404 error for invalid request method: OPTIONS!");
            }
        }

        #endregion StartInjectingErrors tests

        #region StopInjectingErrors tests

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107492)]
        [Description("Start Injecting Errors for POST requests, then Stop Injecting Errors.  Try to add a session and verify it's successful.")]
        public void StopInjectingErrorsForPOST_AddSession_ExpectSuccess(HttpStatusCode statusCode)
        {
            ISession randomSession = SessionFactory.CreateRandomSession();

            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.POST, statusCode);
                accessControlDoubleHelper.StopInjectingErrors(RestRequestMethod.POST);

                Assert.DoesNotThrow(() => { _accessControl.AddSession(randomSession); },
                    "AddSession should not return an error after we've stopped injecting errors from AccessControlDouble!");
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107493)]
        [Description("Add a session.  Start Injecting Errors for PUT requests, then Stop Injecting Errors.  Try to authorize a session and verify it's successful.")]
        public void StopInjectingErrorsForPUT_AuthorizeOperation_ExpectSuccess(HttpStatusCode statusCode)
        {
            ISession randomSession = SessionFactory.CreateRandomSession();
            ISession createdSession = _accessControl.AddSession(randomSession);

            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.PUT, statusCode);
                accessControlDoubleHelper.StopInjectingErrors(RestRequestMethod.PUT);

                Assert.DoesNotThrow(() => { _accessControl.AuthorizeOperation(createdSession); },
                    "GetSession should not return an error after we've stopped injecting errors from AccessControlDouble!");
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107494)]
        [Description("Add a session.  Start Injecting Errors for DELETE requests, then Stop Injecting Errors.  Try to delete a session and verify it's successful.")]
        public void StopInjectingErrorsForDELETE_DeleteSession_ExpectSuccess(HttpStatusCode statusCode)
        {
            ISession randomSession = SessionFactory.CreateRandomSession();
            ISession createdSession = _accessControl.AddSession(randomSession);

            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.DELETE, statusCode);
                accessControlDoubleHelper.StopInjectingErrors(RestRequestMethod.DELETE);

                Assert.DoesNotThrow(() => { _accessControl.DeleteSession(createdSession); },
                    "GetSession should not return an error after we've stopped injecting errors from AccessControlDouble!");
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107495)]
        [Description("Add a session.  Start Injecting Errors for GET requests, then Stop Injecting Errors.  Try to get a session and verify it's successful.")]
        public void StopInjectingErrorsForGET_GetSession_ExpectSuccess(HttpStatusCode statusCode)
        {
            ISession randomSession = SessionFactory.CreateRandomSession();
            ISession createdSession = _accessControl.AddSession(randomSession);

            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.GET, statusCode);
                accessControlDoubleHelper.StopInjectingErrors(RestRequestMethod.GET);

                Assert.DoesNotThrow(() => { _accessControl.GetSession(createdSession.UserId); },
                    "GetSession should not return an error after we've stopped injecting errors from AccessControlDouble!");
            }
        }

        [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllHttpErrorStatusCodes))]
        [TestRail(107496)]
        [Description("Start Injecting Errors for GET requests, then Stop Injecting Errors.  Try to get /status and verify it's successful.")]
        public void StopInjectingErrorsForGET_GetStatus_ExpectSuccess(HttpStatusCode statusCode)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.GET, statusCode);
                accessControlDoubleHelper.StopInjectingErrors(RestRequestMethod.GET);

                Assert.DoesNotThrow(() => { _accessControl.GetStatus(); },
                    "GetStatus should not return an error after we've stopped injecting errors from AccessControlDouble!");
            }
        }

        [TestCase]
        [TestRail(107497)]
        [Description("Try to Stop Injecting Errors for OPTIONS requests.  Verify a 404 error is returned.")]
        public static void StopInjectingErrorsForInvalidRequestType_VerifyAccessControlDoubleReturns404()
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                Assert.Throws<Http404NotFoundException>(() =>
                {
                    accessControlDoubleHelper.StopInjectingErrors(RestRequestMethod.OPTIONS);
                }, "We should get a 404 error for invalid request method: OPTIONS!");
            }
        }

        #endregion StopInjectingErrors tests
    }
}
