using System;
using System.Collections.Generic;
using System.Linq;
using ArtifactStore.Collections.Models;
using Newtonsoft.Json;
using ServiceLibrary.Models.ProjectMeta;

namespace ArtifactStore.ArtifactList.Models
{
    public class ProfileColumn
        : EqualityComparer<ProfileColumn>, // EqualityComparer<T>.Default is used in Join method.
        IEquatable<ProfileColumn> // IEquatable<T> is used by EqualityComparer<T>.Default property.
    {
        public string PropertyName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? PropertyTypeId { get; set; }

        public PropertyTypePredefined Predefined { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public PropertyPrimitiveType PrimitiveType { get; set; }

        public ProfileColumn() { }

        public ProfileColumn(
            string propertyName,
            PropertyTypePredefined predefined,
            PropertyPrimitiveType primitiveType,
            int? propertyTypeId = null)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (propertyTypeId.HasValue && propertyTypeId.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(propertyTypeId));
            }

            if (primitiveType == PropertyPrimitiveType.Image)
            {
                throw new ArgumentException("Image columns are currently not supported.");
            }

            PropertyName = propertyName;
            PropertyTypeId = propertyTypeId;
            Predefined = predefined;
            PrimitiveType = primitiveType;
        }

        public bool NameMatches(string search)
        {
            return string.IsNullOrEmpty(search) ||
                   PropertyName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public override bool Equals(object value)
        {
            if (value == null)
                return false;

            if (object.ReferenceEquals(this, value))
                return true;

            if (this.GetType() != value.GetType())
                return false;

            return Equals((ProfileColumn)value);
        }

        public override bool Equals(ProfileColumn x, ProfileColumn y)
        {
            return (x == null && y == null) || x.Equals(y);
        }

        public bool Equals(ProfileColumn value)
        {
            if (value == null)
                return false;

            if (ReferenceEquals(this, value))
                return true;

            return
                this.PropertyName == value.PropertyName &&
                this.Predefined == value.Predefined &&
                this.PrimitiveType == value.PrimitiveType &&
                this.PropertyTypeId == value.PropertyTypeId;
        }

        public override int GetHashCode()
        {
            return new { PropertyName, Predefined, PrimitiveType, PropertyTypeId }.GetHashCode();
        }

        public override int GetHashCode(ProfileColumn obj)
        {
            return obj.GetHashCode();
        }

        public bool ExistsIn(IEnumerable<PropertyTypeInfo> propertyTypeInfos)
        {
            return Predefined == PropertyTypePredefined.CustomGroup
                ? propertyTypeInfos.Any(info => info.Id == PropertyTypeId)
                : propertyTypeInfos.Any(info => info.Predefined == Predefined);
        }
    }
}