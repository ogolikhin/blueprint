namespace ServiceLibrary.Models.Workflow
{
    public class SqlWorkFlowStateInformation : SqlWorkFlowState
    {
        public string WorkflowName { get; set; }
        public int ProjectId { get; set; }
        public int ArtifactId { get; set; }
        public int ItemId { get; set; }
        public string Name { get; set; }
        public int ItemTypeId { get; set; }
        public int StartRevision { get; set; }
        public int EndRevision { get; set; }
        public int? LockedByUserId { get; set; }
    }
}
