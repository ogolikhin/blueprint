using System.Xml.Serialization;

namespace ServiceLibrary.Models.Workflow
{
    [XmlType("A")]
    public abstract class XmlAction
    {
        [XmlElement("N", IsNullable = false)]
        public string Name { get; set; }
    }
}