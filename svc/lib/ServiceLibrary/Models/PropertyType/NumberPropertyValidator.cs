using System;
using System.Globalization;
using ServiceLibrary.Helpers;
using ServiceLibrary.Helpers.Validators;

namespace ServiceLibrary.Models.PropertyType
{
    public class NumberPropertyValidator : PropertyValidator<DNumberPropertyType>
    {
        protected override PropertySetResult Validate(PropertyLite property, DNumberPropertyType propertyType, IValidationContext validationContext)
        {
            decimal value = property.NumberValue.Value;
            if (IsPropertyValueEmpty(property, propertyType))
                return null;

            //Maximum.
            if (propertyType.IsValidate && value.CompareTo(propertyType.Range.End) > 0)
                return new PropertySetResult(property.PropertyTypeId, ErrorCodes.InvalidArtifactProperty, "Must be less than max value");

            //Minimum.
            if (propertyType.IsValidate && value.CompareTo(propertyType.Range.Start) < 0)
                return new PropertySetResult(property.PropertyTypeId, ErrorCodes.InvalidArtifactProperty, "Must be greater than min value");

            //Decimal places.
            var stringValue = value.ToString("G29", CultureInfo.CurrentCulture);
            var i = stringValue.IndexOf(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator, StringComparison.OrdinalIgnoreCase);
            var decimalPlaces = (i == -1) ? 0 : stringValue.Length - i - 1;
            if (decimalPlaces > propertyType.DecimalPlaces)
                return new PropertySetResult(property.PropertyTypeId, ErrorCodes.InvalidArtifactProperty, "Decimal places greater than maximum configured");

            //Success.
            return null;
        }

        /// <summary>
        /// Determines whether the property value is empty.
        /// </summary>
        protected override bool IsPropertyValueEmpty(PropertyLite property, DNumberPropertyType propertyType)
        {
            return !property.NumberValue.HasValue;
        }
    }
}