using System.Xml.Serialization;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Models.Workflow
{
    [XmlType("StateCondition")]
    public class IeStateCondition : IeCondition
    {
        [XmlIgnore]
        public override ConditionTypes ConditionType => ConditionTypes.State;

        [XmlElement(IsNullable = false)]
        public string State { get; set; }
    }
}