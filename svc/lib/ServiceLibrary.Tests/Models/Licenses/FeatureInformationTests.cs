using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using ServiceLibrary.Helpers.Security;
using ServiceLibrary.Models.Enums;

namespace ServiceLibrary.Models.Licenses
{
    /// <summary>
    /// Summary description for FeatureInformationTests
    /// </summary>
    [TestClass]
    public class FeatureInformationTests
    {
        private string _workflowFeatureName;
        private Mock<ITimeProvider> _timeProviderMock;

        [TestInitialize]
        public void TestInitialize()
        {
            _timeProviderMock = new Mock<ITimeProvider>(MockBehavior.Strict);
            _workflowFeatureName = FeatureInformation.Features.Single(f => f.Value == FeatureTypes.Workflow).Key;
        }

        [TestMethod]
        public void GetStatus_WhenExpirationDateIsUpcoming_StatusIsActive()
        {
            //Arrange
            var expirationDate = DateTime.MaxValue;
            var featureInformation = new FeatureInformation(_workflowFeatureName, expirationDate);
            //Act
            var status = featureInformation.GetStatus();
            //Assert
            Assert.AreEqual(FeatureLicenseStatus.Active, status);
        }

        [TestMethod]
        public void GetStatus_WhenExpirationDateIsEqual_StatusIsActive()
        {
            //Arrange
            var expirationDate = new DateTime(2000, 1, 1);
            _timeProviderMock.Setup(i => i.CurrentUniversalTime).Returns(expirationDate);
            var featureInformation = new FeatureInformation(_workflowFeatureName, expirationDate, _timeProviderMock.Object);
            //Act
            var status = featureInformation.GetStatus();
            //Assert
            Assert.AreEqual(FeatureLicenseStatus.Active, status);
        }

        [TestMethod]
        public void GetStatus_WhenExpirationDateHasPassed_StatusIsExpired()
        {
            //Arrange
            var expirationDate = DateTime.MinValue;
            var featureInformation = new FeatureInformation(_workflowFeatureName, expirationDate);
            //Act
            var status = featureInformation.GetStatus();
            //Assert
            Assert.AreEqual(FeatureLicenseStatus.Expired, status);
        }

        [TestMethod]
        public void GetFeatureType_WhenFeatureNameIsWorkflow_FeatureTypeIsWorkflow()
        {
            //Arrange
            var featureInformation = new FeatureInformation(_workflowFeatureName, DateTime.UtcNow);
            //Act
            var featureType = featureInformation.GetFeatureType();
            //Assert
            Assert.AreEqual(FeatureTypes.Workflow, featureType);
        }

        [TestMethod]
        public void FeatureLicenseHelper_DecryptsAllLicensesSuccessfully()
        {
            //Arrange
            var expirationDate = DateTime.MaxValue;
            var allFeatures = FeatureInformation.Features.Select(p => new FeatureInformation(p.Key, expirationDate)).ToArray();
            var xml = SerializationHelper.ToXml(allFeatures);
            var encryptedXml = SystemEncryptions.Encrypt(xml);

            //Act
            var decryptedLicenses = FeatureLicenseHelper.DecryptLicenses(encryptedXml);

            //Assert
            Assert.AreEqual(allFeatures.Length, decryptedLicenses.Length);
            foreach (var feature in allFeatures)
            {
                var decryptedLicense = decryptedLicenses.Single(l => l.FeatureName == feature.FeatureName && l.ExpirationDate == feature.ExpirationDate);
                Assert.IsNotNull(decryptedLicense);
            }
        }
    }
}
