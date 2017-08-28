using ServiceLibrary.Exceptions;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Reuse;
using ServiceLibrary.Models.Workflow;

namespace ServiceLibrary.Helpers.Validators
{
    public interface IReusePropertyValidator
    {
        void ValidateReuseSettings(
            int propertyTypeId,
            ItemTypeReuseTemplate reuseTemplateSettings);
    }
    public class ReusePropertyValidator: IReusePropertyValidator
    {
        public void ValidateReuseSettings(
            int propertyTypeId, 
            ItemTypeReuseTemplate reuseTemplateSettings)
        {
            if (reuseTemplateSettings == null)
            {
                return;
            }

            if (reuseTemplateSettings.ReadOnlySettings.HasFlag(ItemTypeReuseTemplateSetting.Name) &&
                propertyTypeId == WorkflowConstants.PropertyTypeFakeIdName)
            {
                throw new ConflictException("Cannot modify name from workflow event action. Property is readonly.");
            }

            if (reuseTemplateSettings.ReadOnlySettings.HasFlag(ItemTypeReuseTemplateSetting.Description) &&
                propertyTypeId == WorkflowConstants.PropertyTypeFakeIdDescription)
            {
                throw new ConflictException("Cannot modify description from workflow event action. Property is readonly.");
            }

            var customProperty = reuseTemplateSettings.PropertyTypeReuseTemplates[propertyTypeId];

            var propertyReusetemplate = reuseTemplateSettings.PropertyTypeReuseTemplates[customProperty.PropertyTypeId];
            if (propertyReusetemplate.Settings == PropertyTypeReuseTemplateSettings.ReadOnly)
            {
                throw new ConflictException("Cannot modify property from workflow event action. Property is readonly.");
            }
        }
    }
}