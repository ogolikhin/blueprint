using System;
using Newtonsoft.Json;
using ServiceLibrary.Models.ProjectMeta;

namespace ArtifactStore.Collections.Models
{
    public class ArtifactListColumn
    {
        public string PropertyName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? PropertyTypeId { get; set; }

        public int Predefined { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? PrimitiveType { get; set; }

        public ArtifactListColumn() { }

        public ArtifactListColumn(
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
            Predefined = (int)predefined;
            PrimitiveType = (int)primitiveType;
        }

        public bool NameMatches(string search)
        {
            return string.IsNullOrEmpty(search) ||
                   PropertyName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
