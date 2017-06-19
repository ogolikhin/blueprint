namespace AdminStore.Models.Workflow
{
    public class DState
    {
        public int VersionId { get; set; }

        public int WorkflowStateId { get; set; }

        public int WorkflowId { get; set; }

        public int StartRevision { get; set; }

        public int EndRevision { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool Default { get; set; }

        public float OrderIndex { get; set; }
    }
}