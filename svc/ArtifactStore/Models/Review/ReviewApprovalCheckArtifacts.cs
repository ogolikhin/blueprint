using System.Collections.Generic;

namespace ArtifactStore.Models.Review
{
    public class ReviewApprovalCheckArtifacts
    {
        public ReviewArtifactApprovalCheck ReviewApprovalCheck { get; set; }
        public IEnumerable<int> ValidArtifactIds { get; set; }
    }
}