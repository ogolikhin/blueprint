﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    // Workflow for Import/Export
    [XmlRoot("Workflow")]
    [XmlType("Workflow")]
    public class IeWorkflow
    {
        // Optional, not used for the import, can be used for the update
        //[XmlElement]
        //public int? Id { get; set; }
        //public bool ShouldSerializeId() { return Id.HasValue; }

        [XmlElement(IsNullable = false)]
        public string Name { get; set; }

        [XmlElement(IsNullable = false)]
        public string Description { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("States"), XmlArrayItem("State")]
        public List<IeState> States { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("Transitions")]
        [XmlArrayItem("Transition", typeof(IeTransitionEvent))]
        public List<IeTransitionEvent> TransitionEvents { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("PropertyChanges")]
        [XmlArrayItem("PropertyChange", typeof(IePropertyChangeEvent))]
        public List<IePropertyChangeEvent> PropertyChangeEvents { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("NewArtifacts")]
        [XmlArrayItem("NewArtifact", typeof(IeNewArtifactEvent))]
        public List<IeNewArtifactEvent> NewArtifactsEvents { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("Projects"), XmlArrayItem("Project")]
        public List<IeProject> Projects { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("ArtifactTypes"), XmlArrayItem("ArtifactType")]
        public List<IeArtifactType> ArtifactTypes { get; set; }
    }
}