using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Repositories;
using AdminStore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace AdminStore.Repositories
{
    [TestClass]
    public class SqlSettingsRepositoryTests
    {
        #region Constuctor

        [TestMethod]
        public void Constructor_CreatesConnectionToRaptorMain()
        {
            // Arrange

            // Act
            var repository = new SqlSettingsRepository();

            // Assert
            Assert.AreEqual(WebApiConfig.RaptorMain, repository._connectionWrapper.CreateConnection().ConnectionString);
        }

        #endregion Constructor

        #region GetLdapSettingsAsync

        [TestMethod]
        public async Task GetLdapSettingsAsync_QueryReturnsSettings_ReturnsSettings()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlSettingsRepository(cxn.Object);
            LdapSettings[] result = { new LdapSettings { Id = 1 } };
            cxn.SetupQueryAsync("GetLdapSettings", null, result);

            // Act
            IEnumerable<LdapSettings> settings = await repository.GetLdapSettingsAsync();

            // Assert
            cxn.Verify();
            CollectionAssert.AreEquivalent(result, settings.ToList());
        }

        #endregion GetLdapSettingsAsync

        #region GetInstanceSettingsAsync

        [TestMethod]
        public async Task GetInstanceSettingsAsync_QueryReturnsSettings_ReturnsFirst()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlSettingsRepository(cxn.Object);
            InstanceSettings[] result = { new InstanceSettings { UseDefaultConnection = true } };
            cxn.SetupQueryAsync("GetInstanceSettings", null, result);

            // Act
            InstanceSettings settings = await repository.GetInstanceSettingsAsync();

            // Assert
            cxn.Verify();
            Assert.AreEqual(result.First(), settings);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task GetInstanceSettingsAsync_QueryReturnsEmpty_ThrowsException()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlSettingsRepository(cxn.Object);
            InstanceSettings[] result = { };
            cxn.SetupQueryAsync("GetInstanceSettings", null, result);

            // Act
            InstanceSettings settings = await repository.GetInstanceSettingsAsync();

            // Assert
            cxn.Verify();
        }

        #endregion GetInstanceSettingsAsync

        #region GetFederatedAuthentication

        [TestMethod]
        public async Task GetFederatedAuthentication_QueryReturnsSettings_ReturnsFirst()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlSettingsRepository(cxn.Object);
            FederatedAuthenticationSettings[] result = { new FederatedAuthenticationSettings { IsEnabled = true } };
            cxn.SetupQueryAsync("GetFederatedAuthentication", null, result);

            // Act
            FederatedAuthenticationSettings settings = await repository.GetFederatedAuthentication();

            // Assert
            cxn.Verify();
            Assert.AreEqual(result.First(), settings);
        }

        [TestMethod]
        public async Task GetFederatedAuthentication_QueryReturnsEmpty_ReturnsNull()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlSettingsRepository(cxn.Object);
            FederatedAuthenticationSettings[] result = { };
            cxn.SetupQueryAsync("GetFederatedAuthentication", null, result);

            // Act
            FederatedAuthenticationSettings settings = await repository.GetFederatedAuthentication();

            // Assert
            cxn.Verify();
            Assert.AreEqual(null, settings);
        }

        #endregion GetFederatedAuthentication

        #region GetFederatedAuthenticationSettingsAsync

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public async Task GetFederatedAuthenticationSettingsAsync_ThrowsException()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlSettingsRepository(cxn.Object);

            // Act
            IFederatedAuthenticationSettings settings = await repository.GetFederatedAuthenticationSettingsAsync();

            // Assert
        }

        #endregion GetFederatedAuthenticationSettingsAsync
    }
}
