using ServiceLibrary.Models.ProjectMeta;

namespace ServiceLibrary.Models.Reuse
{
    public class SqlPropertyTypeInfo
    {
        public int PropertyTypeId { get; set; }

        public int? InstancePropertyTypeId { get; set; }

        public PropertyPrimitiveType PrimitiveType { get; set; }

        public PropertyTypePredefined PropertyTypePredefined { get; set; }
    }
}