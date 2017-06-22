using System.Net;
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
        private Mock<IFeatureLicenseHelper> _featureLicenseHelperMock;

        [TestInitialize]
        public void TestInitialize()
        {
            _featureLicenseHelperMock = new Mock<IFeatureLicenseHelper>(MockBehavior.Strict);
        }

        [TestMethod]
        public void OnActionExecuting_WhenRequiredLicenseIsNotFound_ResponseCodeIsForbidden()
        {
            //Arrange
            var requiredLicense = FeatureTypes.Workflow;
            var validLicense = FeatureTypes.Storyteller;
            _featureLicenseHelperMock.Setup(i => i.GetValidBlueprintLicenseFeatures()).Returns(validLicense);
            var attribute = new FeatureActivationAttribute(requiredLicense, _featureLicenseHelperMock.Object);
            var context = new HttpActionContext();
            //Act
            attribute.OnActionExecuting(context);
            //Assert
            Assert.IsNotNull(context.Response);
            Assert.AreEqual(HttpStatusCode.Forbidden, context.Response.StatusCode);
        }

        [TestMethod]
        public void OnActionExecuting_WhenOneOfTheRequiredLicensesIsNotFound_ResponseCodeIsForbidden()
        {
            //Arrange
            var requiredLicenses = FeatureTypes.Workflow | FeatureTypes.Storyteller;
            var validLicenses = FeatureTypes.Workflow | FeatureTypes.Blueprint;
            _featureLicenseHelperMock.Setup(i => i.GetValidBlueprintLicenseFeatures()).Returns(validLicenses);
            var attribute = new FeatureActivationAttribute(requiredLicenses, _featureLicenseHelperMock.Object);
            var context = new HttpActionContext();
            //Act
            attribute.OnActionExecuting(context);
            //Assert
            Assert.IsNotNull(context.Response);
            Assert.AreEqual(HttpStatusCode.Forbidden, context.Response.StatusCode);
        }

        [TestMethod]
        public void OnActionExecuting_WhenAllRequiredLicensesAreNotFound_ResponseCodeIsForbidden()
        {
            //Arrange
            var requiredLicenses = FeatureTypes.Workflow | FeatureTypes.Storyteller;
            var validLicenses = FeatureTypes.BlueprintOpenApi | FeatureTypes.Blueprint;
            _featureLicenseHelperMock.Setup(i => i.GetValidBlueprintLicenseFeatures()).Returns(validLicenses);
            var attribute = new FeatureActivationAttribute(requiredLicenses, _featureLicenseHelperMock.Object);
            var context = new HttpActionContext();
            //Act
            attribute.OnActionExecuting(context);
            //Assert
            Assert.IsNotNull(context.Response);
            Assert.AreEqual(HttpStatusCode.Forbidden, context.Response.StatusCode);
        }

        [TestMethod]
        public void OnActionExecuting_WhenRequiredLicenseIsFound_ResponseIsNull()
        {
            //Arrange
            var requiredLicense = FeatureTypes.Workflow;
            var validLicense = FeatureTypes.Workflow;
            _featureLicenseHelperMock.Setup(i => i.GetValidBlueprintLicenseFeatures()).Returns(validLicense);
            var attribute = new FeatureActivationAttribute(requiredLicense, _featureLicenseHelperMock.Object);
            var context = new HttpActionContext();
            //Act
            attribute.OnActionExecuting(context);
            //Assert
            Assert.IsNull(context.Response);
        }

        [TestMethod]
        public void OnActionExecuting_WhenAllRequiredLicensesAreFound_ResponseIsNull()
        {
            //Arrange
            var requiredLicenses = FeatureTypes.Workflow | FeatureTypes.Storyteller;
            var validLicenses = FeatureTypes.Workflow | FeatureTypes.Storyteller;
            _featureLicenseHelperMock.Setup(i => i.GetValidBlueprintLicenseFeatures()).Returns(validLicenses);
            var attribute = new FeatureActivationAttribute(requiredLicenses, _featureLicenseHelperMock.Object);
            var context = new HttpActionContext();
            //Act
            attribute.OnActionExecuting(context);
            //Assert
            Assert.IsNull(context.Response);
        }

        [TestMethod]
        public void OnActionExecuting_WhenMoreThanAllRequiredLicensesAreFound_ResponseIsNull()
        {
            //Arrange
            var requiredLicenses = FeatureTypes.Workflow | FeatureTypes.Storyteller;
            var validLicenses = FeatureTypes.Workflow | FeatureTypes.Storyteller | FeatureTypes.Blueprint;
            _featureLicenseHelperMock.Setup(i => i.GetValidBlueprintLicenseFeatures()).Returns(validLicenses);
            var attribute = new FeatureActivationAttribute(requiredLicenses, _featureLicenseHelperMock.Object);
            var context = new HttpActionContext();
            //Act
            attribute.OnActionExecuting(context);
            //Assert
            Assert.IsNull(context.Response);
        }
    }
}
