using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    public enum EventType { None, Transition, PropertyChange }

    /// <summary>
    /// Base class for Triggers of specific type
    /// </summary>
    [XmlType("Event")]
    public abstract class IeEvent
    {
        // Defines the type of Trigger
        [XmlIgnore]
        public abstract EventType EventType { get; }

        [XmlElement(IsNullable = false)]
        public string Name { get; set; }

        [XmlElement(IsNullable = false)]
        public string Description { get; set; }
    }
}