﻿using System.Collections.Generic;
using Model.OpenApiModel;

namespace Model
{
    public interface IBaselinesAndReviews : IArtifactBase
    {
        IAuthorHistory AuthorHistory { get; set; }
        List<IBaseline> Baselines { get; }
        List<IBaselineAndReviewFolder> BRFolders { get; }
        List<IReview> Reviews { get; }
    }
}
