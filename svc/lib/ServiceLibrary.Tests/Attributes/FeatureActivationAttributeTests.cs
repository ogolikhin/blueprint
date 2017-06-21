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
        public void IfRequiredLicenseIsNotFoundThenResponseCodeIsForbidden()
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
        public void IfOneOfTheRequiredLicensesIsNotFoundThenResponseCodeIsForbidden()
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
        public void IfAllRequiredLicensesAreNotFoundThenResponseCodeIsForbidden()
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
        public void IfRequiredLicenseIsFoundThenResponseIsNull()
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
        public void IfAllRequiredLicensesAreFoundThenResponseIsNull()
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
        public void IfMoreThanAllRequiredLicensesAreFoundThenResponseIsNull()
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
