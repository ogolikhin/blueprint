
namespace Model.NovaModel.Reviews
{
    // see blueprint/svc/ArtifactStore/Models/Review/ReviewArtifactsResult.cs
    /// <summary>
    /// Artifact representation for Review Editor experience
    /// </summary>
    public class ReviewArtifact : BaseReviewArtifact
    {
        public bool IsApprovalRequired { get; set; }
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Disapproved { get; set; }
        public int Viewed { get; set; }
        public int Unviewed { get; set; }
    }
}
