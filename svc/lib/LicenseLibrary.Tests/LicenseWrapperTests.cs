using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sp.Agent.Licensing;

namespace LicenseLibrary
{
    [TestClass]
    public class LicenseWrapperTests
    {
        [TestMethod]
        public void Constructor_Condition_Expectation() //TODO
        {
            // Arrange
            string featureName = "feature";
            var license = new Mock<ILicense>(); //TODO
            var feature = new Mock<IFeature>(); //TODO

            // Act
            var result = new LicenseWrapper(featureName, license.Object, feature.Object);

            // Assert
            //TODO
        }

        //TODO test constructor with license only

        //TODO test constructor with empty enumerable

        //TODO test constructor with single-item enumerable

        //TODO test constructor with multi-item enumerable

        //TODO test ValidFeatures with feature = null

        //TODO test ValidFeatures with feature != null
    }
}
