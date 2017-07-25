using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    /// <summary>
    /// Property Change Trigger
    /// </summary>
    [XmlType("PropertyChange")]
    public class IePropertyChangeEvent : IeEvent
    {
        [XmlIgnore]
        public override EventType EventType => EventType.PropertyChange;

        [XmlElement(IsNullable = false)]
        public string PropertyName { get; set; }

        // Optional, not used for the import, will be used for the update
        [XmlElement]
        public int? PropertyId { get; set; }
        public bool ShouldSerializePropertyId() { return PropertyId.HasValue; }
    }
}