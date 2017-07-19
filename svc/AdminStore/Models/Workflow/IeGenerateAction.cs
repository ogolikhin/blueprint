using System.Xml.Serialization;
using ServiceLibrary.Models.Enums;

namespace AdminStore.Models.Workflow
{
    /// <summary>
    /// Generate Action 
    /// </summary>
    [XmlType("GenerateAction")]
    public class IeGenerateAction : IeBaseAction
    {
        [XmlElement(IsNullable = false)]
        
        public GenerateActionTypes GenerateActionType { get; set; }

        // Used only for GenerateActionType = Children
        [XmlElement]
        public int? ChildCount { get; set; }
        public bool ShouldSerializeChildCount() { return ChildCount.HasValue; }

        // Used only for GenerateActionType = Children
        [XmlElement(IsNullable = false)]
        public string ArtifactType { get; set; }
    }
}