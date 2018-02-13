using System;
using System.Collections.Generic;
using System.Linq;
using ArtifactStore.Collections.Models;
using Newtonsoft.Json;
using ServiceLibrary.Models.ProjectMeta;

namespace ArtifactStore.ArtifactList.Models
{
    public class ProfileColumn
    {
        public string PropertyName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? PropertyTypeId { get; set; }

        public PropertyTypePredefined Predefined { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public PropertyPrimitiveType PrimitiveType { get; set; }

        public ProfileColumn()
        {
        }

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
            return propertyTypeInfos.Any(info => (info.Name == PropertyName) &&
                                                 (info.Predefined == Predefined) &&
                                                 (info.PrimitiveType == PrimitiveType) &&
                                                 (info.Id == PropertyTypeId));
        }
    }
}