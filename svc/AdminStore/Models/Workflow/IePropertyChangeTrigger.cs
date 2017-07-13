using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    /// <summary>
    /// Property Change Trigger
    /// </summary>
    [XmlType("PropertyChangeTrigger")]
    public class IePropertyChangeTrigger : IeTrigger
    {
        [XmlIgnore]
        public override TriggerTypes TriggerType => TriggerTypes.PropertyChange;

        [XmlElement(IsNullable = false)]
        public string PropertyName { get; set; }
    }
}