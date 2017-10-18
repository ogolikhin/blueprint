using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.Reuse;
using ServiceLibrary.Models.Workflow;

namespace ServiceLibrary.Helpers.Validators
{
    public interface IReusePropertyValidator
    {
        PropertySetResult ValidateReuseSettings(
            int propertyTypeId,
            ItemTypeReuseTemplate reuseTemplateSettings);
    }
    public class ReusePropertyValidator : IReusePropertyValidator
    {
        public PropertySetResult ValidateReuseSettings(
            int propertyTypeId,
            ItemTypeReuseTemplate reuseTemplateSettings)
        {
            if (reuseTemplateSettings == null)
            {
                return null;
            }

            if (reuseTemplateSettings.ReadOnlySettings.HasFlag(ItemTypeReuseTemplateSetting.Name) &&
                propertyTypeId == WorkflowConstants.PropertyTypeFakeIdName)
            {
                return new PropertySetResult(propertyTypeId, ErrorCodes.InvalidArtifactProperty, "Cannot modify name from workflow event action. Property is readonly.");
            }

            if (reuseTemplateSettings.ReadOnlySettings.HasFlag(ItemTypeReuseTemplateSetting.Description) &&
                propertyTypeId == WorkflowConstants.PropertyTypeFakeIdDescription)
            {
                return new PropertySetResult(propertyTypeId, ErrorCodes.InvalidArtifactProperty, "Cannot modify description from workflow event action. Property is readonly.");
            }

            var customProperty = reuseTemplateSettings.PropertyTypeReuseTemplates[propertyTypeId];

            var propertyReusetemplate = reuseTemplateSettings.PropertyTypeReuseTemplates[customProperty.PropertyTypeId];
            if (propertyReusetemplate.Settings == PropertyTypeReuseTemplateSettings.ReadOnly)
            {
                return new PropertySetResult(propertyTypeId, ErrorCodes.InvalidArtifactProperty, "Cannot modify property from workflow event action. Property is readonly.");
            }
            return null;
        }
    }
}