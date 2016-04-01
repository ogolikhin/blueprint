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
        [TestInitialize]
        public void Initialize()
        {

        }


        #region Constuctor

        [TestMethod]
        public void Constructor_CreatesDefaultDependencies()
        {
            // Arrange

            // Act
            var controller = new ConfigController();

            // Assert
            Assert.IsInstanceOfType(controller._configRepo, typeof(SqlConfigRepository));
            Assert.IsInstanceOfType(controller._appSettingsRepo, typeof(ApplicationSettingsRepository));
            Assert.IsInstanceOfType(controller._httpClientProvider, typeof(HttpClientProvider));
            Assert.IsInstanceOfType(controller._log, typeof(sl.ServiceLogRepository));
        }

        #endregion

        #region GetConfigSettings

        [TestMethod]
        public async Task GetConfigSettings_HttpClientReturnsContent_ReturnsHeadersAndContent()
        {
            // Arrange
            var settings = new Dictionary<string, Dictionary<string, string>> { { "Group", new Dictionary<string, string> { { "Key", "Value" } } } };
            var controller = CreateController(settings);

            // Act
            var result = await controller.GetConfigSettings() as ResponseMessageResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(settings, await result.Response.Content.ReadAsAsync<Dictionary<string, Dictionary<string, string>>>());
        }

        [TestMethod]
        public async Task GetConfigSettings_HttpClientThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var controller = CreateController();

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
            var settings =  new Dictionary<string, string> { { "Key", "Value" } };
            IEnumerable<ApplicationLabel> labels = new[] { new ApplicationLabel { Key = "Key", Locale = "en-US", Text = "Text" } };
            var controller = CreateController(appSettings:settings, labels: labels);

            // Act
            var result = await controller.GetConfig() as ResponseMessageResult;


            // Assert
            Assert.IsNotNull(result);

            var content = await result.Response.Content.ReadAsStringAsync();

            Assert.AreEqual(content, "window.config={'settings':{'Key':'Value'},'labels':{'Key':'Text'}};");
        }

        [TestMethod]
        public async Task GetConfig_WithLocale_ReturnsHeadersAndContent()
        {
            // Arrange
            string locale = "en-CA";
            var settings = new Dictionary<string, string> { { "Key", "Value" } } ;
            IEnumerable<ApplicationLabel> labels = new[] { new ApplicationLabel { Key = "KeyCA", Locale = locale, Text = "TextCA" } };
            var controller = CreateController(appSettings:settings, labels:labels, locale:locale);
            controller.Request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue(locale));

            // Act
            var result = await controller.GetConfig() as ResponseMessageResult;

            // Assert
            Assert.IsNotNull(result);
            var content = await result.Response.Content.ReadAsStringAsync();
            Assert.AreEqual( @"window.config={'settings':{'Key':'Value'},'labels':{'KeyCA':'TextCA'}};", content);
            
        }

        [TestMethod]
        public async Task GetConfig_HttpClientThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var controller = CreateController();

            // Act
            IHttpActionResult result = await controller.GetConfig();

            // Assert
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }

        [TestMethod]
        public async Task GetConfig_EmptySettingsAndLabels_ReturnsHeadersAndContent()
        {
            // Arrange
            var settings = new Dictionary<string,string>();
            IEnumerable<ApplicationLabel> labels = new ApplicationLabel[] { };
            var controller = CreateController(appSettings:settings, labels:labels);

            // Act
            var result = await controller.GetConfig() as ResponseMessageResult;

            // Assert
            Assert.IsNotNull(result);
            var content = await result.Response.Content.ReadAsStringAsync();
            Assert.AreEqual(content, @"window.config={'settings':{},'labels':{}};");
        }

        #endregion GetConfig

        private static ConfigController CreateController(Dictionary<string, Dictionary<string, string>> globalSettings = null, Dictionary<string, string> appSettings=null, IEnumerable<ApplicationLabel> labels = null, string locale = "en-US")
        {
            var configRepo = new Mock<IConfigRepository>();
            var appSettingsRepo = new Mock<IApplicationSettingsRepository>();
            IHttpClientProvider httpClientProvider;
            if (globalSettings == null)
            {
                httpClientProvider = new TestHttpClientProvider(request => { throw new Exception(); });
            }
            else
            {
                var content = new ObjectContent(globalSettings.GetType(), globalSettings, new JsonMediaTypeFormatter());
                httpClientProvider = new TestHttpClientProvider(request => request.RequestUri.AbsolutePath.EndsWithOrdinal("settings/false") ?
                     new HttpResponseMessage(HttpStatusCode.OK) { Content = content } : null);
            }
            if (appSettings == null)
            {
                appSettingsRepo.Setup(it => it.GetSettings()).Throws(new ApplicationException());
            }
            else
            {
                appSettingsRepo.Setup(it => it.GetSettings())
                    .ReturnsAsync(appSettings.Select(i => new ApplicationSetting {Key = i.Key, Value = i.Value}));
            }

            if (labels != null)
            {
                configRepo.Setup(r => r.GetLabels(It.IsAny<string>())).ReturnsAsync(labels.Select(it => { it.Locale = "en-US"; return it; }));
            }

            var logMock = new Mock<sl.IServiceLogRepository>();
            var controller = new ConfigController(configRepo.Object, appSettingsRepo.Object, httpClientProvider, logMock.Object) { Request = new HttpRequestMessage() };
            controller.Request.Headers.Add("Session-Token", "");
            controller.Request.SetConfiguration(new HttpConfiguration());
            return controller;
        }
    }
}
