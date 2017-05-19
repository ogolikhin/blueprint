namespace ServiceLibrary.Models.Workflow
{
    public class WorkflowState : BaseInformation
    {
        public int VersionId { get; set; }

        public int WorkflowId { get; set; }

        public bool IsDefault { get; set; }
    }
}