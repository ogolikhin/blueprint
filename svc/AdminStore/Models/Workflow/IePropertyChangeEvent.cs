using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("Triggers")]
        [XmlArrayItem("PropertyChangeTrigger", typeof(IePropertyChangeTrigger))]
        public List<IeTrigger> Triggers { get; set; }
    }
}