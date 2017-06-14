using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    [XmlType("ArtifactType")]
    public class IeArtifactType
    {
        [XmlElement(IsNullable = false)]
        public string Name { get; set; }
    }
}