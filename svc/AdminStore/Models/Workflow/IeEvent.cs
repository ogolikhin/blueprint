using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using AdminStore.Helpers.Workflow;

namespace AdminStore.Models.Workflow
{
    public enum EventTypes { None, Transition, PropertyChange, NewArtifact }

    // !!! Updating of this class requires regenerating of the xml schema IeWorkflow.xsd is required, see below:
    // !!! xsd.exe AdminStore.dll /t:IeWorkflow
    /// <summary>
    /// Base class for Triggers of specific type
    /// </summary>
    [XmlType("Event")]
    public abstract class IeEvent : IIeWorkflowEntityWithId
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

        #region Generated and modified Equals and GetHashCode methods

        protected bool Equals(IeEvent other)
        {
            return Id.GetValueOrDefault() == other.Id.GetValueOrDefault() && string.Equals(Name, other.Name) && WorkflowHelper.CollectionEquals(Triggers, other.Triggers);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IeEvent)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id.GetHashCode();
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Triggers != null ? Triggers.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
    }
}