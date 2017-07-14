﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    public enum TriggerType { None, Transition, PropertyChange }

    [XmlType("Trigger")]
    public abstract class IeTrigger
    {
        // Defines the type of Trigger
        [XmlIgnore]
        public abstract TriggerType TriggerType { get; }

        [XmlElement(IsNullable = false)]
        public string Name { get; set; }

        [XmlElement(IsNullable = false)]
        public string Description { get; set; }
    }
}