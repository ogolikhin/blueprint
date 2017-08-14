using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    public enum EventTypes { None, Transition, PropertyChange, NewArtifact }

    // !!! Updating of this class requires regenerating of the xml schema IeWorkflow.xsd is required, see below:
    // !!! xsd.exe AdminStore.dll /t:IeWorkflow
    /// <summary>
    /// Base class for Triggers of specific type
    /// </summary>
    [XmlType("Event")]
    public abstract class IeEvent
    {
        // Optional, not used for the import, will be used for the update
        //========================================================
        // To make xml attribute nullable.
        [XmlIgnore]
        public int? Id { get; set; }

        [XmlAttribute("Id")]
        public int IdSerializable
        {
            get { return Id.GetValueOrDefault(); }
            set { Id = value; }
        }

        public bool ShouldSerializeIdSerializable()
        {
            return Id.HasValue;
        }
        //========================================================

        // Defines the type of Event
        [XmlIgnore]
        public abstract EventTypes EventType { get; }

        [XmlElement(IsNullable = false)]
        public string Name { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("Triggers")]
        [XmlArrayItem("Trigger", typeof(IeTrigger))]
        public List<IeTrigger> Triggers { get; set; }
    }
}