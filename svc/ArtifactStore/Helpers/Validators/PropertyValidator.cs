using System.Collections.Generic;
using System.Linq;
using ArtifactStore.Models.PropertyTypes;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.PropertyType;

namespace ArtifactStore.Helpers.Validators
{
    public interface IPropertyValidator
    {
        PropertySetResult Validate(PropertyLite property, List<DPropertyType> propertyTypes);
    }
    public abstract class PropertyValidator<T> : IPropertyValidator where T : DPropertyType
    {
        #region Virtual and abstract methods

        /// <summary>
        /// Validates the specified property.
        /// </summary>
        public virtual PropertySetResult Validate(PropertyLite property, List<DPropertyType> propertyTypes)
        {
            if (propertyTypes.All(a => a.InstancePropertyTypeId.Value != property.PropertyTypeId))
                return null;

            var saveType = propertyTypes.FirstOrDefault(a => a.InstancePropertyTypeId.Value == property.PropertyTypeId) as T;
            if (saveType == null)
                return null;

            var commonError = ValidateValueCommon(property, saveType);
            if (commonError != null)
                return commonError;

            return Validate(property, saveType);
        }

        /// <summary>
        /// Validates the specified property.
        /// </summary>
        protected abstract PropertySetResult Validate(PropertyLite property, T propertyType);

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
                return new PropertySetResult(propertyType.PropertyTypeId, ErrorCodes.InvalidArtifactProperty, "value cannot be empty");
            }
            return null;
        }

        #endregion
    }

    //TODO: If after we implemented all validators and this is not required, delete
    //public class ValidationContext
    //{
    //    #region Properties

    //    /// <summary>
    //    /// Gets all available users.
    //    /// </summary>
    //    public IEnumerable<DUser> Users { get; private set; }

    //    /// <summary>
    //    /// Gets all available groups.
    //    /// </summary>
    //    public IEnumerable<DGroup> Groups { get; private set; }

    //    /// <summary>
    //    /// Gets the property types map.
    //    /// </summary>
    //    public IReadOnlyDictionary<int, DPropertyType> PropertyTypesMap { get; private set; }

    //    /// <summary>
    //    /// Gets the fake property types map.
    //    /// </summary>
    //    public IReadOnlyDictionary<int, DPropertyType> FakePropertyTypesMap { get; private set; }

    //    /// <summary>
    //    /// Reuse Settings Template
    //    /// </summary>
    //    public ReuseContext ReuseContext { get; private set; }

    //    #endregion

    //    #region Constrcution

    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="ValidationContext"/> class.
    //    /// </summary>
    //    public ValidationContext(IEnumerable<UserGroup> usersAndGroups, IDictionary<int, DPropertyType> propertyTypesMap, IDictionary<int, DPropertyType> fakePropertyTypesMap, ReuseContext reuseContext = null)
    //    {
    //        Users = users.ToList().AsReadOnly();
    //        Groups = groups.ToList().AsReadOnly();
    //        PropertyTypesMap = new ReadOnlyDictionary<int, DPropertyType>(propertyTypesMap);
    //        FakePropertyTypesMap = new ReadOnlyDictionary<int, DPropertyType>(fakePropertyTypesMap);
    //        ReuseContext = reuseContext;
    //    }

    //    #endregion
    //}
}