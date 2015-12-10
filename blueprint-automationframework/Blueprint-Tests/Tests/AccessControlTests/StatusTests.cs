using System.Net;

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
        private static TestConfiguration _testConfig = TestConfiguration.GetInstance();
        private static string _sessionUrl = BlueprintServerFactory.GetBlueprintServerFromTestConfig().Address + _serviceRoute + _statusRoute;

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
