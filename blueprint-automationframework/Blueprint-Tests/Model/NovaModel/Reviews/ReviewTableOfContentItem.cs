using Model.NovaModel.Reviews.Enum;

namespace Model.NovaModel.Reviews
{
    // Taken from blueprint/svc/ArtifactStore/Models/Review/ReviewTableOfContent.cs
    public class ReviewTableOfContentItem
    {
        public ApprovalType ApprovalStatus { get; set; }
        public bool InReview { get; set; }
    }
}
