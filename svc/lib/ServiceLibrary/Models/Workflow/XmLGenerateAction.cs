using System.Xml.Serialization;
using ServiceLibrary.Models.Enums;

namespace ServiceLibrary.Models.Workflow
{
    [XmlType("AG")]
    public class XmlGenerateAction : XmlAction
    {
        [XmlIgnore]
        public override ActionTypes ActionType => ActionTypes.Generate;

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