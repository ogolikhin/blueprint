using System.Xml.Serialization;

namespace ServiceLibrary.Models.Workflow
{
    public enum ConditionTypes { State }

    [XmlType("C")]
    public abstract class XmlCondition
    {
        [XmlIgnore]
        public abstract ConditionTypes ConditionType { get; }
    }
}