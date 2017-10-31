namespace AdminStore.Models.Workflow
{
    public class UpdateWorkflowDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Status { get; set; }
        public int VersionId { get; set; }
    }
}
