using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using AdminStore.Models;
using AdminStore.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using sl = ServiceLibrary.Repositories.ConfigControl;

namespace AdminStore.Controllers
{
    [TestClass]
    public class ConfigControllerTests
    {
        #region Constuctor

        [TestMethod]
        public void Constructor_CreatesDefaultDependencies()
        {
            // Arrange

            // Act
            var controller = new ConfigController();

            // Assert
            Assert.IsInstanceOfType(controller._configRepo, typeof(SqlConfigRepository));
            Assert.IsInstanceOfType(controller._httpClientProvider, typeof(HttpClientProvider));
        }

        #endregion

        #region GetConfigSettings

        [TestMethod]
        public async Task GetConfigSettings_HttpClientReturnsContent_ReturnsHeadersAndContent()
        {
            // Arrange
            var settings = new Dictionary<string, Dictionary<string, string>>();
            settings.Add("Group", new Dictionary<string, string>());
            settings["Group"].Add("Key", "Value");
            var configRepo = new Mock<IConfigRepository>();
            var logMock = new Mock<sl.IServiceLogRepository>();
            var content = new ObjectContent(settings.GetType(), settings, new JsonMediaTypeFormatter());
            var httpClientProvider = new TestHttpClientProvider(request => request.RequestUri.AbsolutePath.EndsWith("settings/false") ?
                 new HttpResponseMessage(HttpStatusCode.OK) { Content = content } : null);
            var controller = new ConfigController(configRepo.Object, httpClientProvider, logMock.Object) { Request = new HttpRequestMessage() };
            controller.Request.Headers.Add("Session-Token", "");
            controller.Request.SetConfiguration(new HttpConfiguration());

            // Act
            var result = await controller.GetConfigSettings() as ResponseMessageResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("no-store, must-revalidate, no-cache", result.Response.Headers.GetValues("Cache-Control").FirstOrDefault());
            Assert.AreEqual("no-cache", result.Response.Headers.GetValues("Pragma").FirstOrDefault());
            Assert.AreEqual(settings, await result.Response.Content.ReadAsAsync<Dictionary<string, Dictionary<string, string>>>());
        }

        [TestMethod]
        public async Task GetConfigSettings_HttpClientThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var configRepo = new Mock<IConfigRepository>();
            var logMock = new Mock<sl.IServiceLogRepository>();
            var httpClientProvider = new TestHttpClientProvider(request => { throw new Exception(); });
            var controller = new ConfigController(configRepo.Object, httpClientProvider, logMock.Object) { Request = new HttpRequestMessage() };
            controller.Request.Headers.Add("Session-Token", "");

            // Act
            IHttpActionResult result = await controller.GetConfigSettings();

            // Assert
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }

        #endregion GetConfigSettings

        #region GetConfig

        [TestMethod]
        public async Task GetConfig_NoLocale_ReturnsHeadersAndContent()
        {
            // Arrange
            var settings = new Dictionary<string, Dictionary<string, string>>();
            settings.Add("Group", new Dictionary<string, string>());
            settings["Group"].Add("Key", "Value");
            IEnumerable<ApplicationLabel> labels = new[] { new ApplicationLabel { Key = "Key", Locale = "en-US", Text = "Text" } };
            var configRepo = new Mock<IConfigRepository>();
            var logMock = new Mock<sl.IServiceLogRepository>();
            configRepo.Setup(r => r.GetLabels("en-US")).ReturnsAsync(labels).Verifiable();
            var content = new ObjectContent(settings.GetType(), settings, new JsonMediaTypeFormatter());
            var httpClientProvider = new TestHttpClientProvider(request => request.RequestUri.AbsolutePath.EndsWith("settings/false") ?
                 new HttpResponseMessage(HttpStatusCode.OK) { Content = content } : null);
            var controller = new ConfigController(configRepo.Object, httpClientProvider, logMock.Object) { Request = new HttpRequestMessage() };
            controller.Request.Headers.Add("Session-Token", "");
            controller.Request.SetConfiguration(new HttpConfiguration());

            // Act
            var result = await controller.GetConfig() as ResponseMessageResult;

            // Assert
            configRepo.Verify();
            Assert.IsNotNull(result);
            Assert.AreEqual("no-store, must-revalidate, no-cache", result.Response.Headers.GetValues("Cache-Control").FirstOrDefault());
            Assert.AreEqual("no-cache", result.Response.Headers.GetValues("Pragma").FirstOrDefault());
            Assert.AreEqual(@"window.config = { settings: {'Key':{'Value', 'Group'}}, labels: {'Key':'Text'} };console.log('Configuration for locale en-US loaded successfully.');",
                 await result.Response.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task GetConfig_WithLocale_ReturnsHeadersAndContent()
        {
            // Arrange
            string locale = "en-CA";
            var settings = new Dictionary<string, Dictionary<string, string>>();
            settings.Add("Group", new Dictionary<string, string>());
            settings["Group"].Add("Key", "Value");
            IEnumerable<ApplicationLabel> labels = new[] { new ApplicationLabel { Key = "KeyCA", Locale = locale, Text = "TextCA" } };
            var configRepo = new Mock<IConfigRepository>();
            var logMock = new Mock<sl.IServiceLogRepository>();
            configRepo.Setup(r => r.GetLabels(locale)).ReturnsAsync(labels).Verifiable();
            var content = new ObjectContent(settings.GetType(), settings, new JsonMediaTypeFormatter());
            var httpClientProvider = new TestHttpClientProvider(request => request.RequestUri.AbsolutePath.EndsWith("settings/false") ?
                 new HttpResponseMessage(HttpStatusCode.OK) { Content = content } : null);
            var controller = new ConfigController(configRepo.Object, httpClientProvider, logMock.Object) { Request = new HttpRequestMessage() };
            controller.Request.Headers.Add("Session-Token", "");
            controller.Request.SetConfiguration(new HttpConfiguration());
            controller.Request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue(locale));

            // Act
            var result = await controller.GetConfig() as ResponseMessageResult;

            // Assert
            configRepo.Verify();
            Assert.IsNotNull(result);
            Assert.AreEqual("no-store, must-revalidate, no-cache", result.Response.Headers.GetValues("Cache-Control").FirstOrDefault());
            Assert.AreEqual("no-cache", result.Response.Headers.GetValues("Pragma").FirstOrDefault());
            Assert.AreEqual(@"window.config = { settings: {'Key':{'Value', 'Group'}}, labels: {'KeyCA':'TextCA'} };console.log('Configuration for locale " + locale + @" loaded successfully.');",
                 await result.Response.Content.ReadAsStringAsync());
        }

        [TestMethod]
        public async Task GetConfig_HttpClientThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var configRepo = new Mock<IConfigRepository>();
            var logMock = new Mock<sl.IServiceLogRepository>();
            var httpClientProvider = new TestHttpClientProvider(request => { throw new Exception(); });
            var controller = new ConfigController(configRepo.Object, httpClientProvider, logMock.Object) { Request = new HttpRequestMessage() };
            controller.Request.Headers.Add("Session-Token", "");

            // Act
            IHttpActionResult result = await controller.GetConfig();

            // Assert
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }

        #endregion GetConfig
    }
}
