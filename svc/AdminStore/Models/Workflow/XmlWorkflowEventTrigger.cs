using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    [XmlRoot("T")]
    public class XmlWorkflowEventTrigger
    {
        [XmlElement("N", IsNullable = false)]
        public string Name { get; set; }

        [XmlElement("D", IsNullable = false)]
        public string Description { get; set; }

        [XmlElement(typeof (XmlEmailNotificationAction), ElementName = "AEN")]
        [XmlElement(typeof (XmlGenerateAction), ElementName = "AG")]
        [XmlElement(typeof (XmlPropertyChangeAction), ElementName = "APC")]
        public XmlAction Action { get; set; }
    }
}