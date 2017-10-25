using System.Xml.Serialization;

namespace ServiceLibrary.Models.Workflow
{
    [XmlType("T")]
    public class XmlWorkflowEventTrigger
    {
        [XmlElement("N", IsNullable = false)]
        public string Name { get; set; }

        [XmlElement(typeof(XmlEmailNotificationAction), ElementName = "AEN")]
        [XmlElement(typeof(XmlGenerateAction), ElementName = "AG")]
        [XmlElement(typeof(XmlPropertyChangeAction), ElementName = "APC")]
        public XmlAction Action { get; set; }

        [XmlElement(typeof(XmlStateCondition), ElementName = "SC")]
        public XmlCondition Condition { get; set; }
    }
}