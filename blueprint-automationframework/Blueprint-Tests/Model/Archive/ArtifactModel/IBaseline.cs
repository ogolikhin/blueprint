using Model.ArtifactModel;

namespace Model.Archive.ArtifactModel
{
    public interface IBaseline : IArtifactBase
    {
        IAuthorHistory AuthorHistory { get; set; }
    }
}
