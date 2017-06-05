namespace ServiceLibrary.Models
{
    public interface ITrigger
    {
        int Id { get; set; }

        string Name { get; set; }

        string Description { get; set; }
    }
}
