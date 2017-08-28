namespace ServiceLibrary.Models.Workflow
{
    public class SqlWorkflowNewArtifactEvent
    {
        public int ItemTypeId { get; set; }
        public int? InstanceItemTypeId { get; set; }
        public string ItemTypeName { get; set; }
        public int WorkflowId { get; set; }
        public int WorkflowEventId { get; set; }
        public int WorkflowEventName { get; set; }
        public int WorkflowEventPermissions { get; set; }
        public string Triggers { get; set; }
        public int WorkflowEventStartRevision { get; set; }
        public int WorkflowEventEndRevision { get; set; }
        public int WorkflowEventVersionId { get; set; }
    }
}
