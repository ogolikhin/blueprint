using ServiceLibrary.Models.ProjectMeta;

namespace ArtifactStore.Models.Reuse
{
    public class SqlPropertyTypeInfo
    {
        public int PropertyTypeId { get; set; }

        public int? InstancePropertyTypeId { get; set; }

        public PropertyPrimitiveType PrimitiveType { get; set; }

        public PropertyTypePredefined PropertyTypePredefined { get; set; }
    }
}