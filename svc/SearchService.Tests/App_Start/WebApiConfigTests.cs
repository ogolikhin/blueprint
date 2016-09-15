using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchService.Controllers;
using ServiceLibrary.Attributes;
using ServiceLibrary.Helpers;

namespace SearchService
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
            //config.AssertTotalRoutes(7, "Please update asserts in WebApiConfigTests when changing routes.");
            //config.AssertAction<FilesController>("GetFileHead", HttpMethod.Head, "files/1");
            //config.AssertAction<FilesController>("GetFileContent", HttpMethod.Get, "files/1");
            //config.AssertAction<FilesController>("PostFile", HttpMethod.Post, "files");
            //config.AssertAction<FilesController>("PostFile", HttpMethod.Post, "files?expired=2016-01-01T00:00:00");
            //config.AssertAction<FilesController>("PutFile", HttpMethod.Put, "files/1");
            //config.AssertAction<FilesController>("DeleteFile", HttpMethod.Delete, "files/1");
            //config.AssertAction<FilesController>("DeleteFile", HttpMethod.Delete, "files/1?expired=2016-01-01T00:00:00");
            config.AssertAction<StatusController>("GetStatus", HttpMethod.Get, "status");
            config.AssertAction<StatusController>("GetStatusUpCheck", HttpMethod.Get, "status/upcheck");
        }

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
            config.AssertMethodAttributes(attr => attr.Any(a => a is SessionRequiredAttribute || a is NoSessionRequiredAttribute),
                "{0} is missing SessionRequiredAttribute or NoSessionRequiredAttribute.");
        }
    }
}
