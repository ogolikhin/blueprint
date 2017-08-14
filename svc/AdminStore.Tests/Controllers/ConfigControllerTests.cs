using AdminStore.Models;
using AdminStore.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;
using ServiceLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace AdminStore.Controllers
{
    [TestClass]
    public class ConfigControllerTests
    {
        #region Constuctor

        [TestMethod]
        public void Constructor_CorrectlyInitializesLogSource()
        {
            // Arrange, Act
            var controller = new ConfigController();

            // Assert
            Assert.AreEqual(controller.LogSource, WebApiConfig.LogSourceConfig);
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

        #endregion GetConfigSettings

        #region GetApplicationSettings

        [TestMethod]
        public async Task GetApplicationSettings_HasAccess_ReturnsApplicationSettings()
        {
            // Arrange
            var settings = new Dictionary<string, string> { { "Key", "Value" } };
            var controller = CreateController(appSettings: settings);

            // Act
            var result = await controller.GetApplicationSettings() as ResponseMessageResult;

            // Assert
            Assert.IsNotNull(result);
            var content = (ObjectContent<Dictionary<string, object>>)result.Response.Content;
            var actualSettings = (Dictionary<string, object>)content.Value;
            Assert.IsTrue(actualSettings.ContainsKey("Key"), "Cannot find 'Key' setting");
            Assert.AreEqual("Value", actualSettings["Key"]);     
        }

        [TestMethod]
        public async Task GetApplicationSettings_ApplicationSettingsContainsFeatures()
        {
            // Arrange
            var settings = new Dictionary<string, string> { };
            var features = new Dictionary<string, bool> { { "MyFeature", true} };
            var controller = CreateController(appSettings: settings, features: features);

            // Act
            var result = await controller.GetApplicationSettings() as ResponseMessageResult;

            // Assert
            Assert.IsNotNull(result);
            var content = (ObjectContent<Dictionary<string, object>>)result.Response.Content;
            var actualSettings = (Dictionary<string, object>)content.Value;
            Assert.IsTrue(actualSettings.ContainsKey("Features"), "Cannot find 'Features'");
            var actualFeatures = (Dictionary<string,bool>)actualSettings["Features"];
            CollectionAssert.AreEquivalent(features, actualFeatures);
        }

        #endregion

        #region GetUserManagementSettings

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetUserManagementSettings_NonInstanceAdmin_ThrowsAuthorizationException()
        {
            // Arrange
            const int userId = 1;
            var user = new LoginUser {InstanceAdminRoleId = null};
            var settings = new UserManagementSettings
            {
                IsPasswordExpirationEnabled = false,
                IsFederatedAuthenticationEnabled = true
            };

            var settingsRepositoryMock = new Mock<ISqlSettingsRepository>();
            settingsRepositoryMock
                .Setup(m => m.GetUserManagementSettingsAsync())
                .ReturnsAsync(settings);
            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(m => m.GetLoginUserByIdAsync(userId))
                .ReturnsAsync(user);
            var controller = new ConfigController
            (
                null, 
                settingsRepositoryMock.Object, 
                userRepositoryMock.Object,
                null,
                null,
                null
            )
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = new Session { UserId = userId };

            // Act
            await controller.GetUserManagementSettings();
        }

        [TestMethod]
        public async Task GetUserManagementSettings_InstanceAdmin_ReturnsUserManagementSettings()
        {
            // Arrange
            const int userId = 1;
            var user = new LoginUser { InstanceAdminRoleId = 1 };
            var settings = new UserManagementSettings
            {
                IsPasswordExpirationEnabled = false,
                IsFederatedAuthenticationEnabled = true
            };

            var settingsRepositoryMock = new Mock<ISqlSettingsRepository>();
            settingsRepositoryMock
                .Setup(m => m.GetUserManagementSettingsAsync())
                .ReturnsAsync(settings);
            var userRepositoryMock = new Mock<IUserRepository>();
            userRepositoryMock
                .Setup(m => m.GetLoginUserByIdAsync(userId))
                .ReturnsAsync(user);

            var controller = new ConfigController
            (
                null, 
                settingsRepositoryMock.Object, 
                userRepositoryMock.Object, 
                null,
                null, 
                null
            )
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties[ServiceConstants.SessionProperty] = new Session { UserId = userId };

            // Act
            var result = await controller.GetUserManagementSettings() as OkNegotiatedContentResult<UserManagementSettings>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Content, settings);
        }

        #endregion GetUserManagementSettings

        #region GetConfig

        [TestMethod]
        public async Task GetConfig_NonEmptySettings_ReturnsHeadersAndContent()
        {
            // Arrange
            var settings = new Dictionary<string, string> { { "Key", "Value" } };
            var controller = CreateController(appSettings: settings);

            // Act
            var result = await controller.GetConfig() as ResponseMessageResult;

            // Assert
            Assert.IsNotNull(result);

            var content = await result.Response.Content.ReadAsStringAsync();

            Assert.AreEqual("(function (window) {\n" +
                "    if (!window.config) {\n" +
                "        window.config = {};\n" +
                "    }\n" +
                "    window.config.settings = {'Key':'Value','Features':'{\"Workflow\":true}'}\n" +
                "}(window));", content);
        }

        [TestMethod]
        public async Task GetConfig_EmptySettings_ReturnsHeadersAndContent()
        {
            // Arrange
            var settings = new Dictionary<string, string>();
            var controller = CreateController(appSettings: settings, features: new Dictionary<string, bool>());

            // Act
            var result = await controller.GetConfig() as ResponseMessageResult;

            // Assert
            Assert.IsNotNull(result);
            var content = await result.Response.Content.ReadAsStringAsync();
            Assert.AreEqual("(function (window) {\n" +
                "    if (!window.config) {\n" +
                "        window.config = {};\n" +
                "    }\n" +
                "    window.config.settings = {'Features':'{}'}\n" +
                "}(window));", content);
        }

        #endregion GetConfig

        private static ConfigController CreateController(Dictionary<string, Dictionary<string, string>> globalSettings = null, Dictionary<string, string> appSettings=null, Dictionary<string,bool> features = null)
        {
            var appSettingsRepo = new Mock<IApplicationSettingsRepository>();
            var featuresService = new Mock<IFeaturesService>();
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
                appSettingsRepo
                    .Setup(it => it.GetSettingsAsync(true))
                    .Throws(new ApplicationException());
            }
            else
            {
                appSettingsRepo
                    .Setup(it => it.GetSettingsAsync(true))
                    .ReturnsAsync(appSettings.Select(i => new ApplicationSetting {Key = i.Key, Value = i.Value, Restricted = true}));
            }

            featuresService
                .Setup(fs => fs.GetFeaturesAsync())
                .ReturnsAsync(features == null
                    ? new Dictionary<string, bool> { { "Workflow", true } }
                    : features
                );

            var logMock = new Mock<IServiceLogRepository>();
            var controller = new ConfigController(appSettingsRepo.Object, null, null, featuresService.Object, httpClientProvider, logMock.Object) { Request = new HttpRequestMessage() };
            controller.Request.Headers.Add("Session-Token", "");
            controller.Request.SetConfiguration(new HttpConfiguration());

            return controller;
        }
    }
}
