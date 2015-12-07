using System.Net;

using NUnit.Framework;
using CustomAttributes;
using Model.Facades;

namespace AccessControlTests
{
    [TestFixture]
    [Category(Categories.AccessControl)]
    public static class StatusTests
    {
        private static string _serverUrl = "http://localhost:9801/";//TODO: replace with TestConfiguration.GetInstance();
        private static string _serviceRoute = "svc/accesscontrol/";
        private static string _statusRoute = "status/";
        private static readonly string _sessionUrl = _serverUrl + _serviceRoute + _statusRoute;

        [Test]
        [Ignore(IgnoreReasons.DeploymentNotReady)]
        public static void GetStatus_OK()
        {
            var response = WebRequestFacade.GetWebResponseFacade(_sessionUrl);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "'GET {0}' should return {1}, but failed with {2}",
                _sessionUrl, HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
