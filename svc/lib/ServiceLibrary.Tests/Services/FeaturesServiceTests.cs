using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Repositories.ApplicationSettings;
using Moq;
using ServiceLibrary.Helpers;
using ServiceLibrary.Helpers.Cache;
using System.Threading.Tasks;
using ServiceLibrary.Models.Enums;

namespace ServiceLibrary.Services
{
    [TestClass]
    public class FeaturesServiceTests
    {
        private Mock<IFeaturesRepository> _featuresRepositoryMock;
        private Mock<IFeatureLicenseHelper> _licenseHelperMock;

        [TestInitialize]
        public void TestInitialize()
        {
            _featuresRepositoryMock = new Mock<IFeaturesRepository>(MockBehavior.Strict);
            _licenseHelperMock = new Mock<IFeatureLicenseHelper>(MockBehavior.Strict);
        }

        public void SetupFeaturesRepositoryMock(params Feature[] features)
        {
            _featuresRepositoryMock
                .Setup(f => f.GetFeaturesAsync(false))
                .ReturnsAsync(features == null ? new Feature[0] : features);
        }

        public void SetupLicenseHelperMock(FeatureTypes featureTypes = FeatureTypes.None)
        {
            _licenseHelperMock
                .Setup(l => l.GetValidBlueprintLicenseFeatures())
                .Returns(featureTypes);
        }

        [TestMethod]
        public async Task FeaturesService_GetFeaturesAsync_CallToFeaturesRepository()
        {
            // Arrange
            const string featureName = "TestFeature";
            const bool isFeatureEnabled = true;

            SetupFeaturesRepositoryMock(CreateFeature(featureName, isFeatureEnabled));
            SetupLicenseHelperMock();
            
            var service = new FeaturesService(
                _featuresRepositoryMock.Object,
                _licenseHelperMock.Object,
                AsyncCache.NoCache
            );

            // Act
            var actualFeatures = await service.GetFeaturesAsync();

            // Asserts
            Assert.IsNotNull(actualFeatures);
            Assert.IsTrue(actualFeatures.ContainsKey(featureName), "Cannot find TestFeature");
            Assert.AreEqual(isFeatureEnabled, actualFeatures[featureName]);
        }

        [TestMethod]
        public async Task FeaturesService_GetFeaturesAsync_WorkflowLicenceExists_WorkflowEnabled()
        {
            // Arrange
            SetupLicenseHelperMock(FeatureTypes.Workflow);
            SetupFeaturesRepositoryMock();

            var service = new FeaturesService(
                _featuresRepositoryMock.Object,
                _licenseHelperMock.Object,
                AsyncCache.NoCache
            );

            // Act
            var actualFeatures = await service.GetFeaturesAsync();

            // Asserts
            Assert.IsNotNull(actualFeatures);
            Assert.IsTrue(actualFeatures.ContainsKey("Workflow"), "Cannot find Workflow");
            Assert.AreEqual(true, actualFeatures["Workflow"]);
        }

        [TestMethod]
        public async Task FeaturesService_GetFeaturesAsync_WorkflowLicenceMissing_WorkflowDisabled()
        {
            // Arrange
            SetupLicenseHelperMock(FeatureTypes.MicrosoftTfsIntegration);
            SetupFeaturesRepositoryMock();

            var service = new FeaturesService(
                _featuresRepositoryMock.Object,
                _licenseHelperMock.Object,
                AsyncCache.NoCache
            );

            // Act
            var actualFeatures = await service.GetFeaturesAsync();

            // Asserts
            Assert.IsNotNull(actualFeatures);
            Assert.IsTrue(actualFeatures.ContainsKey("Workflow"), "Cannot find Workflow");
            Assert.AreEqual(false, actualFeatures["Workflow"]);
        }

        [TestMethod]
        public void FeaturesService_DefaultConstructor_InitializationPassed()
        {
            // Act
            var service = new FeaturesService();

            // Asserts
            Assert.IsNotNull(service);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "ServiceLibrary.Services.FeaturesService")]
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FeaturesService_Constructor_NullFeaturesRepository_ArgumentNullExceptionIsExpected()
        {
            var dummy = new FeaturesService(null, _licenseHelperMock.Object, AsyncCache.NoCache);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "ServiceLibrary.Services.FeaturesService")]
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FeaturesService_Constructor_NullLicenseHelper_ArgumentNullExceptionIsExpected()
        {
            var dummy = new FeaturesService(_featuresRepositoryMock.Object, null, AsyncCache.NoCache);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "ServiceLibrary.Services.FeaturesService")]
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FeaturesService_Constructor_NullAsyncCache_ArgumentNullExceptionIsExpected()
        {
            var dummy = new FeaturesService(_featuresRepositoryMock.Object, _licenseHelperMock.Object, null);
        }

        private Feature CreateFeature(string name, bool enabled = true)
        {
            return new Feature { Name = name, Enabled = enabled };
        }
    }
}
