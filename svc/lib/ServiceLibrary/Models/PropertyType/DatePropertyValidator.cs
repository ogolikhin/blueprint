using ServiceLibrary.Helpers;
using ServiceLibrary.Helpers.Validators;

namespace ServiceLibrary.Models.PropertyType
{
    public class DatePropertyValidator : PropertyValidator<DatePropertyType>
    {
        protected override PropertySetResult Validate(
            PropertyLite property, 
            DatePropertyType propertyType, 
            IValidationContext validationContext)
        {
            if (IsPropertyValueEmpty(property, propertyType) || !propertyType.IsValidate)
                return null;

            var value = property.DateValue.Value;

            // Maximum.
            if (propertyType.IsValidate && value.CompareTo(propertyType.Range.End) > 0)
                return new PropertySetResult(property.PropertyTypeId, ErrorCodes.InvalidArtifactProperty, "Must be less than max value");

            // Minimum.
            if (propertyType.IsValidate && value.CompareTo(propertyType.Range.Start) < 0)
                return new PropertySetResult(property.PropertyTypeId, ErrorCodes.InvalidArtifactProperty, "Must be greater than min value");

            // Success.
            return null;
        }

        /// <summary>
        /// Determines whether the property value is empty.
        /// </summary>
        protected override bool IsPropertyValueEmpty(PropertyLite property, DatePropertyType propertyType)
        {
            return !property.DateValue.HasValue;
        }
    }
}
