using System;
using System.Net;
using CustomAttributes;
using Helper;
using Model;
using Model.Factories;
using NUnit.Framework;
using Utilities;
using Utilities.Facades;

namespace AccessControlDoubleTests
{
    [TestFixture]
    [Category(Categories.AccessControlDouble)]
    [Category(Categories.InjectsErrorsIntoAccessControl)]
    [Explicit(IgnoreReasons.DeploymentNotReady)]
    public class ErrorInjectionTests
    {
        private readonly IAccessControl _accessControl = AccessControlFactory.GetAccessControlFromTestConfig();


        [TestCase(HttpStatusCode.BadRequest,            typeof(Http400BadRequestException))]
        [TestCase(HttpStatusCode.Unauthorized,          typeof(Http401UnauthorizedException))]
        [TestCase(HttpStatusCode.Forbidden,             typeof(Http403ForbiddenException))]
        [TestCase(HttpStatusCode.NotFound,              typeof(Http404NotFoundException))]
        [TestCase(HttpStatusCode.MethodNotAllowed,      typeof(Http405MethodNotAllowedException))]
        [TestCase(HttpStatusCode.NotAcceptable,         typeof(Http406NotAcceptableException))]
        [TestCase(HttpStatusCode.Conflict,              typeof(Http409ConflictException))]
        [TestCase(HttpStatusCode.InternalServerError,   typeof(Http500InternalServerErrorException))]
        [TestCase(HttpStatusCode.ServiceUnavailable,    typeof(Http503ServiceUnavailableException))]
        public void GetStatus_ExpectError(HttpStatusCode statusCode, Type expectedException)
        {
            using (var accessControlDoubleHelper = AccessControlDoubleHelper.GetAccessControlDoubleFromTestConfig())
            {
                accessControlDoubleHelper.StartInjectingErrors(RestRequestMethod.GET, statusCode);

                Assert.Throws(expectedException, () => { _accessControl.GetStatus(); },
                    "GetStatus should return a {0} error!", statusCode);
            }
        }
    }
}
