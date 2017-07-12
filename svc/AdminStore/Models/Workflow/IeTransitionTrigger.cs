using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    /// <summary>
    /// Transition Trigger
    /// </summary>
    [XmlType("TransitionTrigger")]
    public class IeTransitionTrigger : IeTrigger
    {
        [XmlIgnore]
        public override TriggerTypes TriggerType => TriggerTypes.Transition;

        [XmlElement(IsNullable = false)]
        public string FromState { get; set; }

        [XmlElement(IsNullable = false)]
        public string ToState { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("PermissionGroups"), XmlArrayItem("Group")]
        public List<IeGroup> PermissionGroups { get; set; }
    }
}