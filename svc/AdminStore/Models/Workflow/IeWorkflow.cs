using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using AdminStore.Helpers.Workflow;

namespace AdminStore.Models.Workflow
{
    public interface IIeWorkflowEntityWithId
    {
        int? Id { get; }
    }


    // !!! Updating of this class requires regenerating of the xml schema IeWorkflow.xsd is required, see below:
    // !!! xsd.exe AdminStore.dll /t:IeWorkflow
    // Workflow for Import/Export
    [XmlRoot("Workflow")]
    [XmlType("Workflow")]
    public class IeWorkflow : IIeWorkflowEntityWithId
    {
        // Optional, not used for the import, will be used for the update
        // ========================================================
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
        // ========================================================

        [XmlElement(IsNullable = false)]
        public string Name { get; set; }

        [XmlElement(IsNullable = false)]
        public string Description { get; set; }

        [XmlIgnore]
        public bool IsActive { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("States"), XmlArrayItem("State")]
        public List<IeState> States { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("Transitions"), XmlArrayItem("Transition")]
        public List<IeTransitionEvent> TransitionEvents { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("PropertyChanges"), XmlArrayItem("PropertyChange")]
        public List<IePropertyChangeEvent> PropertyChangeEvents { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("NewArtifacts"), XmlArrayItem("NewArtifact")]
        public List<IeNewArtifactEvent> NewArtifactEvents { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("Projects"), XmlArrayItem("Project")]
        public List<IeProject> Projects { get; set; }

        #region Generated and modified Equals and GetHashCode methods

        protected bool Equals(IeWorkflow other)
        {
            return Id.GetValueOrDefault() == other.Id.GetValueOrDefault() && string.Equals(Name, other.Name) && string.Equals(Description, other.Description) && IsActive == other.IsActive && WorkflowHelper.CollectionEquals(States, other.States) && WorkflowHelper.CollectionEquals(TransitionEvents, other.TransitionEvents) && WorkflowHelper.CollectionEquals(PropertyChangeEvents, other.PropertyChangeEvents) && WorkflowHelper.CollectionEquals(NewArtifactEvents, other.NewArtifactEvents) && WorkflowHelper.CollectionEquals(Projects, other.Projects);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IeWorkflow)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id.GetHashCode();
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Description != null ? Description.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IsActive.GetHashCode();
                hashCode = (hashCode * 397) ^ (States != null ? States.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TransitionEvents != null ? TransitionEvents.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (PropertyChangeEvents != null ? PropertyChangeEvents.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (NewArtifactEvents != null ? NewArtifactEvents.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Projects != null ? Projects.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
    }
}