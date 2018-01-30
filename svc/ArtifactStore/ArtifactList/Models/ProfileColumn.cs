using System;
using System.Collections.Generic;
using System.Linq;
using ArtifactStore.Collections.Models;
using ServiceLibrary.Models.ProjectMeta;

namespace ArtifactStore.ArtifactList.Models
{
    public class ProfileColumn
    {
        public string PropertyName { get; }

        public int? PropertyTypeId { get; }

        public PropertyTypePredefined Predefined { get; }

        public PropertyPrimitiveType PrimitiveType { get; }

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

        public bool ExistsIn(IEnumerable<PropertyTypeInfo> propertyTypeInfos)
        {
            return Predefined == PropertyTypePredefined.CustomGroup ?
                propertyTypeInfos.Any(info => info.Id == PropertyTypeId) :
                propertyTypeInfos.Any(info => info.Predefined == Predefined);
        }
    }
}
