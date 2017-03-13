using System.Linq;
using System.Net.Http;
using System.Web.Http;
using AccessControl.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Attributes;
using ServiceLibrary.Helpers;

namespace AccessControl
{
    [TestClass]
    public class WebApiConfigTests
    {
        [TestMethod]
        public void Register_Always_RegistersCorrectRoutes()
        {
            // Arrange
            var config = new HttpConfiguration();

            // Act
            WebApiConfig.Register(config);
            config.EnsureInitialized();

            // Assert
            config.AssertTotalRoutes(12, "Please update asserts in WebApiConfigTests when changing routes.");
            config.AssertAction<LicensesController>("GetActiveLicenses", HttpMethod.Get, "licenses/active");
            config.AssertAction<LicensesController>("GetLockedLicenses", HttpMethod.Get, "licenses/locked");
            config.AssertAction<LicensesController>("GetLicenseTransactions", HttpMethod.Get, "licenses/transactions?days=1&consumerType=1");
            config.AssertAction<LicensesController>("GetLicenseUsage", HttpMethod.Get, "licenses/usage?month=10&year=2016");
            config.AssertAction<SessionsController>("GetSession", HttpMethod.Get, "sessions/1");
            config.AssertAction<SessionsController>("SelectSessions", HttpMethod.Get, "sessions/select");
            config.AssertAction<SessionsController>("SelectSessions", HttpMethod.Get, "sessions/select?ps=100&pn=1");
            config.AssertAction<SessionsController>("PostSession", HttpMethod.Post, "sessions/1?userName=admin&licenseLevel=1");
            config.AssertAction<SessionsController>("PostSession", HttpMethod.Post, "sessions/1?userName=admin&licenseLevel=1&isSso=true");
            config.AssertAction<SessionsController>("PutSession", HttpMethod.Put, "sessions");
            config.AssertAction<SessionsController>("PutSession", HttpMethod.Put, "sessions?op=op&aid=1");
            config.AssertAction<SessionsController>("DeleteSession", HttpMethod.Delete, "sessions");
            config.AssertAction<SessionsController>("DeleteSession", HttpMethod.Delete, "sessions/1");
            config.AssertAction<StatusController>("GetStatus", HttpMethod.Get, "status");
            config.AssertAction<StatusController>("GetStatusUpCheck", HttpMethod.Get, "status/upcheck");
        }

        [TestMethod]
        public void Register_GetAndHeadMethods_HaveNoCacheAttribute()
        {
            // Arrange
            var config = new HttpConfiguration();

            // Act
            WebApiConfig.Register(config);
            config.EnsureInitialized();

            // Assert
            config.AssertMethodAttributes(attr => attr.Any(a => a is HttpGetAttribute || a is HttpHeadAttribute) == attr.Any(a => a is NoCacheAttribute),
                "{0} is missing NoCacheAttribute.");
        }
    }
}
