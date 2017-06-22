using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Enums;

namespace ServiceLibrary.Models.Licenses
{
    /// <summary>
    /// Summary description for FeatureInformationTests
    /// </summary>
    [TestClass]
    public class FeatureInformationTests
    {
        private const string WorkflowFeatureName = "Workflow";
        private Mock<ITimeProvider> _timeProviderMock;

        [TestInitialize]
        public void TestInitialize()
        {
            _timeProviderMock = new Mock<ITimeProvider>(MockBehavior.Strict);
        }

        [TestMethod]
        public void GetStatus_WhenExpirationDateIsUpcoming_StatusIsActive()
        {
            //Arrange
            var expirationDate = DateTime.MaxValue;
            var featureInformation = new FeatureInformation(WorkflowFeatureName, expirationDate);
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
            var featureInformation = new FeatureInformation(WorkflowFeatureName, expirationDate, _timeProviderMock.Object);
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
            var featureInformation = new FeatureInformation(WorkflowFeatureName, expirationDate);
            //Act
            var status = featureInformation.GetStatus();
            //Assert
            Assert.AreEqual(FeatureLicenseStatus.Expired, status);
        }

        [TestMethod]
        public void GetFeatureType_WhenFeatureNameIsWorkflow_FeatureTypeIsWorkflow()
        {
            //Arrange
            var featureInformation = new FeatureInformation(WorkflowFeatureName, DateTime.UtcNow);
            //Act
            var featureType = featureInformation.GetFeatureType();
            //Assert
            Assert.AreEqual(FeatureTypes.Workflow, featureType);
        }
    }
}
