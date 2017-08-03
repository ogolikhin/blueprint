using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using ServiceLibrary.Helpers.Security;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Licenses;

namespace ServiceLibrary.Repositories
{
    /// <summary>
    /// Tests for the ApplicationSettingsRepository
    /// </summary>
    [TestClass]
    public class ApplicationSettingsRepositoryTests
    {
        private Mock<ISqlConnectionWrapper> _sqlConnectionWrapperMock;
        private ApplicationSettingsRepository _applicationSettingsRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            _sqlConnectionWrapperMock = new Mock<ISqlConnectionWrapper>(MockBehavior.Strict);
            _applicationSettingsRepository = new ApplicationSettingsRepository(_sqlConnectionWrapperMock.Object);
        }

        [TestMethod]
        public async Task ApplicationSettingsRepository_ReturnsWorkflowFalse_WhenLicenseInfoSettingIsNotFound()
        {
            //Arrange
            var dbSettings = new List<ApplicationSetting>();
            _sqlConnectionWrapperMock.Setup(m => m.QueryAsync<ApplicationSetting>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<IDbTransaction>(), It.IsAny<int?>(), It.IsAny<CommandType?>())).ReturnsAsync(dbSettings);

            //Act
            var settings = await _applicationSettingsRepository.GetSettingsAsync(true);

            //Assert
            var workflowSetting = settings.Single(s => s.Key == ServiceConstants.WorkflowFeatureKey);
            Assert.AreEqual(false.ToString(), workflowSetting.Value);
        }

        [TestMethod]
        public async Task ApplicationSettingsRepository_ReturnsWorkflowFalse_WhenLicenseInfoSettingValueIsEmpty()
        {
            //Arrange
            var licenseInfoValue = string.Empty;
            var dbSettings = new List<ApplicationSetting> {new ApplicationSetting {Value = licenseInfoValue, Key = ServiceConstants.LicenseInfoApplicationSettingKey, Restricted = false}};
            _sqlConnectionWrapperMock.Setup(m => m.QueryAsync<ApplicationSetting>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<IDbTransaction>(), It.IsAny<int?>(), It.IsAny<CommandType?>())).ReturnsAsync(dbSettings);

            //Act
            var settings = await _applicationSettingsRepository.GetSettingsAsync(true);

            //Assert
            var workflowSetting = settings.Single(s => s.Key == ServiceConstants.WorkflowFeatureKey);
            Assert.AreEqual(false.ToString(), workflowSetting.Value);
        }

        [TestMethod]
        public async Task ApplicationSettingsRepository_ReturnsWorkflowFalse_WhenLicenseInfoSettingValueIsNull()
        {
            //Arrange
            string licenseInfoValue = null;
            var dbSettings = new List<ApplicationSetting> {new ApplicationSetting {Value = licenseInfoValue, Key = ServiceConstants.LicenseInfoApplicationSettingKey, Restricted = false}};
            _sqlConnectionWrapperMock.Setup(m => m.QueryAsync<ApplicationSetting>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<IDbTransaction>(), It.IsAny<int?>(), It.IsAny<CommandType?>())).ReturnsAsync(dbSettings);

            //Act
            var settings = await _applicationSettingsRepository.GetSettingsAsync(true);

            //Assert
            var workflowSetting = settings.Single(s => s.Key == ServiceConstants.WorkflowFeatureKey);
            Assert.AreEqual(false.ToString(), workflowSetting.Value);
        }

        [TestMethod]
        public async Task ApplicationSettingsRepository_ReturnsWorkflowFalse_WhenLicenseInfoSettingValueIsInvalid()
        {
            //Arrange
            var licenseInfoValue = "invalid encrypted value";
            var dbSettings = new List<ApplicationSetting> {new ApplicationSetting {Value = licenseInfoValue, Key = ServiceConstants.LicenseInfoApplicationSettingKey, Restricted = false}};
            _sqlConnectionWrapperMock.Setup(m => m.QueryAsync<ApplicationSetting>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<IDbTransaction>(), It.IsAny<int?>(), It.IsAny<CommandType?>())).ReturnsAsync(dbSettings);

            //Act
            var settings = await _applicationSettingsRepository.GetSettingsAsync(true);

            //Assert
            var workflowSetting = settings.Single(s => s.Key == ServiceConstants.WorkflowFeatureKey);
            Assert.AreEqual(false.ToString(), workflowSetting.Value);
        }

        [TestMethod]
        public async Task ApplicationSettingsRepository_ReturnsWorkflowFalse_WhenLicenseFeatureNameIsInvalid()
        {
            //Arrange
            var license = new[] {new FeatureInformation("invalid feature name", DateTime.MaxValue)};
            var xml = SerializationHelper.ToXml(license);
            var encryptedXml = SystemEncryptions.Encrypt(xml);
            var licenseInfoValue = encryptedXml;

            var dbSettings = new List<ApplicationSetting> {new ApplicationSetting {Value = licenseInfoValue, Key = ServiceConstants.LicenseInfoApplicationSettingKey, Restricted = false}};
            _sqlConnectionWrapperMock.Setup(m => m.QueryAsync<ApplicationSetting>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<IDbTransaction>(), It.IsAny<int?>(), It.IsAny<CommandType?>())).ReturnsAsync(dbSettings);

            //Act
            var settings = await _applicationSettingsRepository.GetSettingsAsync(true);

            //Assert
            var workflowSetting = settings.Single(s => s.Key == ServiceConstants.WorkflowFeatureKey);
            Assert.AreEqual(false.ToString(), workflowSetting.Value);
        }

        [TestMethod]
        public async Task ApplicationSettingsRepository_ReturnsWorkflowFalse_WhenLicensesAreNotAnArrayOfFeatureInformationObjects()
        {
            //Arrange
            var license = new[] {"this is not an array of FeatureInformation objects"};
            var xml = SerializationHelper.ToXml(license);
            var encryptedXml = SystemEncryptions.Encrypt(xml);
            var licenseInfoValue = encryptedXml;

            var dbSettings = new List<ApplicationSetting> {new ApplicationSetting {Value = licenseInfoValue, Key = ServiceConstants.LicenseInfoApplicationSettingKey, Restricted = false}};
            _sqlConnectionWrapperMock.Setup(m => m.QueryAsync<ApplicationSetting>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<IDbTransaction>(), It.IsAny<int?>(), It.IsAny<CommandType?>())).ReturnsAsync(dbSettings);

            //Act
            var settings = await _applicationSettingsRepository.GetSettingsAsync(true);

            //Assert
            var workflowSetting = settings.Single(s => s.Key == ServiceConstants.WorkflowFeatureKey);
            Assert.AreEqual(false.ToString(), workflowSetting.Value);
        }

        [TestMethod]
        public async Task ApplicationSettingsRepository_ReturnsWorkflowFalse_WhenWorkflowLicenseIsNotFound()
        {
            //Arrange
            var licenseInfoValue = GetEncryptedLicense(FeatureTypes.Storyteller, DateTime.MaxValue);
            var dbSettings = new List<ApplicationSetting> {new ApplicationSetting {Value = licenseInfoValue, Key = ServiceConstants.LicenseInfoApplicationSettingKey, Restricted = false}};
            _sqlConnectionWrapperMock.Setup(m => m.QueryAsync<ApplicationSetting>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<IDbTransaction>(), It.IsAny<int?>(), It.IsAny<CommandType?>())).ReturnsAsync(dbSettings);

            //Act
            var settings = await _applicationSettingsRepository.GetSettingsAsync(true);

            //Assert
            var workflowSetting = settings.Single(s => s.Key == ServiceConstants.WorkflowFeatureKey);
            Assert.AreEqual(false.ToString(), workflowSetting.Value);
        }

        private string GetEncryptedLicense(FeatureTypes feature, DateTime expiration)
        {
            var license = new[] {new FeatureInformation(FeatureInformation.Features.Single(p => p.Value == feature).Key, expiration)};
            var xml = SerializationHelper.ToXml(license);
            return SystemEncryptions.Encrypt(xml);
        }

        [TestMethod]
        public async Task ApplicationSettingsRepository_ReturnsWorkflowFalse_WhenWorkflowLicenseIsExpired()
        {
            //Arrange
            var licenseInfoValue = GetEncryptedLicense(FeatureTypes.Workflow, DateTime.MinValue);
            var dbSettings = new List<ApplicationSetting> {new ApplicationSetting {Value = licenseInfoValue, Key = ServiceConstants.LicenseInfoApplicationSettingKey, Restricted = false}};
            _sqlConnectionWrapperMock.Setup(m => m.QueryAsync<ApplicationSetting>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<IDbTransaction>(), It.IsAny<int?>(), It.IsAny<CommandType?>())).ReturnsAsync(dbSettings);

            //Act
            var settings = await _applicationSettingsRepository.GetSettingsAsync(true);

            //Assert
            var workflowSetting = settings.Single(s => s.Key == ServiceConstants.WorkflowFeatureKey);
            Assert.AreEqual(false.ToString(), workflowSetting.Value);
        }

        [TestMethod]
        public async Task ApplicationSettingsRepository_ReturnsWorkflowTrue_WhenWorkflowLicenseIsValid()
        {
            //Arrange
            var licenseInfoValue = GetEncryptedLicense(FeatureTypes.Workflow, DateTime.MaxValue);
            var dbSettings = new List<ApplicationSetting> {new ApplicationSetting {Value = licenseInfoValue, Key = ServiceConstants.LicenseInfoApplicationSettingKey, Restricted = false}};
            _sqlConnectionWrapperMock.Setup(m => m.QueryAsync<ApplicationSetting>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<IDbTransaction>(), It.IsAny<int?>(), It.IsAny<CommandType?>())).ReturnsAsync(dbSettings);

            //Act
            var settings = await _applicationSettingsRepository.GetSettingsAsync(true);

            //Assert
            var workflowSetting = settings.Single(s => s.Key == ServiceConstants.WorkflowFeatureKey);
            Assert.AreEqual(true.ToString(), workflowSetting.Value);
        }
    }
}
