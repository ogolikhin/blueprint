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

namespace AccessControlDoubleTests
{
    /// <summary>
    /// These tests don't test any Blueprint functionality.  They only test whether the AccessControlDouble is functioning properly.
    /// However, because some of the tests need to add sessions into AccessControl, these tests do depend on AccessControl to work correctly.
    /// </summary>
    [TestFixture]
    [Category(Categories.AccessControlDouble)]
    [Category(Categories.InjectsErrorsIntoAccessControl)]
    [Category(Categories.CannotRunInParallel)]
    [Explicit(IgnoreReasons.DeploymentNotReady)]
    public class ErrorInjectionTests
    {
        private readonly IAccessControl _accessControl = AccessControlFactory.GetAccessControlFromTestConfig();

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

        [Test, TestCaseSource(nameof(StatusCodesAndExceptions))]
        public void AddSession_ExpectError(HttpStatusCode statusCode, Type expectedException)
        {
            ISession randomSession = SessionFactory.CreateRandomSession();

            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.POST, statusCode);

                Assert.Throws(expectedException, () => { _accessControl.AddSession(randomSession); },
                    "AddSession should return a {0} error!", statusCode);
            }
        }

        [Test, TestCaseSource(nameof(StatusCodesAndExceptions))]
        public void AuthorizeOperation_ExpectError(HttpStatusCode statusCode, Type expectedException)
        {
            ISession randomSession = SessionFactory.CreateRandomSession();
            ISession createdSession = _accessControl.AddSession(randomSession);

            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.PUT, statusCode);

                Assert.Throws(expectedException, () => { _accessControl.AuthorizeOperation(createdSession); },
                    "AuthorizeOperation should return a {0} error!", statusCode);
            }
        }

        [Test, TestCaseSource(nameof(StatusCodesAndExceptions))]
        public void DeleteSession_ExpectError(HttpStatusCode statusCode, Type expectedException)
        {
            ISession randomSession = SessionFactory.CreateRandomSession();
            ISession createdSession = _accessControl.AddSession(randomSession);

            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.DELETE, statusCode);

                Assert.Throws(expectedException, () => { _accessControl.DeleteSession(createdSession); },
                    "DeleteSession should return a {0} error!", statusCode);
            }
        }

        [Test, TestCaseSource(nameof(StatusCodesAndExceptions))]
        public void GetSession_ExpectError(HttpStatusCode statusCode, Type expectedException)
        {
            ISession randomSession = SessionFactory.CreateRandomSession();
            ISession createdSession = _accessControl.AddSession(randomSession);

            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.GET, statusCode);

                Assert.Throws(expectedException, () => { _accessControl.GetSession(createdSession.UserId); },
                    "GetSession should return a {0} error!", statusCode);
            }
        }

        [Test, TestCaseSource(nameof(StatusCodesAndExceptions))]
        public void GetStatus_ExpectError(HttpStatusCode statusCode, Type expectedException)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.GET, statusCode);

                Assert.Throws(expectedException, () => { _accessControl.GetStatus(); },
                    "GetStatus should return a {0} error!", statusCode);
            }
        }

        [Test, TestCaseSource(nameof(StatusCodes))]
        public static void InjectErrorsForInvalidRequestType_VerifyAccessControlDoubleReturns404(HttpStatusCode statusCode)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                Assert.Throws<Http404NotFoundException>(() =>
                {
                    accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.OPTIONS, statusCode);
                }, "We should get a 404 error for invalid request method: OPTIONS!");
            }
        }
    }
}
