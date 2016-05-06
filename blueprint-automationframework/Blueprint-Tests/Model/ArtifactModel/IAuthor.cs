namespace Model.ArtifactModel
{
    public interface IAuthor
    {
        string Type { get; set; }
        int Id { get; set; }
        string DisplayName { get; set; }
    }
}
