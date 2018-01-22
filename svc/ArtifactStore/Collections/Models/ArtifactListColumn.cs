using Newtonsoft.Json;
using ServiceLibrary.Models.ProjectMeta;

namespace ArtifactStore.Collections.Models
{
    public class ArtifactListColumn
    {
        public ArtifactListColumn()
        {
        }

        public ArtifactListColumn(string name, PropertyTypePredefined predefined, PropertyPrimitiveType primitiveType)
        {
            PropertyName = name;
            Predefined = (int)predefined;
            PrimitiveType = (int)primitiveType;
        }

        public string PropertyName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? PropertyTypeId { get; set; }

        public int Predefined { get; set; }

        public int PrimitiveType { get; set; }
    }
}