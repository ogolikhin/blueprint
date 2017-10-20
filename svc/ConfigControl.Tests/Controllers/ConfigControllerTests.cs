using ConfigControl.Models;
using ConfigControl.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace ConfigControl.Controllers
{
    [TestClass]
    public class ConfigControllerTests
    {
        #region GetConfig

        [TestMethod]
        public async Task GetConfig_RepositoryReturnsSettings_ReturnsHeadersAndContent()
        {
            // Arrange
            ConfigSetting[] settings = { new ConfigSetting { Key = "Key", Value = "Value", Group = "Group", IsRestricted = true } };
            var expected = new Dictionary<string, Dictionary<string, string>>();
            expected.Add("Group", new Dictionary<string, string>());
            expected["Group"].Add("Key", "Value");
            var configRepo = new Mock<IConfigRepository>();
            configRepo.Setup(r => r.GetSettings(It.IsAny<bool>())).ReturnsAsync((IEnumerable<ConfigSetting>)settings).Verifiable();
            var controller = new ConfigController(configRepo.Object) { Request = new HttpRequestMessage() };
            controller.Request.SetConfiguration(new HttpConfiguration());

            // Act
            var result = await controller.GetConfig(true) as ResponseMessageResult;

            // Assert
            configRepo.Verify();
            Assert.IsNotNull(result);
            var actual = await result.Response.Content.ReadAsAsync<Dictionary<string, Dictionary<string, string>>>();
            Assert.AreEqual(expected["Group"]["Key"], actual["Group"]["Key"]);

        }

        [TestMethod]
        public async Task GetConfig_RepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var configRepo = new Mock<IConfigRepository>();
            configRepo.Setup(r => r.GetSettings(It.IsAny<bool>())).Throws<Exception>().Verifiable();
            var controller = new ConfigController(configRepo.Object) { Request = new HttpRequestMessage() };
            controller.Request.SetConfiguration(new HttpConfiguration());

            // Act
            var result = await controller.GetConfig(false);

            // Assert
            configRepo.Verify();
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }

        #endregion GetConfig
    }
}
