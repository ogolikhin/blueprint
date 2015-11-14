using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using AdminStore.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using System.Net.Http.Formatting;
using AdminStore.Models;
using System.Collections.Generic;
using System.Net.Http.Headers;

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
            ConfigSetting[] settings = { new ConfigSetting { Key = "Key", Value = "Value", Group = "Group" } };
            var configRepo = new Mock<IConfigRepository>();
            var content = new ObjectContent(settings.GetType(), settings, new JsonMediaTypeFormatter());
            var httpClientProvider = new TestHttpClientProvider(request => request.RequestUri.AbsolutePath.EndsWith("settings/false") ?
                new HttpResponseMessage(HttpStatusCode.OK) { Content = content } : null);
            var controller = new ConfigController(configRepo.Object, httpClientProvider);
            controller.Request = new HttpRequestMessage();
            controller.Request.Headers.Add("Session-Token", "");
            controller.Request.SetConfiguration(new HttpConfiguration());

            // Act
            var result = await controller.GetConfigSettings() as ResponseMessageResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("no-store, must-revalidate, no-cache", result.Response.Headers.GetValues("Cache-Control").FirstOrDefault());
            Assert.AreEqual("no-cache", result.Response.Headers.GetValues("Pragma").FirstOrDefault());
            Assert.AreEqual(settings, await result.Response.Content.ReadAsAsync<ConfigSetting[]>());
        }

        [TestMethod]
        public async Task GetConfigSettings_HttpClientThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var configRepo = new Mock<IConfigRepository>();
            var httpClientProvider = new TestHttpClientProvider(request => { throw new Exception(); });
            var controller = new ConfigController(configRepo.Object, httpClientProvider);
            controller.Request = new HttpRequestMessage();
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
            ConfigSetting[] settings = { new ConfigSetting { Key = "Key", Value = "Value", Group = "Group" } };
            IEnumerable<ApplicationLabel> labels = new[] { new ApplicationLabel { Key = "Key", Locale = "en-US", Text = "Text" } };
            var configRepo = new Mock<IConfigRepository>();
            configRepo.Setup(r => r.GetLabels("en-US")).Returns(Task.FromResult(labels)).Verifiable();
            var content = new ObjectContent(settings.GetType(), settings, new JsonMediaTypeFormatter());
            var httpClientProvider = new TestHttpClientProvider(request => request.RequestUri.AbsolutePath.EndsWith("settings/false") ?
                new HttpResponseMessage(HttpStatusCode.OK) { Content = content } : null);
            var controller = new ConfigController(configRepo.Object, httpClientProvider);
            controller.Request = new HttpRequestMessage();
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
            ConfigSetting[] settings = { new ConfigSetting { Key = "Key", Value = "Value", Group = "Group" } };
            IEnumerable<ApplicationLabel> labels = new[] { new ApplicationLabel { Key = "KeyCA", Locale = locale, Text = "TextCA" } };
            var configRepo = new Mock<IConfigRepository>();
            configRepo.Setup(r => r.GetLabels(locale)).Returns(Task.FromResult(labels)).Verifiable();
            var content = new ObjectContent(settings.GetType(), settings, new JsonMediaTypeFormatter());
            var httpClientProvider = new TestHttpClientProvider(request => request.RequestUri.AbsolutePath.EndsWith("settings/false") ?
                new HttpResponseMessage(HttpStatusCode.OK) { Content = content } : null);
            var controller = new ConfigController(configRepo.Object, httpClientProvider);
            controller.Request = new HttpRequestMessage();
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
            var httpClientProvider = new TestHttpClientProvider(request => { throw new Exception(); });
            var controller = new ConfigController(configRepo.Object, httpClientProvider);
            controller.Request = new HttpRequestMessage();
            controller.Request.Headers.Add("Session-Token", "");

            // Act
            IHttpActionResult result = await controller.GetConfig();

            // Assert
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }

        #endregion GetConfig
    }
}
