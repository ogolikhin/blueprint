using System.Linq;
using System.Net.Http;
using System.Web.Http;
using ConfigControl.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Attributes;
using ServiceLibrary.Helpers;

namespace ConfigControl
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
            config.AssertTotalRoutes(7, "Please update asserts in WebApiConfigTests when changing routes.");
            config.AssertAction<ConfigController>("GetConfig", HttpMethod.Get, "settings/true");
            config.AssertAction<LogController>("Log", HttpMethod.Post, "log");
            config.AssertAction<LogController>("Log", HttpMethod.Post, "log/CLog");
            config.AssertAction<LogController>("Log", HttpMethod.Post, "log/StandardLog");
            config.AssertAction<LogController>("Log", HttpMethod.Post, "log/PerformanceLog");
            config.AssertAction<LogController>("Log", HttpMethod.Post, "log/SQLTraceLog");
            config.AssertAction<StatusController>("GetStatus", HttpMethod.Get, "status");
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
