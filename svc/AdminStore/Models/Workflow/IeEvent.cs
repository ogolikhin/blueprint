using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    public enum EventType { None, Transition, PropertyChange, NewArtifact }

    /// <summary>
    /// Base class for Triggers of specific type
    /// </summary>
    [XmlType("Event")]
    public abstract class IeEvent
    {
        // Defines the type of Event
        [XmlIgnore]
        public abstract EventType EventType { get; }

        [XmlElement(IsNullable = false)]
        public string Name { get; set; }

        [XmlElement(IsNullable = false)]
        public string Description { get; set; }


        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("Triggers")]
        [XmlArrayItem("Trigger", typeof(IeTrigger))]
        public List<IeTrigger> Triggers { get; set; }
    }
}