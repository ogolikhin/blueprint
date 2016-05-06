namespace Model.ArtifactModel
{
 public interface IAttachment
    {
        int Id { get; set; }
        string FileName { get; set; }
        string Link { get; set; }
        bool IsReadOnly { get; set; }
    }
}
