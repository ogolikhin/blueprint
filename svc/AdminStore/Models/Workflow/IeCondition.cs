using System.Xml.Serialization;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Models.Workflow
{
    [XmlType("Condition")]
    public abstract class IeCondition
    {
        [XmlIgnore]
        public abstract ConditionTypes ConditionType { get; }
    }
}