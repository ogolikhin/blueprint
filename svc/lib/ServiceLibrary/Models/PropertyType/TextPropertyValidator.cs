using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Helpers.Validators;

namespace ServiceLibrary.Models.PropertyType
{
    public class TextPropertyValidator : PropertyValidator<TextPropertyType>
    {
        /// <summary>
        /// Validates the specified property.
        /// </summary>
        protected override PropertySetResult Validate(PropertyLite property, TextPropertyType propertyType, IValidationContext validationContext)
        {
            if (property.TextOrChoiceValue == null || property.Choices != null)
            {
                return null; // Ignore
            }

            // NOTE: No validation is provided for propertyType.IsRichText, propertyType.AllowMultiple
            //       and propertyType.Predefined

            return null; // Success
        }

        /// <summary>
        /// Determines whether the property value is empty.
        /// </summary>
        protected override bool IsPropertyValueEmpty(PropertyLite property, TextPropertyType propertyType)
        {
            return string.IsNullOrEmpty(property.TextOrChoiceValue);
        }
    }
}