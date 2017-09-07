using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Reuse;
using ServiceLibrary.Models.Workflow;

namespace ServiceLibrary.Helpers.Validators
{
    [TestClass]
    public class ReusePropertyValidatorTests
    {
        private IReusePropertyValidator _reusePropertyValidator;
        [TestInitialize]
        public void TestInitialize()
        {
            _reusePropertyValidator = new ReusePropertyValidator();
        }
        [TestMethod]
        public void ValidateReuseSettings_ValidateReadonlyNameUpdating_Fails()
        {
            var namePropertyId = WorkflowConstants.PropertyTypeFakeIdName;
            var fakeItemTypeReuseTempalate = new ItemTypeReuseTemplate
            {
                ReadOnlySettings = ItemTypeReuseTemplateSetting.Name
            };
            var result = _reusePropertyValidator.ValidateReuseSettings(namePropertyId, fakeItemTypeReuseTempalate);

            Assert.IsNotNull(result);
        }
        [TestMethod]
        public void ValidateReuseSettings_ValidateReadonlyDescriptionNameUpdating_Fails()
        {
            var namePropertyId = WorkflowConstants.PropertyTypeFakeIdDescription;
            var fakeItemTypeReuseTempalate = new ItemTypeReuseTemplate
            {
                ReadOnlySettings = ItemTypeReuseTemplateSetting.Description
            };
            var result = _reusePropertyValidator.ValidateReuseSettings(namePropertyId, fakeItemTypeReuseTempalate);

            Assert.IsNotNull(result);
        }
        [TestMethod]
        public void ValidateReuseSettings_ValidateReadonlyCustomPropertyUpdating_Fails()
        {
            var fakePropertyId = 123;
            var fakeItemTypeReuseTempalate = new ItemTypeReuseTemplate();
            var propertyTypeReuseTemplate = new PropertyTypeReuseTemplate
            {
                PropertyTypeId = fakePropertyId,
                Settings = PropertyTypeReuseTemplateSettings.ReadOnly
            };
            fakeItemTypeReuseTempalate.PropertyTypeReuseTemplates.Add(fakePropertyId, propertyTypeReuseTemplate);

            var result = _reusePropertyValidator.ValidateReuseSettings(fakePropertyId, fakeItemTypeReuseTempalate);

            Assert.IsNotNull(result);
        }
    }
}
