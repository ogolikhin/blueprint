
namespace Model.ArtifactModel
{
    public interface IArtifactInfo
    {
        int Id { get; set; }
        int ProjectId { get; set; }
        string Name { get; set; }
        string TypePrefix { get; set; }
        int BaseTypePredefined { get; set; }
        string Link { get; set; }
    }
}
