using System;
using System.Net;

using NUnit.Framework;
using Model;
using System.Collections.Generic;
using System.Collections.Specialized;
using Logging;
using Helper;
using Model.Facades;

namespace AccessControlTests
{
    public static class StatusTests
    {
        private static string _serverUrl = "http://localhost:9801/";//TODO: replace with TestConfiguration.GetInstance();
        private static string _serviceRoute = "svc/accesscontrol/";
        private static string _statusRoute = "status/";
        private static readonly string _sessionUrl = _serverUrl + _serviceRoute + _statusRoute;
        [Test]
        [Ignore("we don't have deployed access control service")]
        public static void GetStatus_OK()
        {
            var response = WebRequestFacade.GetWebResponseFacade(_sessionUrl);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, "'GET {0}' should return {1}, but failed with {2}",
                _sessionUrl, HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
