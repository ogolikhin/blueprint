using System;
using LicenseLibrary.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LicenseLibrary.Repositories
{
    [TestClass]
    public class DebugLicenseManagerTests
    {
        [TestMethod]
        public void GetLicenseInfo_Always_ReturnsUnlimitedLicenseInfo()
        {
            // Arrange
            var feature = ProductFeature.Author;
            var licenseManager = new DebugLicenseManager();

            // Act
            LicenseInfo result = licenseManager.GetLicenseInfo(feature);

            // Assert
            Assert.AreEqual(feature, result.ProductFeature);
            Assert.IsNull(result.MaximumLicenses);
            Assert.AreEqual(DateTime.MaxValue, result.ExpirationDate);
        }
    }
}
