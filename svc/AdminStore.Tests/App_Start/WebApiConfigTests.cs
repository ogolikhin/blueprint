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
            config.AssertTotalRoutes(28, "Please update asserts in WebApiConfigTests when changing routes.");
            config.AssertAction<ConfigController>("GetConfigSettings", HttpMethod.Get, "config/settings");
            config.AssertAction<ConfigController>("GetConfig", HttpMethod.Get, "config/config.js");
            config.AssertAction<LicensesController>("GetLicenseTransactions", HttpMethod.Get, "licenses/transactions?days=1");
            config.AssertAction<LogController>("Log", HttpMethod.Post, "log");
            config.AssertAction<SessionsController>("PostSession", HttpMethod.Post, "sessions?login=admin");
            config.AssertAction<SessionsController>("PostSession", HttpMethod.Post, "sessions?login=admin&force=true");
            config.AssertAction<SessionsController>("PostSessionSingleSignOn", HttpMethod.Post, "sessions/sso");
            config.AssertAction<SessionsController>("PostSessionSingleSignOn", HttpMethod.Post, "sessions/sso?force=true");
            config.AssertAction<SessionsController>("DeleteSession", HttpMethod.Delete, "sessions");
            config.AssertAction<SessionsController>("IsSessionAlive", HttpMethod.Get, "sessions/alive");
            config.AssertAction<StatusController>("GetStatus", HttpMethod.Get, "status");
            config.AssertAction<StatusController>("GetStatusUpCheck", HttpMethod.Get, "status/upcheck");
            config.AssertAction<InstanceController>("GetInstanceFolder", HttpMethod.Get, "instance/folders/1");
            config.AssertAction<InstanceController>("GetInstanceFolderChildren", HttpMethod.Get, "instance/folders/1/children");
            config.AssertAction<InstanceController>("GetInstanceProject", HttpMethod.Get, "instance/projects/1");
            config.AssertAction<InstanceController>("GetProjectNavigationPath", HttpMethod.Get, "instance/projects/1/navigationPath");
            config.AssertAction<InstanceController>("GetInstanceRoles", HttpMethod.Get, "instance/roles");
            config.AssertAction<JobsController>("GetLatestJobs", HttpMethod.Get, "jobs/");
            config.AssertAction<JobsController>("GetJob", HttpMethod.Get, "jobs/1");
            config.AssertAction<JobsController>("GetJobResultFile", HttpMethod.Get, "jobs/1/result/file");
            config.AssertAction<JobsController>("QueueGenerateProcessTestsJob", HttpMethod.Post, "jobs/process/testgen");
            config.AssertAction<UsersController>("GetLoginUser", HttpMethod.Get, "users/loginuser");
            config.AssertAction<UsersController>("GetUserIcon", HttpMethod.Get, "users/1/icon");
            config.AssertAction<UsersController>("PostRequestPasswordResetAsync", HttpMethod.Post, "users/passwordrecovery/request");
            config.AssertAction<UsersController>("PostPasswordResetAsync", HttpMethod.Post, "users/passwordrecovery/reset");
            config.AssertAction<UsersController>("GetUser", HttpMethod.Get, "users/1");
            config.AssertAction<UsersController>("GetAllUsers", HttpMethod.Get, "users?page=1&pageSize=20&filter=blueprint&sort=source,-license");
            config.AssertAction<UsersController>("PostUser", HttpMethod.Post, "users");
            config.AssertAction<UsersController>("UpdateUser", HttpMethod.Put, "users/1");
            config.AssertAction<UsersController>("PostReset", HttpMethod.Post, "users/reset?login=admin");
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
