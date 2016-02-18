namespace Model.Impl
{
    public class OpenApiComment : IOpenApiComment
    {
        public string LastModified { get; set; }
        public bool IsClosed { get; set; }
        public string Status { get; set; }
        public int Id { get; set; }
        public IOpenApiAuthor Author { get; set; }
        public int Version { get; set; }
        public string Description { get; set; }
    }
}
