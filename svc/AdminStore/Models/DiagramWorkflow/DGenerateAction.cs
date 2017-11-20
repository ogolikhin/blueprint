using ServiceLibrary.Models.Enums;

namespace AdminStore.Models.DiagramWorkflow
{
    public class DGenerateAction : DBaseAction
    {
        public override ActionTypes ActionType => ActionTypes.Generate;
        public GenerateActionTypes GenerateActionType { get; set; }
        public int? ChildCount { get; set; }
        public string ArtifactType { get; set; }
        public int? ArtifactTypeId { get; set; }
    }
}