using System.Xml.Serialization;

namespace ServiceLibrary.Models.Workflow
{
    public enum ActionTypes { EmailNotification, PropertyChange, Generate }

    [XmlType("A")]
    public abstract class XmlAction
    {
        [XmlIgnore]
        public abstract ActionTypes ActionType { get; }

        [XmlElement("N", IsNullable = false)]
        public string Name { get; set; }
    }
}