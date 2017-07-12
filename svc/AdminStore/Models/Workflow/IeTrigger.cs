using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    public enum TriggerTypes { None, Transition, PropertyChange };

    /// <summary>
    /// Base class for Triggers of specific type
    /// </summary>
    [XmlType("Trigger")]
    public abstract class IeTrigger
    {
        // Defines the type of Trigger
        [XmlIgnore]
        public abstract TriggerTypes TriggerType { get; }

        [XmlElement(IsNullable = false)]
        public string Name { get; set; }

        [XmlElement(IsNullable = false)]
        public string Description { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("Actions")]
        [XmlArrayItem("EmailNotificationAction", typeof(IeEmailNotificationAction))]
        [XmlArrayItem("PropertyChangeAction", typeof(IePropertyChangeAction))]
        [XmlArrayItem("GenerateAction", typeof(IeGenerateAction))]
        public List<IeBaseAction> Actions { get; set; }
    }
}