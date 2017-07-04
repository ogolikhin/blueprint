namespace AdminStore.Models.Workflow
{
    public class SqlTrigger
    {
        public int VersionId { get; set; }

        public int TriggerId { get; set; }

        public int? WorkflowId { get; set; }

        public int StartRevision { get; set; }

        public int EndRevision { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DTriggerType Type { get; set; }

        public string Permissions { get; set; }

        public string Validations { get; set; }

        public string Actions { get; set; }

        public int? ProjectId { get; set; }

        public int? WorkflowState1Id { get; set; }

        public int? WorkflowState2Id { get; set; }

        public int? PropertyTypeId { get; set; }
    }

    public enum DTriggerType
    {
        Transition = 0
    }
}