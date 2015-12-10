using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sp.Agent.Configuration;

namespace LicenseLibrary
{
    [TestClass]
    public class InishTechLicenseProviderTests
    {
        #region GetBlueprintLicenses

        [TestMethod]
        public void GetBlueprintLicenses_Condition_Expectation() //TODO
        {
            // Arrange
            var agentContext = new Mock<IAgentContext>();
            var licenseProvider = InishTechLicenseProvider.GetInstance(agentContext.Object);

            // Act
            IEnumerable<LicenseWrapper> result = licenseProvider.GetBlueprintLicenses();

            // Assert
            //TODO
        }

        #endregion GetBlueprintLicenses

        #region GetDataAnalyticsLicenses

        [TestMethod]
        public void GetDataAnalyticsLicenses_Condition_Expectation() //TODO
        {
            // Arrange
            var agentContext = new Mock<IAgentContext>();
            var licenseProvider = InishTechLicenseProvider.GetInstance(agentContext.Object);

            // Act
            IEnumerable<LicenseWrapper> result = licenseProvider.GetDataAnalyticsLicenses();

            // Assert
            //TODO
        }

        #endregion GetDataAnalyticsLicenses
    }
}
