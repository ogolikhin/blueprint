using System;
using System.Collections.Generic;
using LicenseLibrary.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sp.Agent;
using Sp.Agent.Configuration;
using Sp.Agent.Licensing;

namespace LicenseLibrary.Repositories
{
    [TestClass]
    public class InishTechLicenseManagerTests
    {
        #region GetLicenseKey

        [TestMethod]
        public void GetLicenseKey_NoValidLicenses_ReturnsNull()
        {
            // Arrange
            var agentContext = CreateAgentContext(null, null);
            var licenseProvider = new InishTechLicenseManager(agentContext);
            var productFeature = ProductFeature.None;

            // Act
            LicenseKey result = licenseProvider.GetLicenseKey(productFeature);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetLicenseKey_NoMatchingFeature_ReturnsNull()
        {
            // Arrange
            var nextWeek = DateTime.Now.AddDays(7);
            var activationKey = "activationKey";
            var tomorrow = DateTime.Now.AddDays(1);
            int? concurrentUsageLimit = 3;
            var license = CreateLicenseMock(nextWeek, activationKey, concurrentUsageLimit, ProductFeature.Viewer.GetFeatureName(), tomorrow);
            var agentContext = CreateAgentContext(InishTechAttribute.BlueprintProductName, InishTechAttribute.BlueprintProductVersion, license);
            var licenseProvider = new InishTechLicenseManager(agentContext);
            var productFeature = ProductFeature.Author;

            // Act
            LicenseKey result = licenseProvider.GetLicenseKey(productFeature);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetLicenseKey_MatchingLicense_ReturnsCorrectInfo()
        {
            // Arrange
            var nextWeek = DateTime.Now.AddDays(7);
            var activationKey = "activationKey";
            int? concurrentUsageLimit = 3;
            var license = CreateLicenseMock(nextWeek, activationKey, concurrentUsageLimit);
            var agentContext = CreateAgentContext(InishTechAttribute.DataAnalyticsProductName, InishTechAttribute.DataAnalyticsProductVersion, license);
            var licenseProvider = new InishTechLicenseManager(agentContext);
            var productFeature = ProductFeature.DataAnalytics;

            // Act
            LicenseKey result = licenseProvider.GetLicenseKey(productFeature);

            // Assert
            Assert.AreEqual(productFeature, result.ProductFeature);
            Assert.AreEqual(concurrentUsageLimit, result.MaximumLicenses);
            Assert.AreEqual(nextWeek, result.ExpirationDate);
            Assert.AreEqual(activationKey, result.ActivationKey);
        }

        [TestMethod]
        public void GetLicenseKey_MultipleMatchingLicenses_ReturnsCorrectInfo()
        {
            // Arrange
            var tomorrow = DateTime.Now.AddDays(1);
            var activationKey = "activationKey";
            int? concurrentUsageLimit1 = 4;
            var nextMonth = DateTime.Now.AddMonths(1);
            var nextYear = DateTime.Now.AddYears(1);
            int? concurrentUsageLimit2 = 5;
            var nextWeek = DateTime.Now.AddDays(7);
            var license1 = CreateLicenseMock(tomorrow, activationKey, concurrentUsageLimit1, ProductFeature.Author.GetFeatureName(), nextMonth);
            var license2 = CreateLicenseMock(nextYear, activationKey, concurrentUsageLimit2, ProductFeature.Author.GetFeatureName(), nextWeek);
            var agentContext = CreateAgentContext(InishTechAttribute.BlueprintProductName, InishTechAttribute.BlueprintProductVersion, license1, license2);
            var licenseProvider = new InishTechLicenseManager(agentContext);

            // Act
            LicenseKey result = licenseProvider.GetLicenseKey(ProductFeature.Author);

            // Assert
            Assert.AreEqual(ProductFeature.Author, result.ProductFeature);
            Assert.AreEqual(concurrentUsageLimit1 + concurrentUsageLimit2, result.MaximumLicenses);
            Assert.AreEqual(nextWeek, result.ExpirationDate);
            Assert.AreEqual(activationKey, result.ActivationKey);
        }

        [TestMethod]
        public void GetLicenseKey_ConcurrentUsageLimitIsNull_ReturnsUnlimitedUsage()
        {
            // Arrange
            var tomorrow = DateTime.Now.AddDays(1);
            var activationKey = "activationKey";
            int? concurrentUsageLimit1 = 4;
            var nextMonth = DateTime.Now.AddMonths(1);
            var nextYear = DateTime.Now.AddYears(1);
            var nextWeek = DateTime.Now.AddDays(7);
            var license1 = CreateLicenseMock(tomorrow, activationKey, concurrentUsageLimit1, ProductFeature.Author.GetFeatureName(), nextMonth);
            var license2 = CreateLicenseMock(nextYear, activationKey, null, ProductFeature.Author.GetFeatureName(), nextWeek);
            var agentContext = CreateAgentContext(InishTechAttribute.BlueprintProductName, InishTechAttribute.BlueprintProductVersion, license1, license2);
            var licenseProvider = new InishTechLicenseManager(agentContext);

            // Act
            LicenseKey result = licenseProvider.GetLicenseKey(ProductFeature.Author);

            // Assert
            Assert.AreEqual(ProductFeature.Author, result.ProductFeature);
            Assert.IsNull(result.MaximumLicenses);
            Assert.AreEqual(nextWeek, result.ExpirationDate);
            Assert.AreEqual(activationKey, result.ActivationKey);
        }

        [TestMethod]
        public void GetLicenseKey_ConcurrentUsageLimitOverflows_ReturnsUnlimitedUsage()
        {
            // Arrange
            var tomorrow = DateTime.Now.AddDays(1);
            var activationKey = "activationKey";
            int concurrentUsageLimit1 = int.MaxValue - 1;
            var nextMonth = DateTime.Now.AddMonths(1);
            var nextYear = DateTime.Now.AddYears(1);
            int concurrentUsageLimit2 = 5;
            var nextWeek = DateTime.Now.AddDays(7);
            var license1 = CreateLicenseMock(tomorrow, activationKey, concurrentUsageLimit1, ProductFeature.Author.GetFeatureName(), nextMonth);
            var license2 = CreateLicenseMock(nextYear, activationKey, concurrentUsageLimit2, ProductFeature.Author.GetFeatureName(), nextWeek);
            var agentContext = CreateAgentContext(InishTechAttribute.BlueprintProductName,
                InishTechAttribute.BlueprintProductVersion, license1, license2);
            var licenseProvider = new InishTechLicenseManager(agentContext);

            // Act
            LicenseKey result = licenseProvider.GetLicenseKey(ProductFeature.Author);

            // Assert
            Assert.AreEqual(ProductFeature.Author, result.ProductFeature);
            Assert.IsNull(result.MaximumLicenses);
            Assert.AreEqual(nextWeek, result.ExpirationDate);
            Assert.AreEqual(activationKey, result.ActivationKey);
        }

        #endregion GetLicenseKey

        private static IAgentContext CreateAgentContext(string productName, string productVersion,
            params ILicense[] licenses)
        {
            var agentContext = new Mock<IAgentContext>();
            var productContext = new Mock<IProductContext>();
            agentContext.Setup(a => a.ProductContextFor(productName, productVersion))
                .Returns(productContext.Object);
            productContext.Setup(p => p.Licenses.Valid()).Returns(licenses);
            return agentContext.Object;
        }

        private static ILicense CreateLicenseMock(DateTime validUntil, string activationKey,
            int? concurrentUsageLimit, string featureName = null, DateTime featureValidUntil = new DateTime())
        {
            var license = new Mock<ILicense>();
            license.Setup(l => l.ValidUntil).Returns(validUntil);
            license.Setup(l => l.ActivationKey).Returns(activationKey);
            if (featureName == null)
            {
                license.Setup(l => l.Advanced.ConcurrentUsageLimit).Returns(concurrentUsageLimit);
            }
            else
            {
                var feature = new Mock<IFeature>();
                license.Setup(l => l.Advanced.AllFeatures()).Returns(new Dictionary<string, IFeature>
                {
                    { featureName, feature.Object }
                });
                feature.Setup(f => f.ValidUntil).Returns(featureValidUntil);
                feature.Setup(f => f.ConcurrentUsageLimit).Returns(concurrentUsageLimit);
            }
            return license.Object;
        }
    }
}
