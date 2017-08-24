using ArtifactStore.Models.PropertyTypes;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.PropertyType;

namespace ArtifactStore.Helpers.Validators
{
    public class DatePropertyValidator : PropertyValidator<DDatePropertyType>
    {
        protected override PropertySetResult Validate(PropertyLite property, DDatePropertyType propertyType)
        {
            if (IsPropertyValueEmpty(property, propertyType))
                return null;

            var value = property.DateValue.Value;

            //Maximum.
            if (propertyType.IsValidate && value.CompareTo(propertyType.Range.End) > 0)
                return new PropertySetResult(property.PropertyTypeId, ErrorCodes.InvalidArtifactProperty, "Must be less than max value");

            //Minimum.
            if (propertyType.IsValidate && value.CompareTo(propertyType.Range.Start) < 0)
                return new PropertySetResult(property.PropertyTypeId, ErrorCodes.InvalidArtifactProperty, "Must be greater than min value");

            //Success.
            return null;
        }

        /// <summary>
        /// Determines whether the property value is empty.
        /// </summary>
        protected override bool IsPropertyValueEmpty(PropertyLite property, DDatePropertyType propertyType)
        {
            return !property.DateValue.HasValue;
        }
    }
}
