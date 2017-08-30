using System;
using ServiceLibrary.Helpers;
using ServiceLibrary.Helpers.Validators;
using System.Linq;

namespace ServiceLibrary.Models.PropertyType
{
    public class ChoicePropertyValidator : PropertyValidator<ChoicePropertyType>
    {
        protected override PropertySetResult Validate(
            PropertyLite property,
            ChoicePropertyType propertyType,
            IValidationContext validationContext)
        {
            if (!String.IsNullOrEmpty(property.TextOrChoiceValue) && property.ChoiceIds.Count != 0)
                return new PropertySetResult(property.PropertyTypeId, ErrorCodes.InvalidArtifactProperty, "Custom value and choices cannot be specified simultaneously.");

            if (propertyType.IsValidate && !String.IsNullOrEmpty(property.TextOrChoiceValue))
                return new PropertySetResult(property.PropertyTypeId, ErrorCodes.InvalidArtifactProperty, "Property does not support custom values.");

            if (propertyType.AllowMultiple != true && property.ChoiceIds.Count > 1)
                return new PropertySetResult(property.PropertyTypeId, ErrorCodes.InvalidArtifactProperty, "Only single choice is allowed.");

            var items = propertyType.ValidValues;
            var hasInvalidChoice = property.ChoiceIds.Any(c => items.All(i => i.Sid != c));
            if (hasInvalidChoice)
                return new PropertySetResult(property.PropertyTypeId, ErrorCodes.InvalidArtifactProperty, "Specified choice value does not exist.");

            //Success.
            return null;
        }

        /// <summary>
        /// Determines whether the property value is empty.
        /// </summary>
        protected override bool IsPropertyValueEmpty(PropertyLite property, ChoicePropertyType propertyType)
        {
            var hasCustomValue = !propertyType.IsValidate && !String.IsNullOrEmpty(property.TextOrChoiceValue);
            return !hasCustomValue && !property.ChoiceIds.Any();
        }
    }
}