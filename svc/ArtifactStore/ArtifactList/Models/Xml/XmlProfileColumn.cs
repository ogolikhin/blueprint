namespace ArtifactStore.ArtifactList.Models.Xml
{
    public class XmlProfileColumn
    {
        public string PropertyName { get; set; }

        public int? PropertyTypeId { get; set; }

        public int Predefined { get; set; }

        public int PrimitiveType { get; set; }

        public bool ShouldSerializePropertyTypeId()
        {
            return PropertyTypeId.HasValue;
        }
    }
}