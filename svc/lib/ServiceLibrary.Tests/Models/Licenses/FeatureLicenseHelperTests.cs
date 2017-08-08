using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Helpers;
using ServiceLibrary.Helpers.Security;
using ServiceLibrary.Models.Enums;

namespace ServiceLibrary.Models.Licenses
{
    /// <summary>
    /// Tests for the FeatureLicenseHelper
    /// </summary>
    [TestClass]
    public class FeatureLicenseHelperTests
    {
        [TestMethod]
        public void DecryptLicenses_DecryptsAllLicensesSuccessfully()
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

        [TestMethod]
        public void DecryptLicenses_ReturnsNoLicenses_WhenTheEncryptedLicensesStringIsEmpty()
        {
            //Arrange
            var encryptedLicenses = string.Empty;

            //Act
            var decryptedLicenses = FeatureLicenseHelper.DecryptLicenses(encryptedLicenses);

            //Assert
            Assert.AreEqual(0, decryptedLicenses.Length);
        }

        [TestMethod]
        public void DecryptLicenses_ReturnsNoLicenses_WhenTheEncryptedLicensesStringIsNull()
        {
            //Arrange
            string encryptedLicenses = null;

            //Act
            var decryptedLicenses = FeatureLicenseHelper.DecryptLicenses(encryptedLicenses);

            //Assert
            Assert.AreEqual(0, decryptedLicenses.Length);
        }

        [TestMethod]
        public void DecryptLicenses_ReturnsNoLicenses_WhenTheEncryptedLicensesStringIsAnInvalidValue()
        {
            //Arrange
            const string encryptedLicenses = "invalid encrypted value";

            //Act
            var decryptedLicenses = FeatureLicenseHelper.DecryptLicenses(encryptedLicenses);

            //Assert
            Assert.AreEqual(0, decryptedLicenses.Length);
        }

        [TestMethod]
        public void DecryptLicenses_ReturnsNoLicenses_WhenLicensesAreNotAnArrayOfFeatureInformationObjects()
        {
            //Arrange
            var license = new[] {"this is not an array of FeatureInformation objects"};
            var xml = SerializationHelper.ToXml(license);
            var encryptedXml = SystemEncryptions.Encrypt(xml);
            var encryptedLicenses = encryptedXml;

            //Act
            var decryptedLicenses = FeatureLicenseHelper.DecryptLicenses(encryptedLicenses);

            //Assert
            Assert.AreEqual(0, decryptedLicenses.Length);
        }

        [TestMethod]
        public void DecryptLicenses_ReturnsLicenseTypeNone_WhenTheEncryptedLicenseHasAnInvalidFeatureName()
        {
            //Arrange
            var license = new[] {new FeatureInformation("invalid feature name", DateTime.MaxValue)};
            var xml = SerializationHelper.ToXml(license);
            var encryptedXml = SystemEncryptions.Encrypt(xml);
            var encryptedLicenses = encryptedXml;

            //Act
            var decryptedLicenses = FeatureLicenseHelper.DecryptLicenses(encryptedLicenses);

            //Assert
            Assert.AreEqual(1, decryptedLicenses.Length);
            var featureType = decryptedLicenses.Single().GetFeatureType();
            Assert.AreEqual(FeatureTypes.None, featureType);
        }
    }
}
