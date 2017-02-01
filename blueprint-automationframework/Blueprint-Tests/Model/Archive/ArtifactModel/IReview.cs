using Model.ArtifactModel;

namespace Model
{
    public interface IReview : IArtifactBase
    {
        IAuthorHistory AuthorHistory { get; set; }
    }
}
