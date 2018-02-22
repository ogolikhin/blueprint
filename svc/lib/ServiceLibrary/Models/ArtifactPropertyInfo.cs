namespace ServiceLibrary.Models
{
    public class ArtifactPropertyInfo
    {
        public int ArtifactId { get; set; }

        public string Prefix { get; set; }

        public int? ItemTypeId { get; set; }

        public int? PropertyTypeId { get; set; }

        public string PropertyName { get; set; }

        public string PropertyValue { get; set; }

        public int PropertyTypePredefined { get; set; }

        public int? PrimitiveType { get; set; }

        public int? PredefinedType { get; set; }

        public int? ItemTypeIconId { get; set; }

    }
}