using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    [XmlType("PropertyChangeTrigger")]
    public class IePropertyChangeTrigger : IeTrigger
    {
        [XmlIgnore]
        public override TriggerType TriggerType => TriggerType.PropertyChange;

        [XmlElement(typeof(IeEmailNotificationAction), ElementName = "EmailNotificationAction")]
        public IeBaseAction Action { get; set; }
    }
}