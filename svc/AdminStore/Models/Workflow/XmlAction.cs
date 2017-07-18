using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    [XmlType("A")]
    public abstract class XmlAction
    {
        [XmlElement("N", IsNullable = false)]
        public string Name { get; set; }

        [XmlElement("D", IsNullable = false)]
        public string Description { get; set; }
    }
}