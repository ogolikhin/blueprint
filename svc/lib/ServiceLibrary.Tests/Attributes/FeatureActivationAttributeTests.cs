using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Enums;

namespace ServiceLibrary.Attributes
{
    /// <summary>
    /// Summary description for FeatureActivationAttributeTests
    /// </summary>
    [TestClass]
    public class FeatureActivationAttributeTests
    {
        private HttpActionContext _context;

        [TestInitialize]
        public void TestInitialize()
        {
            _context = HttpFilterHelper.CreateHttpActionContext(new HttpRequestMessage(HttpMethod.Get, string.Empty));
        }

        [TestMethod]
        public void FeatureActivationAttribute_InstantiatesSuccessfully()
        {
            var attribute = new FeatureActivationAttribute(FeatureTypes.Workflow);
            Assert.IsNotNull(attribute);
        }

        [TestMethod]
        public void OnActionExecuting_WhenRequiredWorkflowLicenseIsNotFound_ContentContainsWorkflowLicenseErrorCode()
        {
            //Arrange
            var requiredLicense = FeatureTypes.Workflow;
            var licenseErrorCode = ErrorCodes.WorkflowLicenseUnavailable;
            var validLicense = FeatureTypes.Storyteller;
            var attribute = CreateAttribute(validLicense, requiredLicense);

            //Act
            attribute.OnActionExecuting(_context);

            //Assert
            ValidateResponse(licenseErrorCode);
        }

        private FeatureActivationAttribute CreateAttribute(FeatureTypes validLicense, FeatureTypes requiredLicense)
        {
            var featureLicenseHelperMock = new Mock<IFeatureLicenseHelper>(MockBehavior.Strict);
            featureLicenseHelperMock.Setup(i => i.GetValidBlueprintLicenseFeatures()).Returns(validLicense);
            return new FeatureActivationAttribute(requiredLicense, featureLicenseHelperMock.Object);
        }

        private void ValidateResponse(int licenseErrorCode)
        {
            //Response should not be null
            Assert.IsNotNull(_context.Response);
            //Status Code should be Forbidden
            Assert.AreEqual(HttpStatusCode.Forbidden, _context.Response.StatusCode);
            //Content should contain the Error Code
            var contentResult = _context.Response.Content.ReadAsStringAsync().Result;
            Assert.IsTrue(contentResult.Contains(licenseErrorCode.ToStringInvariant()));
        }

        [TestMethod]
        public void OnActionExecuting_WhenRequiredStorytellerLicenseIsNotFound_ContentContainsLicenseErrorCode()
        {
            //Arrange
            var requiredLicense = FeatureTypes.Storyteller;
            var licenseErrorCode = ErrorCodes.LicenseUnavailable;
            var validLicense = FeatureTypes.Blueprint;
            var attribute = CreateAttribute(validLicense, requiredLicense);

            //Act
            attribute.OnActionExecuting(_context);

            //Assert
            ValidateResponse(licenseErrorCode);
        }

        [TestMethod]
        public void OnActionExecuting_WhenRequiredBlueprintLicenseIsNotFound_ContentContainsLicenseErrorCode()
        {
            //Arrange
            var requiredLicense = FeatureTypes.Blueprint;
            var licenseErrorCode = ErrorCodes.LicenseUnavailable;
            var validLicense = FeatureTypes.Storyteller;
            var attribute = CreateAttribute(validLicense, requiredLicense);

            //Act
            attribute.OnActionExecuting(_context);

            //Assert
            ValidateResponse(licenseErrorCode);
        }

        [TestMethod]
        public void OnActionExecuting_WhenRequiredBlueprintOpenApiLicenseIsNotFound_ContentContainsLicenseErrorCode()
        {
            //Arrange
            var requiredLicense = FeatureTypes.BlueprintOpenApi;
            var licenseErrorCode = ErrorCodes.LicenseUnavailable;
            var validLicense = FeatureTypes.Storyteller;
            var attribute = CreateAttribute(validLicense, requiredLicense);

            //Act
            attribute.OnActionExecuting(_context);

            //Assert
            ValidateResponse(licenseErrorCode);
        }

        [TestMethod]
        public void OnActionExecuting_WhenRequiredHewlettPackardQCIntegrationLicenseIsNotFound_ContentContainsLicenseErrorCode()
        {
            //Arrange
            var requiredLicense = FeatureTypes.HewlettPackardQCIntegration;
            var licenseErrorCode = ErrorCodes.LicenseUnavailable;
            var validLicense = FeatureTypes.Storyteller;
            var attribute = CreateAttribute(validLicense, requiredLicense);

            //Act
            attribute.OnActionExecuting(_context);

            //Assert
            ValidateResponse(licenseErrorCode);
        }

        [TestMethod]
        public void OnActionExecuting_WhenRequiredMicrosoftTfsIntegrationLicenseIsNotFound_ContentContainsLicenseErrorCode()
        {
            //Arrange
            var requiredLicense = FeatureTypes.MicrosoftTfsIntegration;
            var licenseErrorCode = ErrorCodes.LicenseUnavailable;
            var validLicense = FeatureTypes.Storyteller;
            var attribute = CreateAttribute(validLicense, requiredLicense);

            //Act
            attribute.OnActionExecuting(_context);

            //Assert
            ValidateResponse(licenseErrorCode);
        }

        [TestMethod]
        public void OnActionExecuting_WhenOneOfTheRequiredLicensesIsNotFound_ContentContainsLicenseErrorCode()
        {
            //Arrange
            var requiredLicense = FeatureTypes.Workflow | FeatureTypes.Storyteller;
            var licenseErrorCode = ErrorCodes.LicenseUnavailable;
            var validLicense = FeatureTypes.Workflow | FeatureTypes.Blueprint;
            var attribute = CreateAttribute(validLicense, requiredLicense);

            //Act
            attribute.OnActionExecuting(_context);

            //Assert
            ValidateResponse(licenseErrorCode);
        }

        [TestMethod]
        public void OnActionExecuting_WhenAllRequiredLicensesAreNotFound_ContentContainsLicenseErrorCode()
        {
            //Arrange
            var requiredLicense = FeatureTypes.Workflow | FeatureTypes.Storyteller;
            var licenseErrorCode = ErrorCodes.LicenseUnavailable;
            var validLicense = FeatureTypes.BlueprintOpenApi | FeatureTypes.Blueprint;
            var attribute = CreateAttribute(validLicense, requiredLicense);

            //Act
            attribute.OnActionExecuting(_context);

            //Assert
            ValidateResponse(licenseErrorCode);
        }

        [TestMethod]
        public void OnActionExecuting_WhenRequiredLicenseIsFound_ResponseIsNull()
        {
            //Arrange
            var requiredLicense = FeatureTypes.Workflow;
            var validLicense = FeatureTypes.Workflow;
            var attribute = CreateAttribute(validLicense, requiredLicense);

            //Act
            attribute.OnActionExecuting(_context);

            //Assert
            Assert.IsNull(_context.Response);
        }

        [TestMethod]
        public void OnActionExecuting_WhenAllRequiredLicensesAreFound_ResponseIsNull()
        {
            //Arrange
            var requiredLicense = FeatureTypes.Workflow | FeatureTypes.Storyteller;
            var validLicense = FeatureTypes.Workflow | FeatureTypes.Storyteller;
            var attribute = CreateAttribute(validLicense, requiredLicense);

            //Act
            attribute.OnActionExecuting(_context);

            //Assert
            Assert.IsNull(_context.Response);
        }

        [TestMethod]
        public void OnActionExecuting_WhenMoreThanAllRequiredLicensesAreFound_ResponseIsNull()
        {
            //Arrange
            var requiredLicense = FeatureTypes.Workflow | FeatureTypes.Storyteller;
            var validLicense = FeatureTypes.Workflow | FeatureTypes.Storyteller | FeatureTypes.Blueprint;
            var attribute = CreateAttribute(validLicense, requiredLicense);

            //Act
            attribute.OnActionExecuting(_context);

            //Assert
            Assert.IsNull(_context.Response);
        }

        [TestMethod]
        public void OnActionExecuting_WhenNoLicenseIsRequired_ResponseIsNull()
        {
            //Arrange
            var requiredLicense = FeatureTypes.None;
            var validLicense = FeatureTypes.Storyteller;
            var attribute = CreateAttribute(validLicense, requiredLicense);

            //Act
            attribute.OnActionExecuting(_context);

            //Assert
            Assert.IsNull(_context.Response);
        }
    }
}
