using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArtifactStore.Models.Review
{
    public class ReviewApprovalCheckArtifacts
    {
        public ReviewArtifactApprovalCheck ReviewApprovalCheck { get; set; }
        public IEnumerable<int> ValidArtifactIds { get; set; }
    }
}