namespace Model.Impl
{
    public class Comment : IComment
    {

    }

    public class OpenApiComment : Comment, IOpenApiComment
    {
        public string LastModified { get; set; }
        public bool IsClosed { get; set; }
        public string Status { get; set; }
        public int Id { get; set; }
        public IAuthor Author { get; set; }
        public int Version { get; set; }
        public string Description { get; set; }
    }
}
