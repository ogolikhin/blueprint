using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    [XmlType("AG")]
    public class XmlGenerateAction : XmlAction
    {
        [XmlElement("T")]
        public GenerateActionTypes GenerateActionType { get; set; }

        // Used only for GenerateActionType = Children
        [XmlElement("CC")]
        public int? ChildCount { get; set; }
        public bool ShouldSerializeChildCount() { return ChildCount.HasValue; }

        // Used only for GenerateActionType = Children
        [XmlElement("AID")]
        public int ArtifactTypeId { get; set; }
    }
}