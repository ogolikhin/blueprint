using System;
using System.Collections.Generic;
using System.Linq;
using LicenseLibrary.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sp.Agent;
using Sp.Agent.Configuration;
using Sp.Agent.Licensing;

namespace LicenseLibrary
{
    [TestClass]
    public class InishTechLicenseProviderTests
    {
        private static readonly Mock<IProductContext> BlueprintProductContext = new Mock<IProductContext>();
        private static readonly Mock<IProductContext> DataAnalyticsProductContext = new Mock<IProductContext>();
        private static InishTechLicenseProvider LicenseProvider;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            var agentContext = new Mock<IAgentContext>();
            agentContext.Setup(a => a.ProductContextFor(InishTechLicenseProvider.ProductName, InishTechLicenseProvider.ProductVersion))
                .Returns(BlueprintProductContext.Object).Verifiable();
            agentContext.Setup(a => a.ProductContextFor(InishTechLicenseProvider.DataAnalyticsProductName, InishTechLicenseProvider.DataAnalyticsProductVersion))
                .Returns(DataAnalyticsProductContext.Object).Verifiable();
            LicenseProvider = InishTechLicenseProvider.GetInstance(agentContext.Object);
        }

        #region GetBlueprintLicenses

        [TestMethod]
        public void GetBlueprintLicenses_NoLicenses_ReturnsEmpty()
        {
            // Arrange
            BlueprintProductContext.Setup(p => p.Licenses.Valid()).Returns(new ILicense[] {}).Verifiable();

            // Act
            IEnumerable<LicenseWrapper> result = LicenseProvider.GetBlueprintLicenses();

            // Assert
            Assert.IsFalse(result.Any());
            BlueprintProductContext.Verify();
        }

        [TestMethod]
        public void GetBlueprintLicenses_SingleLicense_ReturnsWrapperPerValidFeature()
        {
            // Arrange
            var productContext = new Mock<IProductContext>(MockBehavior.Strict);
            var license = new Mock<ILicense>(MockBehavior.Strict);
            var feature1 = new Mock<IFeature>(MockBehavior.Strict);
            var feature2 = new Mock<IFeature>(MockBehavior.Strict);
            var nextWeek = DateTime.Now.AddDays(7);
            var tomorrow = DateTime.Now.AddDays(1);
            var yesterday = DateTime.Now.AddDays(-1);
            var activationKey = "activationKey";
            BlueprintProductContext.Setup(p => p.Licenses.Valid()).Returns(new [] { license.Object }).Verifiable();
            license.Setup(l => l.Advanced.AllFeatures()).Returns(new Dictionary<string, IFeature>
            {
                { "feature1", feature1.Object },
                { "feature2", feature2.Object }
            }).Verifiable();
            license.SetupGet(l => l.ValidUntil).Returns(nextWeek).Verifiable();
            license.SetupGet(l => l.ActivationKey).Returns(activationKey).Verifiable();
            feature1.SetupGet(f => f.ValidUntil).Returns(tomorrow).Verifiable();
            feature1.SetupGet(f => f.ConcurrentUsageLimit).Returns(5).Verifiable();
            feature2.SetupGet(f => f.ValidUntil).Returns(yesterday).Verifiable();

            // Act
            IEnumerable<LicenseWrapper> result = LicenseProvider.GetBlueprintLicenses();

            // Assert
            var resultList = result.ToList();
            Assert.AreEqual(1, resultList.Count);
            Assert.AreEqual("feature1", resultList[0].FeatureName);
            Assert.AreEqual(5, resultList[0].MaximumLicenses);
            Assert.AreEqual(tomorrow, resultList[0].ExpirationDate);
            Assert.AreEqual(activationKey, resultList[0].ActivationKey);
            license.Verify();
            feature1.Verify();
            feature2.Verify();
        }

        #endregion GetBlueprintLicenses

        #region GetDataAnalyticsLicenses

        [TestMethod]
        public void GetDataAnalyticsLicenses_NoLicenses_ReturnsEmpty()
        {
            // Arrange
            DataAnalyticsProductContext.Setup(p => p.Licenses.Valid()).Returns(new ILicense[] { }).Verifiable();

            // Act
            IEnumerable<LicenseWrapper> result = LicenseProvider.GetDataAnalyticsLicenses();

            // Assert
            Assert.IsFalse(result.Any());
            DataAnalyticsProductContext.Verify();
        }

        #endregion GetDataAnalyticsLicenses
    }
}
