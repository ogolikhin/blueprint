namespace ArtifactStore.Collections.Models
{
    public class CollectionArtifact
    {
        public int ArtifactId { get; set; }

        public string Prefix { get; set; }

        public int? ItemTypeId { get; set; }

        public int? PropertyTypeId { get; set; }

        public string PropertyName { get; set; }

        public string PropertyValue { get; set; }

        public int PropertyTypePredefined { get; set; }

    }
}