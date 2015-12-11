using System.Net;
using System.Collections.Generic;

using NUnit.Framework;
using CustomAttributes;
using Model.Facades;
using TestConfig;
using Helper.Factories;

namespace AccessControlTests
{
    [TestFixture]
    [Category(Categories.AccessControl)]
    public static class StatusTests
    {
        private const string _serviceRoute = "/svc/accesscontrol/";
        private const string _statusRoute = "status/";
        private static Dictionary<string, Service> _services = TestConfiguration.GetInstance().Services;
        private static string _sessionUrl = _services["AccessControl"].Address + _serviceRoute + _statusRoute;

        [Test]
        [Explicit(IgnoreReasons.ProductBug)]
        public static void GetStatus_OK()
        {
            var response = WebRequestFacade.GetWebResponseFacade(_sessionUrl);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "'GET {0}' should return {1}, but failed with {2}",
                _sessionUrl, HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
