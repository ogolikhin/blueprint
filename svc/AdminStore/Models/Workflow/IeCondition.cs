using System.Xml.Serialization;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Models.Workflow
{
    // !!! Updating of this class requires regenerating of the xml schema IeWorkflow.xsd is required, see below:
    // !!! xsd.exe AdminStore.dll /t:IeWorkflow
    [XmlType("Condition")]
    public abstract class IeCondition
    {
        [XmlIgnore]
        public abstract ConditionTypes ConditionType { get; }
    }
}