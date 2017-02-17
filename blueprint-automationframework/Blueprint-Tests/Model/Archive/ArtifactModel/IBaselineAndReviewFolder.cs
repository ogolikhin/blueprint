using System.Collections.Generic;
using Model.ArtifactModel;

namespace Model.Archive.ArtifactModel
{
    public interface IBaselineAndReviewFolder : IArtifactBase
    {
        IAuthorHistory AuthorHistory { get; set; }
        List<IBaseline> Baselines { get; }
        List<IBaselineAndReviewFolder> BRFolders { get; }
        List<IReview> Reviews { get; }
    }
}
