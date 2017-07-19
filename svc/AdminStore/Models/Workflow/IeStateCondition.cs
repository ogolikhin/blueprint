using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    [XmlType("StateCondition")]
    public class IeStateCondition : IeCondition
    {
        [XmlElement(IsNullable = false)]
        public string State { get; set; }
    }
}