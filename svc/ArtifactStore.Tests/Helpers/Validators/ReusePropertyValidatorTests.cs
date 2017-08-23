using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers.Validators;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Reuse;
using ServiceLibrary.Models.Workflow;

namespace ArtifactStore.Helpers.Validators
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
        [ExpectedException(typeof(ConflictException))]
        public void ValidateReuseSettings_ValidateReadonlyNameUpdating_Fails()
        {
            var namePropertyId = WorkflowConstants.PropertyTypeFakeIdName;
            var fakeItemTypeReuseTempalate = new ItemTypeReuseTemplate
            {
                ReadOnlySettings = ItemTypeReuseTemplateSetting.Name
            };
            _reusePropertyValidator.ValidateReuseSettings(namePropertyId, fakeItemTypeReuseTempalate);
        }
        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public void ValidateReuseSettings_ValidateReadonlyDescriptionNameUpdating_Fails()
        {
            var namePropertyId = WorkflowConstants.PropertyTypeFakeIdDescription;
            var fakeItemTypeReuseTempalate = new ItemTypeReuseTemplate
            {
                ReadOnlySettings = ItemTypeReuseTemplateSetting.Description
            };
            _reusePropertyValidator.ValidateReuseSettings(namePropertyId, fakeItemTypeReuseTempalate);
        }
        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
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

            _reusePropertyValidator.ValidateReuseSettings(fakePropertyId, fakeItemTypeReuseTempalate);
        }
    }
}
