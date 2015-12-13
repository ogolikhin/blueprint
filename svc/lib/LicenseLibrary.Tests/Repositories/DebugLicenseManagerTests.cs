using System;
using LicenseLibrary.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LicenseLibrary.Repositories
{
    [TestClass]
    public class DebugLicenseManagerTests
    {
        #region GetLicenseKey

        [TestMethod]
        public void GetLicenseKey_Always_ReturnsUnlimitedLicenseInfo()
        {
            // Arrange
            var feature = ProductFeature.Author;
            var licenseManager = new DebugLicenseManager();

            // Act
            LicenseKey result = licenseManager.GetLicenseKey(feature);

            // Assert
            Assert.AreEqual(feature, result.ProductFeature);
            Assert.IsNull(result.MaximumLicenses);
            Assert.AreEqual(DateTime.MaxValue, result.ExpirationDate);
        }

        #endregion GetLicenseKey
    }
}
