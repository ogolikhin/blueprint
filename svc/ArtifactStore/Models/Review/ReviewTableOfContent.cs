using System.Collections.Generic;

namespace ArtifactStore.Models.Review
{
    public class ReviewTableOfContentItem : BaseReviewArtifact
    {
        public ApprovalType ApprovalStatus { get; set; }
        public int Level { get; set; }
        public bool InReview { get; set; }

    }

    public class ReviewTableOfContent
    {
        public IEnumerable<ReviewTableOfContentItem> Items { get; set; }
        public int Total { get; set; }
    }
}