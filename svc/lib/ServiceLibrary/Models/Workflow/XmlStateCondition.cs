using System.Xml.Serialization;

namespace ServiceLibrary.Models.Workflow
{
    [XmlType("SC")]
    public class XmlStateCondition : XmlCondition
    {
        [XmlElement("SID")]
        public int StateId { get; set; }
    }
}