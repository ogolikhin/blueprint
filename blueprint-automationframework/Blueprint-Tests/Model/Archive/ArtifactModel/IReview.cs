using Model.ArtifactModel;

namespace Model.Archive.ArtifactModel
{
    public interface IReview : IArtifactBase
    {
        IAuthorHistory AuthorHistory { get; set; }
    }
}
