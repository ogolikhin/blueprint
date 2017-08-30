namespace ServiceLibrary.Models.Workflow
{
    public interface IWorkflowEvent
    {
        int Id { get; set; }

        string Name { get; set; }
    }
}
