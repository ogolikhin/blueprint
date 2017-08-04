using System.Xml.Serialization;

namespace ServiceLibrary.Models.Workflow
{
    [XmlType("SC")]
    public class XmlStateCondition : XmlCondition
    {
        [XmlIgnore]
        public override ConditionTypes ConditionType => ConditionTypes.State;

        [XmlElement("SID")]
        public int StateId { get; set; }
    }
}