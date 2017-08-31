using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Helpers.Validators;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Models.PropertyType
{
    public class TextPropertyValidator : PropertyValidator<TextPropertyType>
    {
        /// <summary>
        /// Validates the specified property.
        /// </summary>
        protected override PropertySetResult Validate(PropertyLite property, TextPropertyType propertyType, IValidationContext validationContext)
        {
            if (property.ChoiceIds != null)
            {
                return null;
            }

            if (property.TextOrChoiceValue == null && propertyType.Predefined == PropertyTypePredefined.Name)
            {
                return new PropertySetResult(property.PropertyTypeId, ErrorCodes.InvalidArtifactProperty, "Property 'Name' value is required");
            }

            // NOTE: No validation is provided for propertyType.IsRichText, propertyType.AllowMultiple
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