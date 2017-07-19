namespace ServiceLibrary.Models
{
    public interface IWorkflowEvent
    {
        int Id { get; set; }

        string Name { get; set; }

        string Description { get; set; }
    }
}
