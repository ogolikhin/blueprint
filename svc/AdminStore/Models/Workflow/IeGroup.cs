using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    [XmlType("Group")]
    public class IeGroup
    {
        [XmlElement(IsNullable = false)]
        public string Name { get; set; }
    }
}