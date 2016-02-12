namespace Model
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")] // Ignore this warning.
    public interface IAttachment
    {

    }

    public interface IOpenApiAttachment : IAttachment
    {
        int Id { get; set; }
        string FileName { get; set; }
        string Link { get; set; }
        bool IsReadOnly { get; set; }
    }
}
