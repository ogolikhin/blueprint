using System.Xml.Serialization;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Models.Workflow
{
    // !!! Updating of this class requires regenerating of the xml schema IeWorkflow.xsd is required, see below:
    // !!! xsd.exe AdminStore.dll /t:IeWorkflow
    [XmlType("StateCondition")]
    public class IeStateCondition : IeCondition
    {
        [XmlIgnore]
        public override ConditionTypes ConditionType => ConditionTypes.State;

        [XmlElement(IsNullable = false)]
        public string State { get; set; }
    }
}