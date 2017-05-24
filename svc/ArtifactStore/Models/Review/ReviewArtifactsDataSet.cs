using System;

namespace ArtifactStore.Models.Review
{
    public class ReviewedArtifact : BaseReviewArtifact
    {
        public ViewStateType ViewState { get; set; }

        public string Approval { get; set; }

        public ApprovalType ApprovalFlag { get; set; }

        public int ArtifactVersion { get; set; }

        public DateTime PublishedOnTimestamp { get; set; }

        public string UserDisplayName { get; set; }
        /// <summary>
        /// Viewed artifact version
        /// </summary>
        public int? ViewedArtifactVersion { get; set; }

        /// <summary>
        /// e-signed by UserId on that UTC date time
        /// </summary>
        public DateTime? ESignedOn { get; set; }
    }

    public class ReviewArtifactsDataSet : BaseReviewArtifactsContent<ReviewedArtifact>
    {
    }
}