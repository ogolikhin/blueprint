using System;

namespace AdminStore.Models.Workflow
{
    public class SqlWorkflow
    {
        public int VersionId { get; set; }

        public int WorkflowId { get; set; }

        public int StartRevision { get; set; }

        public int EndRevision { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool Active { get; set; }

        public string LastModifiedBy { get; set; }

        public DateTime LastModified { get; set; }
    }

    public class SqlWorkflowMapItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}