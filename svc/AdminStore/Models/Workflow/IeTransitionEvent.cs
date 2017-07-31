using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    /// <summary>
    /// Transition Trigger
    /// </summary>
    [XmlType("Transition")]
    public class IeTransitionEvent : IeEvent
    {
        [XmlIgnore]
        public override EventTypes EventType => EventTypes.Transition;

        [XmlElement(IsNullable = false)]
        public string FromState { get; set; }

        // Optional, not used for the import, will be used for the update
        [XmlElement]
        public int? FromStateId { get; set; }
        public bool ShouldSerializeFromStateId() { return FromStateId.HasValue; }

        [XmlElement(IsNullable = false)]
        public string ToState { get; set; }

        // Optional, not used for the import, will be used for the update
        [XmlElement]
        public int? ToStateId { get; set; }
        public bool ShouldSerializeToStateId() { return ToStateId.HasValue; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("PermissionGroups"), XmlArrayItem("Group")]
        public List<IeGroup> PermissionGroups { get; set; }
    }
}