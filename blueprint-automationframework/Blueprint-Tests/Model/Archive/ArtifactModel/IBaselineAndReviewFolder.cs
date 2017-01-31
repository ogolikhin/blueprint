using Model.ArtifactModel;
using System.Collections.Generic;

namespace Model
{
    public interface IBaselineAndReviewFolder : IArtifactBase
    {
        IAuthorHistory AuthorHistory { get; set; }
        List<IBaseline> Baselines { get; }
        List<IBaselineAndReviewFolder> BRFolders { get; }
        List<IReview> Reviews { get; }
    }
}
