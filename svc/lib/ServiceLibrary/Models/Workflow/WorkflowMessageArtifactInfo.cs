using ServiceLibrary.Models.VersionControl;

namespace ServiceLibrary.Models.Workflow
{
    public class WorkflowMessageArtifactInfo : IBaseArtifactVersionControlInfo
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int ProjectId { get; set; }

        public int ItemTypeId { get; set; }

        public ItemTypePredefined PredefinedType { get; set; }

        public string ProjectName { get; set; }
    }
}
