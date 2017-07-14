using System.Xml.Serialization;

namespace AdminStore.Models.Workflow { 

    [XmlType("Trigger")]
    public class IeTrigger
    {

        [XmlElement(IsNullable = false)]
        public string Name { get; set; }

        [XmlElement(IsNullable = false)]
        public string Description { get; set; }

        [XmlElement(typeof(IeEmailNotificationAction), ElementName = "EmailNotificationAction")]
        [XmlElement(typeof(IeGenerateAction), ElementName = "GenerateAction")]
        [XmlElement(typeof(IePropertyChangeAction), ElementName = "PropertyChangeAction")]
        public IeBaseAction Action { get; set; }
    }
}