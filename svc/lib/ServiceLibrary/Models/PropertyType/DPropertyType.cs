using ServiceLibrary.Models.ProjectMeta;

namespace ServiceLibrary.Models.PropertyType
{
    //TODO: remove properties from this class as we create the new ones
    public class DPropertyType
    {
        public int PropertyTypeId { get; set; }
        public int? VersionId { get; set; }
        public string Name { get; set; }
        public PropertyPrimitiveType? PrimitiveType { get; set; }
        public PropertyTypePredefined Predefined { get; set; }
        public int? InstancePropertyTypeId { get; set; }
        public bool? IsRichText { get; set; }
        public string StringDefaultValue { get; set; }
        public bool? AllowMultiple { get; set; }
        public bool IsRequired { get; set; }
        public bool? Validate { get; set; }
        public int? DefaultValidValueId { get; set; }
    }
}
