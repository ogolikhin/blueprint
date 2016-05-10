namespace Model.ArtifactModel.Impl
{
    public class Comment
    {
        public string LastModified { get; set; }
        public bool IsClosed { get; set; }
        public string Status { get; set; }
        public int Id { get; set; }
        public Author Author { get; set; }
        public int Version { get; set; }
        public string Description { get; set; }
    }
}
