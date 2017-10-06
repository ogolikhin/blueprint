namespace AdminStore.Models.Workflow
{
    public class UpdateWorkflowDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public StatusUpdate Status { get; set; }
    }
}
