using System.Xml.Serialization;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Models.Workflow
{
    // !!! Updating of this class requires regenerating of the xml schema IeWorkflow.xsd is required, see below:
    // !!! xsd.exe AdminStore.dll /t:IeWorkflow
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
        public int? ArtifactTypeId { get; set; }
        public bool ShouldSerializeArtifactTypeId() { return ArtifactTypeId.HasValue; }
    }
}