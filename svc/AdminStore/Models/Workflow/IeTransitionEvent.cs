using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using AdminStore.Helpers.Workflow;

namespace AdminStore.Models.Workflow
{
    // !!! Updating of this class requires regenerating of the xml schema IeWorkflow.xsd is required, see below:
    // !!! xsd.exe AdminStore.dll /t:IeWorkflow
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

        [XmlElement]
        public bool? SkipPermissionGroups { get; set; }
        public bool ShouldSerializeSkipPermissionGroups() { return SkipPermissionGroups.HasValue; }

        #region Generated and modified Equals and GetHashCode methods

        protected bool Equals(IeTransitionEvent other)
        {
            return base.Equals(other) && string.Equals(FromState, other.FromState) && FromStateId.GetValueOrDefault() == other.FromStateId.GetValueOrDefault() && string.Equals(ToState, other.ToState) && ToStateId.GetValueOrDefault() == other.ToStateId.GetValueOrDefault() && WorkflowHelper.CollectionEquals(PermissionGroups, other.PermissionGroups) && SkipPermissionGroups.GetValueOrDefault() == other.SkipPermissionGroups.GetValueOrDefault();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IeTransitionEvent)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (FromState != null ? FromState.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ FromStateId.GetHashCode();
                hashCode = (hashCode * 397) ^ (ToState != null ? ToState.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ ToStateId.GetHashCode();
                hashCode = (hashCode * 397) ^ (PermissionGroups != null ? PermissionGroups.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ SkipPermissionGroups.GetHashCode();
                return hashCode;
            }
        }

        #endregion
    }
}