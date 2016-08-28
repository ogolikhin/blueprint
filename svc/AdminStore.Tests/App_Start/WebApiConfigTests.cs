using System.Linq;
using System.Net.Http;
using System.Web.Http;
using AdminStore.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Attributes;
using ServiceLibrary.Helpers;

namespace AdminStore
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
            config.AssertTotalRoutes(14, "Please update asserts in WebApiConfigTests when changing routes.");
            config.AssertAction<ConfigController>("GetConfigSettings", HttpMethod.Get, "config/settings");
            config.AssertAction<ConfigController>("GetConfig", HttpMethod.Get, "config/config.js");
            config.AssertAction<LicensesController>("GetLicenseTransactions", HttpMethod.Get, "licenses/transactions?days=1");
            config.AssertAction<SessionsController>("PostSession", HttpMethod.Post, "sessions?login=admin");
            config.AssertAction<SessionsController>("PostSession", HttpMethod.Post, "sessions?login=admin&force=true");
            config.AssertAction<SessionsController>("PostSessionSingleSignOn", HttpMethod.Post, "sessions/sso");
            config.AssertAction<SessionsController>("PostSessionSingleSignOn", HttpMethod.Post, "sessions/sso?force=true");
            config.AssertAction<SessionsController>("DeleteSession", HttpMethod.Delete, "sessions");
            config.AssertAction<StatusController>("GetStatus", HttpMethod.Get, "status");
            config.AssertAction<StatusController>("GetStatusUpCheck", HttpMethod.Get, "status/upcheck");
            config.AssertAction<UsersController>("GetLoginUser", HttpMethod.Get, "users/loginuser");
            config.AssertAction<InstanceController>("GetInstanceFolder", HttpMethod.Get, "instance/folders/1");
            config.AssertAction<InstanceController>("GetInstanceFolderChildren", HttpMethod.Get, "instance/folders/1/children");
            config.AssertAction<InstanceController>("GetInstanceProject", HttpMethod.Get, "instance/projects/1");
            config.AssertAction<UsersController>("PostReset", HttpMethod.Post, "users/reset?login=admin");
            config.AssertAction<LogController>("Log", HttpMethod.Post, "log");
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

        [TestMethod]
        public void Register_AllHttpMethods_HaveSessionRequiredOrNoSessionRequiredAttribute()
        {
            // Arrange
            var config = new HttpConfiguration();

            // Act
            WebApiConfig.Register(config);
            config.EnsureInitialized();

            // Assert
            config.AssertMethodAttributes(attr => attr.Any(a => a is SessionAttribute || a is NoSessionRequiredAttribute),
                "{0} is missing SessionAttribute or NoSessionRequiredAttribute.");
        }
    }
}
