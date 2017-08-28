using System.Collections.Generic;
using System.Linq;
using ServiceLibrary.Helpers;
using ServiceLibrary.Helpers.Validators;

namespace ServiceLibrary.Models.PropertyType
{
    public abstract class PropertyValidator<T> : IPropertyValidator where T : DPropertyType
    {
        #region Virtual and abstract methods

        /// <summary>
        /// Validates the specified property.
        /// </summary>
        public virtual PropertySetResult Validate(PropertyLite property, List<DPropertyType> propertyTypes, IValidationContext validationContext)
        {
            if (propertyTypes.All(a => a.InstancePropertyTypeId.Value != property.PropertyTypeId))
                return null;

            var saveType =
                propertyTypes.FirstOrDefault(a => a.InstancePropertyTypeId.Value == property.PropertyTypeId) as T;
            if (saveType == null)
                return null;

            var commonError = ValidateValueCommon(property, saveType);
            if (commonError != null)
                return commonError;

            return Validate(property, saveType, validationContext);
        }

        /// <summary>
        /// Validates the specified property.
        /// </summary>
        protected abstract PropertySetResult Validate(PropertyLite property, T propertyType, IValidationContext validationContext);

        /// <summary>
        /// Determines whether the property value is empty.
        /// </summary>
        protected abstract bool IsPropertyValueEmpty(PropertyLite property, T propertyType);

        #endregion

        #region Helpers

        private PropertySetResult ValidateValueCommon(PropertyLite property, T propertyType)
        {
            //if (propertyType == VersionableState.Removed)
            //{
            //    return new PropertySetResult(propertyType.Id, BusinessLayerErrorCodes.InvalidArtifactPropertyType, StringTokens.ArtifactDataProvider_ThePropertyTypeHasBeenRemoved);
            //}
            if (propertyType.IsRequired && IsPropertyValueEmpty(property, propertyType))
            {
                return new PropertySetResult(propertyType.PropertyTypeId, ErrorCodes.InvalidArtifactProperty,
                    "value cannot be empty");
            }
            return null;
        }

        #endregion
    }
}