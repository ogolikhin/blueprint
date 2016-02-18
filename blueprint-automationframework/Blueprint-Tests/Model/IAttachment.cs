namespace Model
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")] // Ignore this warning.
    public interface IAttachment
    {
        // TODO Future development
    }

    public interface IOpenApiAttachment
    {
        int Id { get; set; }
        string FileName { get; set; }
        string Link { get; set; }
        bool IsReadOnly { get; set; }
    }
}
