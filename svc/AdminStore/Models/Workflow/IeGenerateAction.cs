using System.Xml.Serialization;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Models.Workflow
{
    /// <summary>
    /// Generate Action 
    /// </summary>
    [XmlType("GenerateAction")]
    public class IeGenerateAction : IeBaseAction
    {
        [XmlIgnore]
        public override ActionTypes ActionType => ActionTypes.Generate;

        [XmlElement(IsNullable = false)]
        
        public GenerateActionTypes GenerateActionType { get; set; }

        // Used only for GenerateActionType = Children
        [XmlElement]
        public int? ChildCount { get; set; }
        public bool ShouldSerializeChildCount() { return ChildCount.HasValue; }

        // Used only for GenerateActionType = Children
        [XmlElement(IsNullable = false)]
        public string ArtifactType { get; set; }

        // Used only for GenerateActionType = Children
        // Optional, not used for the import, will be used for the update
        [XmlElement]
        public int? ArtifactId { get; set; }
        public bool ShouldSerializeArtifactId() { return ArtifactId.HasValue; }
    }
}