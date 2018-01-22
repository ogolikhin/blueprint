using ServiceLibrary.Models.ProjectMeta;

namespace ArtifactStore.Collections.Models
{
    public class PropertyTypeInfo
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public PropertyTypePredefined Predefined { get; set; }

        public PropertyPrimitiveType PrimitiveType { get; set; }
    }
}