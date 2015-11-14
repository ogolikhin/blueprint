﻿using AdminStore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Helpers;

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

        #region GetFederatedAuthenticationSettingsAsync

        [TestMethod]
        public async Task GetFederatedAuthentication_QueryReturnsSettings_ReturnsFirst()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlSettingsRepository(cxn.Object);
            var xml = SerializationHelper.Serialize(new SerializationHelper.FASettings());
            dynamic dbObject = new ExpandoObject();
            dbObject.Settings = xml;
            dbObject.Certificate = null;
            var expectedFedAuthSettings = new FederatedAuthenticationSettings(xml, null);
            var result = new [] { dbObject };
            cxn.SetupQueryAsync("GetFederatedAuthentication", null, result);

            // Act
            var settings = await repository.GetFederatedAuthenticationSettingsAsync();

            // Assert
            cxn.Verify();
            Assert.IsTrue(Equals(expectedFedAuthSettings, settings));
        }

        [TestMethod]
        public async Task GetFederatedAuthentication_QueryReturnsEmpty_ReturnsNull()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlSettingsRepository(cxn.Object);
            dynamic result = new dynamic[] { };
            cxn.SetupQueryAsync("GetFederatedAuthentication", null, result);

            // Act
            var settings = await repository.GetFederatedAuthenticationSettingsAsync();

            // Assert
            cxn.Verify();
            Assert.AreEqual(null, settings);
        }

        #endregion GetFederatedAuthenticationSettingsAsync

        private bool Equals(IFederatedAuthenticationSettings x, IFederatedAuthenticationSettings y)
        {
            return Equals(x.Certificate, y.Certificate) &&
                Equals(x.ErrorUrl, y.ErrorUrl) &&
                Equals(x.LoginUrl, y.LoginUrl) &&
                Equals(x.LogoutUrl, y.LogoutUrl) &&
                Equals(x.NameClaimType, y.NameClaimType);
        }
    }
}
