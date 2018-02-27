namespace ServiceLibrary.Models
{
    public class ArtifactPropertyInfo
    {
        public int ArtifactId { get; set; }

        public DateTime? DateTimeValue { get; set; }

        public decimal? DecimalValue { get; set; }

        public string FullTextValue { get; set; }

        public int? ItemTypeIconId { get; set; }

        public int? ItemTypeId { get; set; }

        public string PredefinedPropertyValue { get; set; }

        public int? PredefinedType { get; set; }

        public string Prefix { get; set; }

        public int PrimitiveType { get; set; }

        public int? PrimitiveItemTypePredefined { get; set; }

        public int? PropertyTypeId { get; set; }

        public string PropertyName { get; set; }

        public int PropertyTypePredefined { get; set; }

        public int? ValueId { get; set; }

    }
}