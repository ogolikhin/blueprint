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
    }
}