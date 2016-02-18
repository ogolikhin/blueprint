namespace Model
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")] // Ignore this warning.
    public interface IComment
    {
        // TODO Future development
    }

    public interface IOpenApiComment
    {
        string LastModified { get; set; }
        bool IsClosed { get; set; }
        string Status { get; set; }
        int Id { get; set; }
        IOpenApiAuthor Author { get; set; }
        int Version { get; set; }
        string Description { get; set; }
    }
}
