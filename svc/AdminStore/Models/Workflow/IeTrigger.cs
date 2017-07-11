using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    public enum TriggerTypes { None, Transition, PropertyChange };

    /// <summary>
    /// Base class for Triggers of specific type
    /// </summary>
    public abstract class IeTrigger
    {
        public IeTrigger(TriggerTypes type)
        {
            TriggerType = type;
        }

        // Defines the type of Trigger
        protected TriggerTypes TriggerType { get; set; }

        [XmlElement(IsNullable = false)]
        public string Name { get; set; }

        [XmlElement(IsNullable = false)]
        public string Description { get; set; }

        [XmlElement(IsNullable = false)]
        public string FromState { get; set; }

        [XmlElement(IsNullable = false)]
        public string ToState { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("Actions")]
        [XmlArrayItem("NotificationAction", typeof(IeNotificationAction))]
        [XmlArrayItem("PropertyChangeAction", typeof(IePropertyChangeAction))]
        [XmlArrayItem("GenerateAction", typeof(IeGenerateAction))]
        public List<IeBaseAction> Actions { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("PermissionGroups"), XmlArrayItem("Group")]
        public List<IeGroup> PermissionGroups { get; set; }
    }
}